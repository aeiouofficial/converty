from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
PROVIDER = ROOT / "providers" / "Converty.Provider.FFmpeg" / "FfmpegPresetCompiler.cs"
LAUNCHER = ROOT / "providers" / "Converty.Provider.FFmpeg" / "FfmpegProcessLauncher.cs"
CANARY = ROOT / "tests" / "Converty.WorkerCanary" / "Program.cs"


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
    ):
        assert mode in canary
