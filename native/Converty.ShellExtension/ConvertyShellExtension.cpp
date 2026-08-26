#define WIN32_LEAN_AND_MEAN
#define NOMINMAX
#include <windows.h>
#include <shobjidl_core.h>

#include <array>
#include <atomic>
#include <cstring>
#include <cwchar>
#include <new>
#include <string>
#include <string_view>

namespace
{
constexpr CLSID kConvertyCommandClsid = {
    0x20e7c5c1, 0x3e5f, 0x4d0f, {0x9c, 0x56, 0x2e, 0x9f, 0x2a, 0x97, 0x8a, 0x10}};

constexpr GUID kRootCanonicalName = {
    0xb371d7ca, 0x153d, 0x463d, {0x90, 0x10, 0xce, 0x73, 0xf4, 0x13, 0xeb, 0x81}};

constexpr DWORD kMaximumSelectedFiles = 1024;
constexpr std::size_t kMaximumCommandLineCharacters = 32760;

HMODULE g_module = nullptr;
std::atomic<long> g_objectCount{0};
std::atomic<long> g_serverLockCount{0};

enum class MediaKind
{
    Video,
    Audio,
    Image,
};

struct PresetDefinition
{
    const wchar_t* id;
    const wchar_t* title;
    MediaKind inputKind;
    const wchar_t* outputExtension;
    GUID canonicalName;
};

constexpr std::array<const wchar_t*, 9> kVideoExtensions = {
    L".mp4", L".mov", L".mkv", L".avi", L".webm", L".m4v", L".mpeg", L".mpg", L".wmv"};
constexpr std::array<const wchar_t*, 8> kAudioExtensions = {
    L".wav", L".flac", L".mp3", L".m4a", L".aac", L".ogg", L".opus", L".wma"};
constexpr std::array<const wchar_t*, 8> kImageExtensions = {
    L".png", L".jpg", L".jpeg", L".webp", L".bmp", L".gif", L".tif", L".tiff"};

constexpr std::array<PresetDefinition, 9> kPresets = {{
    {L"video.mp4.h264", L"Convert to MP4", MediaKind::Video, L".mp4",
     {0x213da29e, 0x3f2c, 0x4f7e, {0xad, 0x6d, 0x81, 0x8f, 0x9d, 0x62, 0x51, 0xa1}}},
    {L"video.webm.vp9", L"Convert to WebM", MediaKind::Video, L".webm",
     {0xd31e1d43, 0xd03c, 0x4b06, {0x87, 0xb6, 0xfb, 0x4c, 0x18, 0xe8, 0x88, 0x72}}},
    {L"extract.audio.mp3", L"Extract Audio to MP3", MediaKind::Video, L".mp3",
     {0x42d058a2, 0x6a1e, 0x4f17, {0xa3, 0x84, 0x28, 0x8d, 0x78, 0xf4, 0x5d, 0x18}}},
    {L"audio.mp3", L"Convert to MP3", MediaKind::Audio, L".mp3",
     {0xa048b346, 0x4a75, 0x4b68, {0xb2, 0x28, 0x86, 0xa9, 0xd6, 0x23, 0x17, 0x20}}},
    {L"audio.flac", L"Convert to FLAC", MediaKind::Audio, L".flac",
     {0x536f5ce1, 0x1763, 0x4db9, {0xbf, 0x61, 0x6f, 0xc4, 0x89, 0x10, 0xf8, 0x3e}}},
    {L"audio.wav", L"Convert to WAV", MediaKind::Audio, L".wav",
     {0x53c845e3, 0x54f3, 0x48dd, {0xb8, 0x35, 0x85, 0x89, 0x6d, 0xfd, 0x6f, 0xca}}},
    {L"image.png", L"Convert to PNG", MediaKind::Image, L".png",
     {0xf793c91d, 0x945e, 0x40aa, {0x84, 0x1f, 0xb9, 0xf9, 0x57, 0xf4, 0x2d, 0x61}}},
    {L"image.jpeg", L"Convert to JPEG", MediaKind::Image, L".jpg",
     {0x7a0aa0f0, 0x0cf5, 0x45c7, {0xa3, 0xaa, 0xca, 0x02, 0xc0, 0x02, 0x6b, 0xa0}}},
    {L"image.webp", L"Convert to WebP", MediaKind::Image, L".webp",
     {0x38818621, 0x18cd, 0x4f59, {0xbd, 0xad, 0xfc, 0xc7, 0xef, 0xa8, 0x84, 0xdc}}},
}};

HRESULT CopyComString(const wchar_t* source, LPWSTR* destination) noexcept
{
    if (destination == nullptr)
    {
        return E_POINTER;
    }
    *destination = nullptr;
    if (source == nullptr)
    {
        return E_INVALIDARG;
    }

    const std::size_t length = std::wcslen(source);
    const std::size_t bytes = (length + 1) * sizeof(wchar_t);
    auto* buffer = static_cast<wchar_t*>(CoTaskMemAlloc(bytes));
    if (buffer == nullptr)
    {
        return E_OUTOFMEMORY;
    }

    std::memcpy(buffer, source, bytes);
    *destination = buffer;
    return S_OK;
}

const wchar_t* FindExtension(const wchar_t* path) noexcept
{
    if (path == nullptr)
    {
        return nullptr;
    }

    const wchar_t* lastSlash = std::wcsrchr(path, L'\\');
    const wchar_t* lastForwardSlash = std::wcsrchr(path, L'/');
    if (lastForwardSlash != nullptr && (lastSlash == nullptr || lastForwardSlash > lastSlash))
    {
        lastSlash = lastForwardSlash;
    }

    const wchar_t* lastDot = std::wcsrchr(path, L'.');
    if (lastDot == nullptr || (lastSlash != nullptr && lastDot < lastSlash) || lastDot[1] == L'\0')
    {
        return nullptr;
    }
    return lastDot;
}

template <std::size_t Size>
bool ContainsExtension(const std::array<const wchar_t*, Size>& extensions, const wchar_t* extension) noexcept
{
    if (extension == nullptr)
    {
        return false;
    }

    for (const wchar_t* allowed : extensions)
    {
        if (_wcsicmp(allowed, extension) == 0)
        {
            return true;
        }
    }
    return false;
}

bool TryClassifyExtension(const wchar_t* extension, MediaKind* kind) noexcept
{
    if (kind == nullptr)
    {
        return false;
    }
    if (ContainsExtension(kVideoExtensions, extension))
    {
        *kind = MediaKind::Video;
        return true;
    }
    if (ContainsExtension(kAudioExtensions, extension))
    {
        *kind = MediaKind::Audio;
        return true;
    }
    if (ContainsExtension(kImageExtensions, extension))
    {
        *kind = MediaKind::Image;
        return true;
    }
    return false;
}

HRESULT GetFilesystemPath(IShellItemArray* selection, DWORD index, PWSTR* path) noexcept
{
    if (selection == nullptr || path == nullptr)
    {
        return E_POINTER;
    }
    *path = nullptr;

    IShellItem* item = nullptr;
    HRESULT result = selection->GetItemAt(index, &item);
    if (FAILED(result))
    {
        return result;
    }

    SFGAOF attributes = 0;
    result = item->GetAttributes(SFGAO_FILESYSTEM | SFGAO_FOLDER, &attributes);
    if (SUCCEEDED(result))
    {
        if ((attributes & SFGAO_FILESYSTEM) == 0 || (attributes & SFGAO_FOLDER) != 0)
        {
            result = HRESULT_FROM_WIN32(ERROR_NOT_SUPPORTED);
        }
        else
        {
            result = item->GetDisplayName(SIGDN_FILESYSPATH, path);
        }
    }

    item->Release();
    return result;
}

HRESULT SelectionHasSupportedFamily(IShellItemArray* selection, bool* supported) noexcept
{
    if (supported == nullptr)
    {
        return E_POINTER;
    }
    *supported = false;
    if (selection == nullptr)
    {
        return S_OK;
    }

    DWORD count = 0;
    HRESULT result = selection->GetCount(&count);
    if (FAILED(result))
    {
        return result;
    }
    if (count == 0 || count > kMaximumSelectedFiles)
    {
        return S_OK;
    }

    bool haveKind = false;
    MediaKind selectionKind = MediaKind::Video;
    for (DWORD index = 0; index < count; ++index)
    {
        PWSTR path = nullptr;
        result = GetFilesystemPath(selection, index, &path);
        if (FAILED(result))
        {
            return S_OK;
        }

        MediaKind itemKind = MediaKind::Video;
        const bool classified = TryClassifyExtension(FindExtension(path), &itemKind);
        CoTaskMemFree(path);
        if (!classified)
        {
            return S_OK;
        }

        if (!haveKind)
        {
            selectionKind = itemKind;
            haveKind = true;
        }
        else if (itemKind != selectionKind)
        {
            return S_OK;
        }
    }

    *supported = haveKind;
    return S_OK;
}

HRESULT IsPresetApplicable(
    IShellItemArray* selection,
    const PresetDefinition& preset,
    bool* applicable) noexcept
{
    if (applicable == nullptr)
    {
        return E_POINTER;
    }
    *applicable = false;
    if (selection == nullptr)
    {
        return S_OK;
    }

    DWORD count = 0;
    HRESULT result = selection->GetCount(&count);
    if (FAILED(result))
    {
        return result;
    }
    if (count == 0 || count > kMaximumSelectedFiles)
    {
        return S_OK;
    }

    bool allAlreadyTargetExtension = true;
    for (DWORD index = 0; index < count; ++index)
    {
        PWSTR path = nullptr;
        result = GetFilesystemPath(selection, index, &path);
        if (FAILED(result))
        {
            return S_OK;
        }

        const wchar_t* extension = FindExtension(path);
        MediaKind kind = MediaKind::Video;
        const bool classified = TryClassifyExtension(extension, &kind);
        if (!classified || kind != preset.inputKind)
        {
            CoTaskMemFree(path);
            return S_OK;
        }
        if (_wcsicmp(extension, preset.outputExtension) != 0)
        {
            allAlreadyTargetExtension = false;
        }
        CoTaskMemFree(path);
    }

    *applicable = !allAlreadyTargetExtension;
    return S_OK;
}

std::wstring QuoteWindowsArgument(std::wstring_view argument)
{
    std::wstring quoted;
    quoted.reserve(argument.size() + 2);
    quoted.push_back(L'"');

    std::size_t backslashes = 0;
    for (const wchar_t character : argument)
    {
        if (character == L'\\')
        {
            ++backslashes;
            continue;
        }

        if (character == L'"')
        {
            quoted.append((backslashes * 2) + 1, L'\\');
            quoted.push_back(L'"');
        }
        else
        {
            quoted.append(backslashes, L'\\');
            quoted.push_back(character);
        }
        backslashes = 0;
    }

    quoted.append(backslashes * 2, L'\\');
    quoted.push_back(L'"');
    return quoted;
}

bool AppendArgument(std::wstring* commandLine, std::wstring_view argument)
{
    if (commandLine == nullptr)
    {
        return false;
    }

    std::wstring quoted = QuoteWindowsArgument(argument);
    const std::size_t separatorCharacters = commandLine->empty() ? 0 : 1;
    if (commandLine->size() + separatorCharacters + quoted.size() > kMaximumCommandLineCharacters)
    {
        return false;
    }

    if (!commandLine->empty())
    {
        commandLine->push_back(L' ');
    }
    commandLine->append(quoted);
    return true;
}

HRESULT GetModuleDirectory(std::wstring* directory)
{
    if (directory == nullptr)
    {
        return E_POINTER;
    }
    directory->clear();

    DWORD capacity = 512;
    for (;;)
    {
        std::wstring modulePath(capacity, L'\0');
        SetLastError(ERROR_SUCCESS);
        const DWORD copied = GetModuleFileNameW(g_module, modulePath.data(), capacity);
        if (copied == 0)
        {
            return HRESULT_FROM_WIN32(GetLastError());
        }
        if (copied < capacity - 1)
        {
            modulePath.resize(copied);
            const std::size_t separator = modulePath.find_last_of(L"\\/");
            if (separator == std::wstring::npos)
            {
                return HRESULT_FROM_WIN32(ERROR_BAD_PATHNAME);
            }
            *directory = modulePath.substr(0, separator);
            return S_OK;
        }
        if (capacity >= 32768)
        {
            return HRESULT_FROM_WIN32(ERROR_BUFFER_OVERFLOW);
        }
        capacity *= 2;
    }
}

HRESULT LaunchBridge(IShellItemArray* selection, const PresetDefinition& preset)
{
    bool applicable = false;
    HRESULT result = IsPresetApplicable(selection, preset, &applicable);
    if (FAILED(result) || !applicable)
    {
        return FAILED(result) ? result : HRESULT_FROM_WIN32(ERROR_NOT_SUPPORTED);
    }

    std::wstring moduleDirectory;
    result = GetModuleDirectory(&moduleDirectory);
    if (FAILED(result))
    {
        return result;
    }

    const std::wstring bridgePath = moduleDirectory + L"\\" + L"Converty.Bridge.exe";
    const DWORD bridgeAttributes = GetFileAttributesW(bridgePath.c_str());
    if (bridgeAttributes == INVALID_FILE_ATTRIBUTES)
    {
        return HRESULT_FROM_WIN32(GetLastError());
    }
    if ((bridgeAttributes & (FILE_ATTRIBUTE_DIRECTORY | FILE_ATTRIBUTE_REPARSE_POINT)) != 0)
    {
        return HRESULT_FROM_WIN32(ERROR_ACCESS_DENIED);
    }

    std::wstring commandLine;
    if (!AppendArgument(&commandLine, bridgePath)
        || !AppendArgument(&commandLine, L"--preset")
        || !AppendArgument(&commandLine, preset.id)
        || !AppendArgument(&commandLine, L"--"))
    {
        return HRESULT_FROM_WIN32(ERROR_BUFFER_OVERFLOW);
    }

    DWORD count = 0;
    result = selection->GetCount(&count);
    if (FAILED(result))
    {
        return result;
    }
    for (DWORD index = 0; index < count; ++index)
    {
        PWSTR path = nullptr;
        result = GetFilesystemPath(selection, index, &path);
        if (FAILED(result))
        {
            return result;
        }

        const bool appended = AppendArgument(&commandLine, path);
        CoTaskMemFree(path);
        if (!appended)
        {
            return HRESULT_FROM_WIN32(ERROR_BUFFER_OVERFLOW);
        }
    }

    STARTUPINFOW startupInfo{};
    startupInfo.cb = sizeof(startupInfo);
    PROCESS_INFORMATION processInfo{};
    const BOOL created = CreateProcessW(
        bridgePath.c_str(),
        commandLine.data(),
        nullptr,
        nullptr,
        FALSE,
        CREATE_NO_WINDOW,
        nullptr,
        moduleDirectory.c_str(),
        &startupInfo,
        &processInfo);
    if (!created)
    {
        const DWORD error = GetLastError();
        MessageBoxW(
            nullptr,
            L"Converty could not start the conversion process.",
            L"Converty",
            MB_OK | MB_ICONERROR);
        return HRESULT_FROM_WIN32(error);
    }

    CloseHandle(processInfo.hThread);
    CloseHandle(processInfo.hProcess);
    return S_OK;
}

class ExplorerChildCommand final : public IExplorerCommand
{
public:
    explicit ExplorerChildCommand(const PresetDefinition* preset) noexcept : preset_(preset)
    {
        ++g_objectCount;
    }

