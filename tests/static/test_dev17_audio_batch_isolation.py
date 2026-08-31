from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
SMOKE = ROOT / "build" / "audio-batch-isolation-smoke.ps1"
CI = ROOT / ".github" / "workflows" / "ci.yml"
NATIVE = ROOT / "native" / "Converty.ShellExtension" / "ConvertyShellExtension.cpp"


def test_dev17_audio_batch_isolation_smoke_is_wired_into_ci():
    """The production change is a dedicated Windows mixed-batch acceptance gate in ordinary CI."""
    assert SMOKE.is_file(), "dev.17 Audio batch isolation smoke is missing"
    ci = CI.read_text(encoding="utf-8")
    assert "Audio mixed-batch failure isolation" in ci
    assert "./build/audio-batch-isolation-smoke.ps1" in ci


def test_dev17_exercises_one_real_bridge_batch_with_valid_failure_valid_ordering():
    smoke = SMOKE.read_text(encoding="utf-8")

    for token in (
        "--preset",
        "audio.mp3",
        "malformed",
        "truncated",
        "CONVERTY_BRIDGE_NONINTERACTIVE",
        "ArgumentList.Add($source.Path)",
        "WaitForExit(30000)",
    ):
        assert token in smoke

    assert "valid-before" in smoke.lower()
    assert "valid-after" in smoke.lower()
    assert "mixed batch" in smoke.lower()


def test_dev17_locks_per_file_transactional_and_cleanup_invariants():
    smoke = SMOKE.read_text(encoding="utf-8")

    for token in (
        "Get-FileHash",
        "pre-existing destination",
        "numbered",
        ".converty-*.partial.*",
        "source preserved",
        "exit code 4",
    ):
        assert token.lower() in smoke.lower()


def test_dev17_powershell_log_interpolation_is_parser_safe():
    smoke = SMOKE.read_text(encoding="utf-8")
    assert '"Mixed batch attempt ${attempt}:' in smoke
    assert '"Mixed batch attempt $attempt:' not in smoke


def test_native_explorer_keeps_single_bridge_process_for_same_family_multiselection():
    native = NATIVE.read_text(encoding="utf-8")

    assert "selection->GetCount(&count)" in native
    assert "for (DWORD index = 0; index < count; ++index)" in native
    assert "AppendArgument(&commandLine, path)" in native
    assert "CreateProcessW(" in native
    assert native.count("CreateProcessW(") == 1
