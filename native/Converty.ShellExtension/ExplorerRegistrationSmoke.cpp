#define WIN32_LEAN_AND_MEAN
#define NOMINMAX
#include <windows.h>
#include <shobjidl_core.h>

#include <cwchar>
#include <string>

namespace
{
constexpr CLSID kConvertyCommandClsid = {
    0x20e7c5c1, 0x3e5f, 0x4d0f, {0x9c, 0x56, 0x2e, 0x9f, 0x2a, 0x97, 0x8a, 0x10}};

constexpr DWORD kInvokeTimeoutMilliseconds = 30000;
constexpr DWORD kPollIntervalMilliseconds = 200;
constexpr unsigned int kRequiredStablePolls = 5;

int Fail(int code, HRESULT result) noexcept
{
    return code | (result == S_OK ? 0 : 0x100);
}

HRESULT CreateSingleSelection(const wchar_t* path, IShellItemArray** selection) noexcept
{
    if (path == nullptr || selection == nullptr)
    {
        return E_POINTER;
    }
    *selection = nullptr;

    IShellItem* item = nullptr;
    HRESULT result = SHCreateItemFromParsingName(
        path,
        nullptr,
        IID_IShellItem,
        reinterpret_cast<void**>(&item));
    if (FAILED(result))
    {
        return result;
    }

    result = SHCreateShellItemArrayFromShellItem(
        item,
        IID_IShellItemArray,
        reinterpret_cast<void**>(selection));
    item->Release();
    return result;
}

HRESULT FindEnabledMp3Command(
    IExplorerCommand* root,
    IShellItemArray* selection,
    IExplorerCommand** command) noexcept
{
    if (root == nullptr || selection == nullptr || command == nullptr)
    {
        return E_POINTER;
    }
    *command = nullptr;

    IEnumExplorerCommand* enumerator = nullptr;
    HRESULT result = root->EnumSubCommands(&enumerator);
    if (FAILED(result) || enumerator == nullptr)
    {
        return FAILED(result) ? result : E_FAIL;
    }

    for (;;)
    {
        IExplorerCommand* child = nullptr;
        ULONG fetched = 0;
        result = enumerator->Next(1, &child, &fetched);
        if (result == S_FALSE)
        {
            enumerator->Release();
            return HRESULT_FROM_WIN32(ERROR_NOT_FOUND);
        }
        if (FAILED(result) || fetched != 1 || child == nullptr)
        {
            if (child != nullptr)
            {
                child->Release();
            }
            enumerator->Release();
            return FAILED(result) ? result : E_FAIL;
        }

        LPWSTR title = nullptr;
        const HRESULT titleResult = child->GetTitle(selection, &title);
        EXPCMDSTATE state = ECS_DISABLED;
        const HRESULT stateResult = child->GetState(selection, TRUE, &state);
        const bool matches = SUCCEEDED(titleResult)
            && title != nullptr
            && std::wcscmp(title, L"Convert to MP3") == 0
            && SUCCEEDED(stateResult)
            && state == ECS_ENABLED;
        CoTaskMemFree(title);

        if (matches)
        {
            *command = child;
            enumerator->Release();
            return S_OK;
        }

        child->Release();
    }
}

std::wstring BuildMp3OutputPath(const wchar_t* inputPath)
{
    std::wstring output(inputPath == nullptr ? L"" : inputPath);
    const std::size_t separator = output.find_last_of(L"\\/");
    const std::size_t dot = output.find_last_of(L'.');
    if (dot == std::wstring::npos || (separator != std::wstring::npos && dot < separator))
    {
        output.append(L".mp3");
    }
    else
    {
        output.replace(dot, std::wstring::npos, L".mp3");
    }
    return output;
}

bool TryGetNonEmptyFileSize(const std::wstring& path, ULONGLONG* size) noexcept
{
    if (size == nullptr)
    {
        return false;
    }
    *size = 0;

    WIN32_FILE_ATTRIBUTE_DATA attributes{};
    if (!GetFileAttributesExW(path.c_str(), GetFileExInfoStandard, &attributes))
    {
        return false;
    }
    if ((attributes.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY) != 0)
    {
        return false;
    }

    ULARGE_INTEGER fileSize{};
    fileSize.HighPart = attributes.nFileSizeHigh;
    fileSize.LowPart = attributes.nFileSizeLow;
    *size = fileSize.QuadPart;
    return *size > 0;
}

bool WaitForStableNonEmptyFile(const std::wstring& path) noexcept
{
    ULONGLONG previousSize = 0;
    unsigned int stablePolls = 0;

    for (DWORD elapsed = 0; elapsed < kInvokeTimeoutMilliseconds; elapsed += kPollIntervalMilliseconds)
    {
        ULONGLONG currentSize = 0;
        if (TryGetNonEmptyFileSize(path, &currentSize))
        {
            if (currentSize == previousSize)
            {
                ++stablePolls;
                if (stablePolls >= kRequiredStablePolls)
                {
                    return true;
                }
            }
            else
            {
                previousSize = currentSize;
                stablePolls = 0;
            }
        }
        else
        {
            previousSize = 0;
            stablePolls = 0;
        }

        Sleep(kPollIntervalMilliseconds);
    }

    return false;
}

HRESULT ExerciseInvoke(IExplorerCommand* root, const wchar_t* inputPath)
{
    IShellItemArray* selection = nullptr;
    HRESULT result = CreateSingleSelection(inputPath, &selection);
    if (FAILED(result) || selection == nullptr)
    {
        return FAILED(result) ? result : E_FAIL;
    }

    IExplorerCommand* command = nullptr;
    result = FindEnabledMp3Command(root, selection, &command);
    if (FAILED(result) || command == nullptr)
    {
        selection->Release();
        return FAILED(result) ? result : E_FAIL;
    }

    const std::wstring outputPath = BuildMp3OutputPath(inputPath);
    if (GetFileAttributesW(outputPath.c_str()) != INVALID_FILE_ATTRIBUTES)
    {
        command->Release();
        selection->Release();
        return HRESULT_FROM_WIN32(ERROR_FILE_EXISTS);
    }

    result = command->Invoke(selection, nullptr);
    command->Release();
    selection->Release();
    if (FAILED(result))
    {
        return result;
    }

    return WaitForStableNonEmptyFile(outputPath)
        ? S_OK
        : HRESULT_FROM_WIN32(ERROR_TIMEOUT);
}
} // namespace