    HRESULT STDMETHODCALLTYPE QueryInterface(REFIID interfaceId, void** object) override
    {
        if (object == nullptr)
        {
            return E_POINTER;
        }
        *object = nullptr;
        if (IsEqualIID(interfaceId, IID_IUnknown) || IsEqualIID(interfaceId, IID_IExplorerCommand))
        {
            *object = static_cast<IExplorerCommand*>(this);
            AddRef();
            return S_OK;
        }
        return E_NOINTERFACE;
    }

    ULONG STDMETHODCALLTYPE AddRef() override
    {
        return ++referenceCount_;
    }

    ULONG STDMETHODCALLTYPE Release() override
    {
        const ULONG remaining = --referenceCount_;
        if (remaining == 0)
        {
            delete this;
        }
        return remaining;
    }

    HRESULT STDMETHODCALLTYPE GetTitle(IShellItemArray*, LPWSTR* title) override
    {
        return CopyComString(preset_->title, title);
    }

    HRESULT STDMETHODCALLTYPE GetIcon(IShellItemArray*, LPWSTR* icon) override
    {
        if (icon == nullptr)
        {
            return E_POINTER;
        }
        *icon = nullptr;
        return E_NOTIMPL;
    }

    HRESULT STDMETHODCALLTYPE GetToolTip(IShellItemArray*, LPWSTR* toolTip) override
    {
        if (toolTip == nullptr)
        {
            return E_POINTER;
        }
        *toolTip = nullptr;
        return E_NOTIMPL;
    }

