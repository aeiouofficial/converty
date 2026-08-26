import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]


def text(path: str) -> str:
    return (ROOT / path).read_text(encoding="utf-8")


def test_native_shell_extension_is_a_real_iexplorercommand_dll() -> None:
    cmake = text("native/CMakeLists.txt")
    source = text("native/Converty.ShellExtension/ConvertyShellExtension.cpp")
    exports = text("native/Converty.ShellExtension/Converty.ShellExtension.def")

    assert "add_library(Converty.ShellExtension SHARED" in cmake
    assert "Converty.ShellExtension.def" in cmake
    assert "converty_apply_msvc_hardening(Converty.ShellExtension)" in cmake
    assert "IExplorerCommand" in source
    assert "IEnumExplorerCommand" in source
    assert "EnumSubCommands" in source
    assert "ECF_HASSUBCOMMANDS" in source
    assert "SIGDN_FILESYSPATH" in source
    assert "ECS_HIDDEN" in source
    assert "STDAPI DllGetClassObject" in source
    assert "STDAPI DllCanUnloadNow" in source
    assert "DllGetClassObject PRIVATE" in exports
    assert "DllCanUnloadNow PRIVATE" in exports


def test_shell_handoff_launches_only_fixed_app_local_bridge_without_a_shell() -> None:
    source = text("native/Converty.ShellExtension/ConvertyShellExtension.cpp")
    lowered = source.lower()

    assert 'L"Converty.Bridge.exe"' in source
    assert "GetModuleFileNameW" in source
    assert "CreateProcessW" in source
    assert "CREATE_NO_WINDOW" in source
    assert 'L"--preset"' in source
    assert 'L"--"' in source
    assert "QuoteWindowsArgument" in source
    for forbidden in ("shellexecut", "cmd.exe", "powershell", "system(", "_wsystem"):
        assert forbidden not in lowered


def test_shell_menu_contains_only_stable_known_product_preset_ids() -> None:
    source = text("native/Converty.ShellExtension/ConvertyShellExtension.cpp")
    core = text("src/Converty.Core/Presets/ProductPresetRegistry.cs")
    preset_ids = (
        "video.mp4.h264",
        "video.webm.vp9",
        "extract.audio.mp3",
        "audio.mp3",
        "audio.flac",
        "audio.wav",
        "image.png",
        "image.jpeg",
        "image.webp",
    )

    for preset_id in preset_ids:
        assert preset_id in source
        assert preset_id in core
    assert "-c:v" not in source
    assert "-c:a" not in source
    assert "libx264" not in source
    assert "libmp3lame" not in source


def test_native_product_smokes_build_release_and_exercise_the_shell_dll() -> None:
    cmake = text("native/CMakeLists.txt")
    presets = json.loads(text("CMakePresets.json"))
    smoke = text("native/Converty.ShellExtension/ExplorerRegistrationSmoke.cpp")

    configure = next(item for item in presets["configurePresets"] if item["name"] == "native-smoke")
    assert configure["cacheVariables"]["CMAKE_BUILD_TYPE"] == "Release"
    assert "add_executable(Converty.ExplorerRegistrationSmoke" in cmake
    assert "onecore" in cmake.lower()
    assert "converty_apply_msvc_hardening(Converty.ExplorerRegistrationSmoke)" in cmake
    assert "LoadLibraryExW" in smoke
    assert 'GetProcAddress(module, "DllGetClassObject")' in smoke
    assert "IClassFactory" in smoke
    assert "SHCreateItemFromParsingName" in smoke
    assert "SHCreateShellItemArrayFromShellItem" in smoke
    assert 'std::wcscmp(title, L"Convert to MP3")' in smoke
    assert "command->Invoke(selection, nullptr)" in smoke


def test_development_package_registers_and_invokes_the_same_shell_command() -> None:
    manifest = text("packaging/Converty.Package/AppxManifest.xml")
    registration_smoke = text("build/explorer-registration-smoke.ps1")
    workflow = text(".github/workflows/ci.yml")
    clsid = "20E7C5C1-3E5F-4D0F-9C56-2E9F2A978A10"

    assert 'Category="windows.comServer"' in manifest
    assert '<com:SurrogateServer DisplayName="Converty Explorer Command">' in manifest
    assert 'Path="Converty.ShellExtension.dll"' in manifest
    assert 'ThreadingModel="STA"' in manifest
    assert 'Category="windows.fileExplorerContextMenus"' in manifest
    assert '<desktop5:ItemType Type="*">' in manifest
    assert manifest.count(clsid) == 2

    assert "Add-AppxPackage -Register $manifest" in registration_smoke
    assert "Remove-AppxPackage" in registration_smoke
    assert "'--module' $shellDll $input" in registration_smoke
    assert "Packaged Explorer COM activation/invoke smoke" in registration_smoke
    assert "./build/explorer-registration-smoke.ps1" in workflow
    assert "./build/product-conversion-smoke.ps1" in workflow
