from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
PROVIDER = ROOT / "providers" / "Converty.Provider.FFmpeg" / "FfmpegPresetCompiler.cs"
LAUNCHER = ROOT / "providers" / "Converty.Provider.FFmpeg" / "FfmpegProcessLauncher.cs"
TRUSTED_FFMPEG = ROOT / "providers" / "Converty.Provider.FFmpeg" / "TrustedFfmpegPath.cs"
TRUSTED_FFPROBE = ROOT / "providers" / "Converty.Provider.FFmpeg" / "TrustedFfprobePath.cs"
CANARY = ROOT / "tests" / "Converty.WorkerCanary" / "Program.cs"
ACTUAL_ENGINE_TEST = (
    ROOT
    / "tests"
    / "Converty.Security.Tests"
    / "Workers"
    / "ActualEngineDescendantIsolationTests.cs"
)


def test_video_engine_uses_qualified_file_only_format_surface():
    provider = PROVIDER.read_text(encoding="utf-8")
    assert '"-protocol_whitelist", "file"' in provider
    assert '"-format_whitelist"' in provider
    assert '"mov,matroska,avi,mpeg,asf"' in provider


def test_probe_engine_uses_qualified_file_only_format_surface():
    launcher = LAUNCHER.read_text(encoding="utf-8")
    assert '"-protocol_whitelist", "file"' in launcher
    assert '"-format_whitelist"' in launcher
    assert '"mov,matroska,avi,mpeg,asf,mp3"' in launcher


def test_actual_engine_descendant_canary_modes_are_present():
    canary = CANARY.read_text(encoding="utf-8")
    for mode in (
        "--spawn-ffprobe-read",
        "--spawn-ffmpeg-write-wave",
        "--spawn-ffmpeg-hold",
        "--spawn-ffmpeg-connect-loopback",
        "--spawn-ffprobe-connect-loopback",
    ):
        assert mode in canary


def test_actual_engine_descendant_gate_proves_appcontainer_network_filesystem_and_job_cleanup():
    test_source = ACTUAL_ENGINE_TEST.read_text(encoding="utf-8")
    for marker in (
        "child_appcontainer=1",
        "ActualFfprobeDescendantCanReadOnlyGrantedInput",
        "ActualFfmpegDescendantCanWriteOnlyInsideGrantedStaging",
        "ActualFfmpegDescendantCannotConnectToLoopback",
        "ActualFfprobeDescendantCannotConnectToLoopback",
        "ActualFfmpegDescendantDiesWithWorkerJobOnCancellation",
        "AssertNoLoopbackConnectionAsync",
        "IsAppContainerProcess",
        "AssertProcessExitedAsync",
    ):
        assert marker in test_source


def test_trusted_engine_paths_reject_reparse_points_at_fixed_bundle_boundaries():
    for path in (TRUSTED_FFMPEG, TRUSTED_FFPROBE):
        source = path.read_text(encoding="utf-8")
        assert "FileAttributes.ReparsePoint" in source
        assert "RejectReparsePoint(root" in source
        assert "RejectReparsePoint(toolsDirectory" in source
        assert "RejectReparsePoint(engineDirectory" in source
        assert "RejectReparsePoint(executablePath" in source


def test_probe_launcher_rejects_reparse_executable_and_input():
    launcher = LAUNCHER.read_text(encoding="utf-8")
    assert "Trusted ffprobe.exe must not be a reparse point." in launcher
    assert "Probe input must not be a reparse point." in launcher
    assert "Path.IsPathFullyQualified(ffprobePath)" in launcher
    assert "Path.IsPathFullyQualified(inputPath)" in launcher