    HRESULT STDMETHODCALLTYPE GetCanonicalName(GUID* canonicalName) override
    {
        if (canonicalName == nullptr)
        {
            return E_POINTER;
        }
        *canonicalName = preset_->canonicalName;
        return S_OK;
    }

    HRESULT STDMETHODCALLTYPE GetState(IShellItemArray* selection, BOOL, EXPCMDSTATE* state) override
    {
        if (state == nullptr)
        {
            return E_POINTER;
        }

        bool applicable = false;
        const HRESULT result = IsPresetApplicable(selection, *preset_, &applicable);
        if (FAILED(result))
        {
            return result;
        }
        *state = applicable ? ECS_ENABLED : ECS_HIDDEN;
        return S_OK;
    }

    HRESULT STDMETHODCALLTYPE Invoke(IShellItemArray* selection, IBindCtx*) override
    {
        try
        {
            return LaunchBridge(selection, *preset_);
        }
        catch (const std::bad_alloc&)
        {
            return E_OUTOFMEMORY;
        }
    }

    HRESULT STDMETHODCALLTYPE GetFlags(EXPCMDFLAGS* flags) override
    {
        if (flags == nullptr)
        {
            return E_POINTER;
        }
        *flags = ECF_DEFAULT;
        return S_OK;
    }

