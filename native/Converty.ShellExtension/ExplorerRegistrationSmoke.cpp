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

using DllGetClassObjectFunction = HRESULT(STDAPICALLTYPE*)(REFCLSID, REFIID, LPVOID*);

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

HRESULT CreateCommandFromModule(
    const wchar_t* modulePath,
    IExplorerCommand** command,
    HMODULE* loadedModule) noexcept
{
    if (modulePath == nullptr || command == nullptr || loadedModule == nullptr)
    {
        return E_POINTER;
    }
    *command = nullptr;
    *loadedModule = nullptr;

    HMODULE module = LoadLibraryExW(
        modulePath,
        nullptr,
        LOAD_LIBRARY_SEARCH_DLL_LOAD_DIR | LOAD_LIBRARY_SEARCH_SYSTEM32);
    if (module == nullptr)
    {
        return HRESULT_FROM_WIN32(GetLastError());
    }

    FARPROC address = GetProcAddress(module, "DllGetClassObject");
    if (address == nullptr)
    {
        const DWORD error = GetLastError();
        FreeLibrary(module);
        return HRESULT_FROM_WIN32(error);
    }

#pragma warning(push)
#pragma warning(disable : 4191) // GetProcAddress is the required Win32 boundary for an exported COM entry point.
    const auto getClassObject = reinterpret_cast<DllGetClassObjectFunction>(address);
#pragma warning(pop)

    IClassFactory* factory = nullptr;
    HRESULT result = getClassObject(
        kConvertyCommandClsid,
        IID_IClassFactory,
        reinterpret_cast<void**>(&factory));
    if (FAILED(result) || factory == nullptr)
    {
        FreeLibrary(module);
        return FAILED(result) ? result : E_FAIL;
    }

    result = factory->CreateInstance(
        nullptr,
        IID_IExplorerCommand,
        reinterpret_cast<void**>(command));
    factory->Release();
    if (FAILED(result) || *command == nullptr)
    {
        FreeLibrary(module);
        return FAILED(result) ? result : E_FAIL;
    }

    *loadedModule = module;
    return S_OK;
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

HRESULT ValidateRootCommand(IExplorerCommand* command) noexcept
{
    if (command == nullptr)
    {
        return E_POINTER;
    }

    LPWSTR title = nullptr;
    HRESULT result = command->GetTitle(nullptr, &title);
    const bool titleValid = SUCCEEDED(result)
        && title != nullptr
        && std::wcscmp(title, L"Converty") == 0;
    CoTaskMemFree(title);
    if (!titleValid)
    {
        return FAILED(result) ? result : E_FAIL;
    }

    EXPCMDFLAGS flags = ECF_DEFAULT;
    result = command->GetFlags(&flags);
    if (FAILED(result) || (flags & ECF_HASSUBCOMMANDS) == 0)
    {
        return FAILED(result) ? result : E_FAIL;
    }

    IEnumExplorerCommand* enumerator = nullptr;
    result = command->EnumSubCommands(&enumerator);
    if (FAILED(result) || enumerator == nullptr)
    {
        return FAILED(result) ? result : E_FAIL;
    }

    IExplorerCommand* child = nullptr;
    ULONG fetched = 0;
    result = enumerator->Next(1, &child, &fetched);
    enumerator->Release();
    if (result != S_OK || fetched != 1 || child == nullptr)
    {
        if (child != nullptr)
        {
            child->Release();
        }
        return FAILED(result) ? result : E_FAIL;
    }

    LPWSTR childTitle = nullptr;
    result = child->GetTitle(nullptr, &childTitle);
    const bool childTitleValid = SUCCEEDED(result)
        && childTitle != nullptr
        && childTitle[0] != L'\0';
    CoTaskMemFree(childTitle);
    child->Release();
    return childTitleValid ? S_OK : (FAILED(result) ? result : E_FAIL);
}
} // namespace

int wmain(int argc, wchar_t* argv[])
{
    const bool directModuleMode = argc == 4 && std::wcscmp(argv[1], L"--module") == 0;
    const bool packagedMode = argc == 1 || argc == 2;
    if (!directModuleMode && !packagedMode)
    {
        return Fail(8, E_INVALIDARG);
    }

    const wchar_t* inputPath = directModuleMode ? argv[3] : (argc == 2 ? argv[1] : nullptr);

    const HRESULT initialize = CoInitializeEx(nullptr, COINIT_APARTMENTTHREADED);
    if (FAILED(initialize))
    {
        return Fail(1, initialize);
    }

    IExplorerCommand* command = nullptr;
    HMODULE loadedModule = nullptr;
    HRESULT result = directModuleMode
        ? CreateCommandFromModule(argv[2], &command, &loadedModule)
        : CoCreateInstance(
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

    result = ValidateRootCommand(command);
    if (FAILED(result))
    {
        command->Release();
        if (loadedModule != nullptr)
        {
            FreeLibrary(loadedModule);
        }
        CoUninitialize();
        return Fail(3, result);
    }

    if (inputPath != nullptr)
    {
        result = ExerciseInvoke(command, inputPath);
        if (FAILED(result))
        {
            command->Release();
            if (loadedModule != nullptr)
            {
                FreeLibrary(loadedModule);
            }
            CoUninitialize();
            return Fail(9, result);
        }
    }

    command->Release();
    if (loadedModule != nullptr)
    {
        FreeLibrary(loadedModule);
    }
    CoUninitialize();
    return 0;
}
