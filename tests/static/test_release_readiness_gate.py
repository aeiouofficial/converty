from __future__ import annotations

import json
import subprocess
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
SCRIPT = ROOT / "scripts/verify_release_readiness.py"
WORKFLOW = ROOT / ".github/workflows/release-readiness.yml"


def test_release_readiness_verifier_is_fail_closed_with_current_open_blockers() -> None:
    assert SCRIPT.is_file(), "release-readiness verifier must exist"
    result = subprocess.run(
        [sys.executable, str(SCRIPT), "--require-ready"],
        cwd=ROOT,
        text=True,
        capture_output=True,
        check=False,
    )
    assert result.returncode == 2, result.stdout + result.stderr
    output = (result.stdout + result.stderr).lower()
    for blocker in (
        "liverepositoryruleset",
        "mainbranchprotection",
        "headedwindows11exploreracceptance",
        "productionsignedpackageb2requalification",
        "productionffmpegredistributionapproval",
        "signedproductionmsixlifecycle",
        "finalsecurityfuzzchaosreleaseenduseracceptance",
    ):
        assert blocker.lower() in output


def test_release_readiness_verifier_emits_machine_readable_status() -> None:
    assert SCRIPT.is_file()
    result = subprocess.run(
        [sys.executable, str(SCRIPT), "--json"],
        cwd=ROOT,
        text=True,
        capture_output=True,
        check=False,
    )
    assert result.returncode == 0, result.stderr
    payload = json.loads(result.stdout)
    assert payload["schemaVersion"] == 1
    assert payload["workspaceVersion"] == (ROOT / "VERSION").read_text(encoding="utf-8").strip()
    assert payload["shipReady"] is False
    assert payload["openBlockers"]
    assert payload["liveGithubChecked"] is False


def test_release_readiness_workflow_is_manual_read_only_and_hash_locked() -> None:
    assert WORKFLOW.is_file(), "manual release-readiness workflow must exist"
    text = WORKFLOW.read_text(encoding="utf-8")
    assert "workflow_dispatch:" in text
    assert "push:" not in text
    assert "pull_request:" not in text
    assert "permissions:\n  contents: read" in text
    assert "persist-credentials: false" in text
    assert "actions/checkout@3d3c42e5aac5ba805825da76410c181273ba90b1" in text
    assert "actions/setup-python@5fda3b95a4ea91299a34e894583c3862153e4b97" in text
    assert "--require-hashes --only-binary=:all: -r eng/ci-python-requirements.txt" in text
    assert "python scripts/verify_release_readiness.py --require-ready --live-github" in text
    assert "contents: write" not in text
    assert "write-all" not in text


def test_release_readiness_verifier_checks_live_github_without_mutating_it() -> None:
    assert SCRIPT.is_file()
    text = SCRIPT.read_text(encoding="utf-8")
    for token in (
        "/rulesets",
        "/branches/main",
        "GITHUB_REPOSITORY",
        "GITHUB_TOKEN",
        "liveGithubChecked",
        "mainBranchProtected",
        "rulesetCount",
    ):
        assert token in text
    for forbidden in ("POST", "PATCH", "PUT", "DELETE"):
        assert forbidden not in text