    HRESULT STDMETHODCALLTYPE EnumSubCommands(IEnumExplorerCommand** commands) override
    {
        if (commands == nullptr)
        {
            return E_POINTER;
        }
        *commands = nullptr;
        return E_NOTIMPL;
    }

private:
    ~ExplorerChildCommand()
    {
        --g_objectCount;
    }

    std::atomic<ULONG> referenceCount_{1};
    const PresetDefinition* preset_;
};

class ExplorerCommandEnumerator final : public IEnumExplorerCommand
{
public:
    explicit ExplorerCommandEnumerator(std::size_t index = 0) noexcept : index_(index)
    {
        ++g_objectCount;
    }

    HRESULT STDMETHODCALLTYPE QueryInterface(REFIID interfaceId, void** object) override
    {
        if (object == nullptr)
        {
            return E_POINTER;
        }
        *object = nullptr;
        if (IsEqualIID(interfaceId, IID_IUnknown) || IsEqualIID(interfaceId, IID_IEnumExplorerCommand))
        {
            *object = static_cast<IEnumExplorerCommand*>(this);
            AddRef();
            return S_OK;
        }
        return E_NOINTERFACE;
    }

    ULONG STDMETHODCALLTYPE AddRef() override
    {
        return ++referenceCount_;
    }

