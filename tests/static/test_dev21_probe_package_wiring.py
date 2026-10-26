from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
STAGE = ROOT / "build" / "stage-dev-package.ps1"
CI = ROOT / ".github" / "workflows" / "ci.yml"
SMOKE = ROOT / "build" / "probe-worker-smoke.ps1"


def test_dev21_package_stages_fixed_probe_worker_and_ffprobe():
    stage = STAGE.read_text(encoding="utf-8").lower()
    for token in (
        "[string]$ffprobepath",
        "converty.probeworker.exe",
        "src/converty.probeworker/bin",
        "ffprobe.exe",
        "tools/ffmpeg",
    ):
        assert token in stage


def test_dev21_ci_supplies_pinned_ffprobe_and_runs_real_probe_smoke():
    ci = CI.read_text(encoding="utf-8").lower()
    assert "-ffprobepath ./artifacts/dev-ffmpeg/ffprobe.exe" in ci
    assert "probe worker packaged ffprobe acceptance" in ci
    assert "./build/probe-worker-smoke.ps1" in ci


def test_dev21_probe_smoke_locks_packaged_and_fail_closed_invariants():
    assert SMOKE.is_file(), "dev.21 packaged ProbeWorker/ffprobe smoke is missing"
    smoke = SMOKE.read_text(encoding="utf-8").lower()
    for token in (
        "converty.probeworker.exe",
        "tools/ffmpeg/ffprobe.exe",
        "media.probe.result.v1",
        "unsupportedinput",
        "unicode",
        "metachar",
        "get-ciminstance win32_process",
        "ffprobe",
    ):
        assert token in smoke