int wmain(int argc, wchar_t* argv[])
{
    if (argc < 1 || argc > 2)
    {
        return Fail(8, E_INVALIDARG);
    }

    const HRESULT initialize = CoInitializeEx(nullptr, COINIT_APARTMENTTHREADED);
    if (FAILED(initialize))
    {
        return Fail(1, initialize);
    }

    IExplorerCommand* command = nullptr;
    HRESULT result = CoCreateInstance(
        kConvertyCommandClsid,
        nullptr,
        CLSCTX_INPROC_SERVER | CLSCTX_LOCAL_SERVER,
        IID_IExplorerCommand,
        reinterpret_cast<void**>(&command));
    if (FAILED(result) || command == nullptr)
    {
        CoUninitialize();
        return Fail(2, result);
    }

    LPWSTR title = nullptr;
    result = command->GetTitle(nullptr, &title);
    if (FAILED(result) || title == nullptr || std::wcscmp(title, L"Converty") != 0)
    {
        CoTaskMemFree(title);
        command->Release();
        CoUninitialize();
        return Fail(3, result);
    }
    CoTaskMemFree(title);

    EXPCMDFLAGS flags = ECF_DEFAULT;
    result = command->GetFlags(&flags);
    if (FAILED(result) || (flags & ECF_HASSUBCOMMANDS) == 0)
    {
        command->Release();
        CoUninitialize();
        return Fail(4, result);
    }

    IEnumExplorerCommand* enumerator = nullptr;
    result = command->EnumSubCommands(&enumerator);
    if (FAILED(result) || enumerator == nullptr)
    {
        command->Release();
        CoUninitialize();
        return Fail(5, result);
    }

    IExplorerCommand* child = nullptr;
    ULONG fetched = 0;
    result = enumerator->Next(1, &child, &fetched);
    if (result != S_OK || fetched != 1 || child == nullptr)
    {
        enumerator->Release();
        command->Release();
        CoUninitialize();
        return Fail(6, result);
    }

    LPWSTR childTitle = nullptr;
    result = child->GetTitle(nullptr, &childTitle);
    const bool childTitleValid = SUCCEEDED(result) && childTitle != nullptr && childTitle[0] != L'\0';
    CoTaskMemFree(childTitle);
    child->Release();
    enumerator->Release();
    if (!childTitleValid)
    {
        command->Release();
        CoUninitialize();
        return Fail(7, result);
    }

    if (argc == 2)
    {
        result = ExerciseInvoke(command, argv[1]);
        if (FAILED(result))
        {
            command->Release();
            CoUninitialize();
            return Fail(9, result);
        }
    }

    command->Release();
    CoUninitialize();
    return 0;
}
