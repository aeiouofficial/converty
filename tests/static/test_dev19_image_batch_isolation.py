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


def test_dev19_smoke_never_aliases_a_source_with_the_preexisting_target_path():
    """The harness must not overwrite a selected source while seeding collision targets."""
    smoke = SMOKE.read_text(encoding="utf-8")

    preset_match = re.search(r"ArgumentList\.Add\('image(?P<ext>\.[a-z0-9]+)'\)", smoke, re.IGNORECASE)
    assert preset_match, "dev.19 smoke must expose the fixed Image output extension"
    target_extension = preset_match.group("ext").lower()

    fixture_paths = {
        variable: filename
        for variable, filename in re.findall(
            r"\$(\w+)\s*=\s*Join-Path\s+\$caseRoot\s+'([^']+)'",
            smoke,
            re.IGNORECASE,
        )
    }
    selected_variables = re.findall(r"Path=\$(\w+);\s*Valid=\$(?:true|false)", smoke, re.IGNORECASE)
    assert selected_variables, "dev.19 smoke must define the mixed Image selection"

    for variable in selected_variables:
        assert variable in fixture_paths, f"missing fixture path for selected variable ${variable}"
        source_extension = Path(fixture_paths[variable]).suffix.lower()
        assert source_extension != target_extension, (
            f"${variable} aliases its source with the pre-existing {target_extension} target; "
            "the harness would mutate the source before Bridge execution"
        )