    ULONG STDMETHODCALLTYPE Release() override
    {
        const ULONG remaining = --referenceCount_;
        if (remaining == 0)
        {
            delete this;
        }
        return remaining;
    }

    HRESULT STDMETHODCALLTYPE Next(ULONG count, IExplorerCommand** commands, ULONG* fetched) override
    {
        if (commands == nullptr || (count != 1 && fetched == nullptr))
        {
            return E_POINTER;
        }
        if (fetched != nullptr)
        {
            *fetched = 0;
        }

        ULONG produced = 0;
        while (produced < count && index_ < kPresets.size())
        {
            auto* command = new (std::nothrow) ExplorerChildCommand(&kPresets[index_]);
            if (command == nullptr)
            {
                return E_OUTOFMEMORY;
            }
            commands[produced] = command;
            ++produced;
            ++index_;
        }

        if (fetched != nullptr)
        {
            *fetched = produced;
        }
        return produced == count ? S_OK : S_FALSE;
    }

    HRESULT STDMETHODCALLTYPE Skip(ULONG count) override
    {
        const std::size_t remaining = kPresets.size() - index_;
        if (count > remaining)
        {
            index_ = kPresets.size();
            return S_FALSE;
        }
        index_ += count;
        return S_OK;
    }

    HRESULT STDMETHODCALLTYPE Reset() override
    {
        index_ = 0;
        return S_OK;
    }

