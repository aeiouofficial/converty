from __future__ import annotations

import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
WORKFLOW = ROOT / ".github/workflows/ci.yml"
LOCK = ROOT / "eng/ci-python-requirements.txt"
GOVERNANCE = ROOT / "docs/supply-chain/REPOSITORY_GOVERNANCE_POLICY.md"


def test_python_ci_dependencies_are_committed_and_hash_verified() -> None:
    assert LOCK.is_file(), "CI Python lock file must be committed"
    text = LOCK.read_text(encoding="utf-8")
    assert "jsonschema==4.26.0" in text
    assert "pytest==9.0.2" in text
    package_lines = [
        line for line in text.splitlines()
        if line and not line.startswith(("#", " ", "\t", "-"))
    ]
    assert package_lines, "lock file must contain pinned package entries"
    for line in package_lines:
        assert re.fullmatch(r"[A-Za-z0-9_.-]+==[^\\s\\]+ \\\\", line), line
    assert text.count("--hash=sha256:") >= len(package_lines)
    assert "--require-hashes" not in text


def test_ci_installs_only_from_hash_locked_python_requirements() -> None:
    workflow = WORKFLOW.read_text(encoding="utf-8")
    locked_install = (
        "python -m pip install --disable-pip-version-check --no-input "
        "--require-hashes --only-binary=:all: -r eng/ci-python-requirements.txt"
    )
    assert workflow.count(locked_install) == 2
    assert '"jsonschema==4.26.0"' not in workflow
    assert '"pytest==9.0.2"' not in workflow


def test_permanent_workflow_permissions_remain_read_only() -> None:
    workflow = WORKFLOW.read_text(encoding="utf-8")
    assert re.search(r"(?m)^permissions:\\n  contents: read$", workflow)
    assert "write-all" not in workflow
    assert not re.search(r"(?m)^\\s+[A-Za-z-]+: write\\s*$", workflow)


def test_future_main_governance_policy_is_explicit_but_not_claimed_live() -> None:
    assert GOVERNANCE.is_file(), "future-main governance policy must be committed"
    text = GOVERNANCE.read_text(encoding="utf-8").lower()
    for phrase in (
        "desired future main ruleset",
        "block force pushes",
        "block deletions",
        "restrict updates",
        "linear history",
        "main-authority-continuity",
        "supply-chain-static",
        "managed",
        "verified signatures",
        "does not claim live configuration",
        "historical unsigned dev.20",
        "exact candidate",
    ):
        assert phrase in text
