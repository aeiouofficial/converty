import re
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
SMOKE = ROOT / "build" / "image-batch-isolation-smoke.ps1"
CI = ROOT / ".github" / "workflows" / "ci.yml"


def test_dev19_image_batch_smoke_exists_and_is_wired_into_ci():
    """Dev.19 requires one real Windows product gate for mixed Image selections."""
    assert SMOKE.is_file(), "dev.19 Image mixed-batch acceptance smoke is missing"
    ci = CI.read_text(encoding="utf-8")
    assert "Image mixed-batch failure isolation" in ci
    assert "./build/image-batch-isolation-smoke.ps1" in ci


def test_dev19_smoke_uses_one_bridge_batch_and_continues_after_failures():
    assert SMOKE.is_file(), "dev.19 Image mixed-batch acceptance smoke is missing"
    smoke = SMOKE.read_text(encoding="utf-8").lower()
    for token in (
        "argumentlist.add('--preset')",
        "argumentlist.add('image.png')",
        "argumentlist.add($source.path)",
        "converty_bridge_noninteractive",
        "waitforexit(30000)",
        "malformed",
        "truncated",
        "foreach ($source in $sources)",
        "exit code 4",
    ):
        assert token in smoke


def test_dev19_smoke_locks_transactional_and_process_cleanup_invariants():
    assert SMOKE.is_file(), "dev.19 Image mixed-batch acceptance smoke is missing"
    smoke = SMOKE.read_text(encoding="utf-8").lower()
    for token in (
        "get-filehash",
        "pre-existing destination",
        "numbered",
        ".converty-*.partial.*",
        "source preserved",
        "orphan converter processes",
        "get-ciminstance win32_process",
    ):
        assert token in smoke


def test_dev19_smoke_never_seeds_a_preexisting_destination_over_a_source():
    """Same-extension Image conversion is valid, but collision setup must never mutate its source."""
    smoke = SMOKE.read_text(encoding="utf-8")
    guarded_seed = re.compile(
        r"if\s*\(\s*-not\s+\[string\]::Equals\("
        r"\$baseOutput,\s*\$source\.Path,\s*\[StringComparison\]::OrdinalIgnoreCase"
        r"\)\s*\)\s*\{[^}]*"
        r"\[System\.IO\.File\]::WriteAllBytes\(\$baseOutput,",
        re.IGNORECASE | re.DOTALL,
    )
    assert guarded_seed.search(smoke), (
        "dev.19 collision setup must skip pre-existing-destination seeding when the "
        "resolved target aliases the selected source path"
    )
