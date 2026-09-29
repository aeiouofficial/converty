from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
MODE_SMOKE = ROOT / "build" / "video-mode-qualification-smoke.ps1"
CI = ROOT / ".github" / "workflows" / "ci.yml"


def test_packaged_video_mode_qualification_smoke_exists():
    assert MODE_SMOKE.is_file(), "Task 10 requires a dedicated packaged execution-mode qualification smoke."


def test_mode_smoke_has_unambiguous_copy_remux_transcode_witnesses():
    source = MODE_SMOKE.read_text(encoding="utf-8")
    for marker in (
        "COPY_WITNESS",
        "REMUX_WITNESS",
        "TRANSCODE_WITNESS",
        "Get-FileHash",
        "streamhash",
        "video.mp4.h264",
        "h264",
        "aac",
        "mpeg2video",
        "mp3",
    ):
        assert marker in source


def test_ci_runs_packaged_video_mode_qualification_before_full_video_regressions():
    ci = CI.read_text(encoding="utf-8")
    marker = "./build/video-mode-qualification-smoke.ps1"
    full_matrix = "./build/video-input-acceptance-smoke.ps1"
    assert marker in ci
    assert full_matrix in ci
    assert ci.index(marker) < ci.index(full_matrix)
