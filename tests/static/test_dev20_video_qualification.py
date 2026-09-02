from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
ACCEPTANCE = ROOT / "build" / "video-input-acceptance-smoke.ps1"
BATCH = ROOT / "build" / "video-batch-isolation-smoke.ps1"
CI = ROOT / ".github" / "workflows" / "ci.yml"

VIDEO_EXTENSIONS = (
    ".mp4",
    ".mov",
    ".mkv",
    ".avi",
    ".webm",
    ".m4v",
    ".mpeg",
    ".mpg",
    ".wmv",
)
VIDEO_PRESETS = (
    "video.mp4.h264",
    "video.webm.vp9",
    "extract.audio.mp3",
)


def test_dev20_video_acceptance_and_batch_gates_exist_and_are_wired():
    """Dev.20 requires real packaged Video acceptance and mixed-batch gates."""
    assert ACCEPTANCE.is_file(), "dev.20 Video source acceptance smoke is missing"
    assert BATCH.is_file(), "dev.20 Video mixed-batch smoke is missing"

    ci = CI.read_text(encoding="utf-8")
    assert "Video source and malformed-input acceptance" in ci
    assert "./build/video-input-acceptance-smoke.ps1" in ci
    assert "Video mixed-batch failure isolation" in ci
    assert "./build/video-batch-isolation-smoke.ps1" in ci

    image_batch = ci.index("Image mixed-batch failure isolation")
    video_acceptance = ci.index("Video source and malformed-input acceptance")
    video_batch = ci.index("Video mixed-batch failure isolation")
    managed_tests = ci.index("      - name: Test\n")
    assert image_batch < video_acceptance < video_batch < managed_tests


def test_dev20_acceptance_covers_all_sources_actions_and_probe_contracts():
    assert ACCEPTANCE.is_file(), "dev.20 Video source acceptance smoke is missing"
    smoke = ACCEPTANCE.read_text(encoding="utf-8").lower()

    for extension in VIDEO_EXTENSIONS:
        assert extension in smoke
    for preset in VIDEO_PRESETS:
        assert preset in smoke
    for token in (
        "ffprobe",
        "codec_name",
        "h264",
        "aac",
        "vp9",
        "opus",
        "mp3",
        "malformed",
        "truncated",
        "foreach ($attempt in 1..2)",
    ):
        assert token in smoke


def test_dev20_acceptance_locks_transactional_path_invariants():
    assert ACCEPTANCE.is_file(), "dev.20 Video source acceptance smoke is missing"
    smoke = ACCEPTANCE.read_text(encoding="utf-8").lower()

    for token in (
        "hör",
        "& semi;",
        "get-filehash",
        "pre-existing destination",
        "numbered",
        ".converty-*.partial.*",
        "argumentlist.add('--preset')",
        "converty_bridge_noninteractive",
    ):
        assert token in smoke


def test_dev20_batch_locks_failure_isolation_and_orphan_cleanup():
    assert BATCH.is_file(), "dev.20 Video mixed-batch smoke is missing"
    smoke = BATCH.read_text(encoding="utf-8").lower()

    for token in (
        "valid-mp4",
        "malformed-avi",
        "valid-mov",
        "truncated-mkv",
        "valid-webm",
        "foreach ($attempt in 1..2)",
        "exit code 4",
        ".converty-*.partial.*",
        "get-ciminstance win32_process",
        "converty.engineworker",
        "ffmpeg",
    ):
        assert token in smoke