    HRESULT STDMETHODCALLTYPE Clone(IEnumExplorerCommand** clone) override
    {
        if (clone == nullptr)
        {
            return E_POINTER;
        }
        *clone = new (std::nothrow) ExplorerCommandEnumerator(index_);
        return *clone == nullptr ? E_OUTOFMEMORY : S_OK;
    }

private:
    ~ExplorerCommandEnumerator()
    {
        --g_objectCount;
    }

    std::atomic<ULONG> referenceCount_{1};
    std::size_t index_;
};

class ExplorerRootCommand final : public IExplorerCommand
{
public:
    ExplorerRootCommand() noexcept
    {
        ++g_objectCount;
    }

    HRESULT STDMETHODCALLTYPE QueryInterface(REFIID interfaceId, void** object) override
    {
        if (object == nullptr)
        {
            return E_POINTER;
        }
        *object = nullptr;
        if (IsEqualIID(interfaceId, IID_IUnknown) || IsEqualIID(interfaceId, IID_IExplorerCommand))
        {
            *object = static_cast<IExplorerCommand*>(this);
            AddRef();
            return S_OK;
        }
        return E_NOINTERFACE;
    }

    ULONG STDMETHODCALLTYPE AddRef() override
    {
        return ++referenceCount_;
    }

    ULONG STDMETHODCALLTYPE Release() override
    {
        const ULONG remaining = --referenceCount_;
        if (remaining == 0)
        {
            delete this;
        }
        return remaining;
    }

    HRESULT STDMETHODCALLTYPE GetTitle(IShellItemArray*, LPWSTR* title) override
    {
        return CopyComString(L"Converty", title);
    }

    HRESULT STDMETHODCALLTYPE GetIcon(IShellItemArray*, LPWSTR* icon) override
    {
        if (icon == nullptr)
        {
            return E_POINTER;
        }
        *icon = nullptr;
        return E_NOTIMPL;
    }

    HRESULT STDMETHODCALLTYPE GetToolTip(IShellItemArray*, LPWSTR* toolTip) override
    {
        if (toolTip == nullptr)
        {
            return E_POINTER;
        }
        *toolTip = nullptr;
        return E_NOTIMPL;
    }

    HRESULT STDMETHODCALLTYPE GetCanonicalName(GUID* canonicalName) override
    {
        if (canonicalName == nullptr)
        {
            return E_POINTER;
        }
        *canonicalName = kRootCanonicalName;
        return S_OK;
    }

    HRESULT STDMETHODCALLTYPE GetState(IShellItemArray* selection, BOOL, EXPCMDSTATE* state) override
    {
        if (state == nullptr)
        {
            return E_POINTER;
        }

        bool supported = false;
        const HRESULT result = SelectionHasSupportedFamily(selection, &supported);
        if (FAILED(result))
        {
            return result;
        }
        *state = supported ? ECS_ENABLED : ECS_HIDDEN;
        return S_OK;
    }

