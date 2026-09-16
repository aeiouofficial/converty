from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
PROVIDER = ROOT / "providers" / "Converty.Provider.FFmpeg" / "FfmpegPresetCompiler.cs"
LAUNCHER = ROOT / "providers" / "Converty.Provider.FFmpeg" / "FfmpegProcessLauncher.cs"
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


def test_actual_packaged_engine_descendant_gate_uses_product_workers():
    test_source = ACTUAL_ENGINE_TEST.read_text(encoding="utf-8")
    assert "Converty.ProbeWorker.exe" in test_source
    assert "Converty.EngineWorker.exe" in test_source
    assert "ffprobe.exe" in test_source
    assert "ffmpeg.exe" in test_source
    assert "TokenIsAppContainer" in test_source
    assert "AssertProcessExitedAsync" in test_source
