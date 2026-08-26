#define WIN32_LEAN_AND_MEAN
#define NOMINMAX
#include <windows.h>
#include <shobjidl_core.h>

#include <cwchar>

namespace
{
constexpr CLSID kConvertyCommandClsid = {
    0x20e7c5c1, 0x3e5f, 0x4d0f, {0x9c, 0x56, 0x2e, 0x9f, 0x2a, 0x97, 0x8a, 0x10}};

int Fail(int code, HRESULT result) noexcept
{
    return code | (static_cast<int>(result) == 0 ? 0 : 0x100);
}
} // namespace

int wmain()
{
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
    command->Release();
    CoUninitialize();

    return childTitleValid ? 0 : Fail(7, result);
}
