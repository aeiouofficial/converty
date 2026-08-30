from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
SMOKE = ROOT / "build" / "audio-input-acceptance-smoke.ps1"
CI = ROOT / ".github" / "workflows" / "ci.yml"
BRIDGE_ERROR_DIALOG = ROOT / "src" / "Converty.Bridge" / "Shell" / "BridgeErrorDialog.cs"
NONINTERACTIVE_ENV = "CONVERTY_BRIDGE_NONINTERACTIVE"


def test_dev16_audio_input_acceptance_smoke_is_wired_into_ci():
    """The production change is the dedicated dev.16 Windows acceptance smoke + CI step."""
    assert SMOKE.is_file(), "dev.16 Audio input acceptance smoke is missing"
    ci = CI.read_text(encoding="utf-8")
    assert "Audio source and malformed-input acceptance" in ci
    assert "./build/audio-input-acceptance-smoke.ps1" in ci


def test_dev16_covers_representative_supported_audio_sources_and_all_fixed_actions():
    smoke = SMOKE.read_text(encoding="utf-8")

    for source_extension in (".wav", ".flac", ".mp3", ".m4a", ".ogg", ".opus"):
        assert source_extension in smoke

    for preset_id in (
        "audio.mp3",
        "audio.flac",
        "audio.m4a.aac",
        "audio.opus",
        "audio.ogg.vorbis",
        "audio.wav",
    ):
        assert preset_id in smoke

    assert "artifacts/audio-input-acceptance-smoke" in smoke.replace("\\\\", "/")
    assert "ArgumentList.Add('--preset')" in smoke
    assert "ArgumentList.Add($presetId)" in smoke


def test_dev16_negative_cases_lock_transactional_failure_invariants():
    smoke = SMOKE.read_text(encoding="utf-8")

    for token in (
        "malformed",
        "truncated",
        "Get-FileHash",
        "pre-existing destination",
        ".converty-*.partial.*",
        "expected failure",
    ):
        assert token.lower() in smoke.lower()

    assert "WaitForExit(30000)" in smoke
    assert "$bridgeExitCode -eq 0" in smoke


def test_dev16_noninteractive_mode_preserves_failure_exit_without_modal_blocking():
    smoke = SMOKE.read_text(encoding="utf-8")
    dialog = BRIDGE_ERROR_DIALOG.read_text(encoding="utf-8")

    assert NONINTERACTIVE_ENV in smoke
    assert NONINTERACTIVE_ENV in dialog
    assert "Console.Error" in dialog
    assert "MessageBoxW" in dialog