    HRESULT STDMETHODCALLTYPE Invoke(IShellItemArray*, IBindCtx*) override
    {
        return E_NOTIMPL;
    }

    HRESULT STDMETHODCALLTYPE GetFlags(EXPCMDFLAGS* flags) override
    {
        if (flags == nullptr)
        {
            return E_POINTER;
        }
        *flags = ECF_HASSUBCOMMANDS;
        return S_OK;
    }

    HRESULT STDMETHODCALLTYPE EnumSubCommands(IEnumExplorerCommand** commands) override
    {
        if (commands == nullptr)
        {
            return E_POINTER;
        }
        *commands = new (std::nothrow) ExplorerCommandEnumerator();
        return *commands == nullptr ? E_OUTOFMEMORY : S_OK;
    }

private:
    ~ExplorerRootCommand()
    {
        --g_objectCount;
    }

    std::atomic<ULONG> referenceCount_{1};
};

class ExplorerCommandClassFactory final : public IClassFactory
{
public:
    ExplorerCommandClassFactory() noexcept
    {
        ++g_objectCount;
    }

    HRESULT STDMETHODCALLTYPE QueryInterface(REFIID interfaceId, void** object) override
    {
        if (object == nullptr)
        {
            return E_POINTER;
        }
        *object = nullptr;
        if (IsEqualIID(interfaceId, IID_IUnknown) || IsEqualIID(interfaceId, IID_IClassFactory))
        {
            *object = static_cast<IClassFactory*>(this);
            AddRef();
            return S_OK;
        }
        return E_NOINTERFACE;
    }

    ULONG STDMETHODCALLTYPE AddRef() override
    {
        return ++referenceCount_;
    }

    ULONG STDMETHODCALLTYPE Release() override
    {
        const ULONG remaining = --referenceCount_;
        if (remaining == 0)
        {
            delete this;
        }
        return remaining;
    }

    HRESULT STDMETHODCALLTYPE CreateInstance(IUnknown* outer, REFIID interfaceId, void** object) override
    {
        if (object == nullptr)
        {
            return E_POINTER;
        }
        *object = nullptr;
        if (outer != nullptr)
        {
            return CLASS_E_NOAGGREGATION;
        }

        auto* command = new (std::nothrow) ExplorerRootCommand();
        if (command == nullptr)
        {
            return E_OUTOFMEMORY;
        }
        const HRESULT result = command->QueryInterface(interfaceId, object);
        command->Release();
        return result;
    }

    HRESULT STDMETHODCALLTYPE LockServer(BOOL lock) override
    {
        if (lock)
        {
            ++g_serverLockCount;
        }
        else
        {
            --g_serverLockCount;
        }
        return S_OK;
    }

private:
    ~ExplorerCommandClassFactory()
    {
        --g_objectCount;
    }

    std::atomic<ULONG> referenceCount_{1};
};
} // namespace

STDAPI DllGetClassObject(
    REFCLSID classId,
    REFIID interfaceId,
    void** object)
{
    if (object == nullptr)
    {
        return E_POINTER;
    }
    *object = nullptr;
    if (!IsEqualCLSID(classId, kConvertyCommandClsid))
    {
        return CLASS_E_CLASSNOTAVAILABLE;
    }

    auto* factory = new (std::nothrow) ExplorerCommandClassFactory();
    if (factory == nullptr)
    {
        return E_OUTOFMEMORY;
    }
    const HRESULT result = factory->QueryInterface(interfaceId, object);
    factory->Release();
    return result;
}

STDAPI DllCanUnloadNow(void)
{
    return g_objectCount.load() == 0 && g_serverLockCount.load() == 0 ? S_OK : S_FALSE;
}

BOOL APIENTRY DllMain(HMODULE module, DWORD reason, LPVOID)
{
    if (reason == DLL_PROCESS_ATTACH)
    {
        g_module = module;
        DisableThreadLibraryCalls(module);
    }
    return TRUE;
}
