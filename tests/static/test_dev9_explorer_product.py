from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]


def text(path: str) -> str:
    return (ROOT / path).read_text(encoding="utf-8")


def test_native_shell_extension_is_a_real_iexplorercommand_dll() -> None:
    cmake = text("native/CMakeLists.txt")
    source = text("native/Converty.ShellExtension/ConvertyShellExtension.cpp")

    assert "add_library(Converty.ShellExtension SHARED" in cmake
    assert "converty_apply_msvc_hardening(Converty.ShellExtension)" in cmake
    assert "IExplorerCommand" in source
    assert "IEnumExplorerCommand" in source
    assert "EnumSubCommands" in source
    assert "ECF_HASSUBCOMMANDS" in source
    assert "SIGDN_FILESYSPATH" in source
    assert "ECS_HIDDEN" in source
    assert "DllGetClassObject" in source
    assert "DllCanUnloadNow" in source


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
