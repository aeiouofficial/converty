from __future__ import annotations

import importlib.util
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
VERIFIER = ROOT / "scripts" / "verify_main_continuity.py"
AGENT_CONTRACT = ROOT / "AGENTS.md"
CI_WORKFLOW = ROOT / ".github" / "workflows" / "ci.yml"


def load_verifier_module():
    assert VERIFIER.is_file(), "main-authority continuity verifier is missing"
    spec = importlib.util.spec_from_file_location("verify_main_continuity", VERIFIER)
    assert spec is not None and spec.loader is not None
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


def test_main_push_is_authoritative() -> None:
    module = load_verifier_module()
    result = module.verify_main_continuity(
        event_name="push",
        ref_name="main",
        head_sha="abc123",
        is_ancestor=lambda *_: False,
    )
    assert result.ok
    assert "main" in result.message.lower()


def test_pull_request_event_remains_reviewable() -> None:
    module = load_verifier_module()
    result = module.verify_main_continuity(
        event_name="pull_request",
        ref_name="feature/example",
        head_sha="abc123",
        is_ancestor=lambda *_: False,
    )
    assert result.ok


def test_side_branch_push_passes_only_if_already_contained_in_main() -> None:
    module = load_verifier_module()
    result = module.verify_main_continuity(
        event_name="push",
        ref_name="dev/example",
        head_sha="abc123",
        is_ancestor=lambda head, main: head == "abc123" and main == "origin/main",
    )
    assert result.ok
    assert "contained" in result.message.lower()


def test_side_branch_push_ahead_of_main_is_development_only() -> None:
    module = load_verifier_module()
    result = module.verify_main_continuity(
        event_name="push",
        ref_name="dev/example",
        head_sha="abc123",
        is_ancestor=lambda *_: False,
    )
    assert not result.ok
    assert "development-only" in result.message.lower()
    assert "main" in result.message.lower()
    assert "completion" in result.message.lower()


def test_agent_contract_requires_main_first_authority_and_live_completion_check() -> None:
    assert AGENT_CONTRACT.is_file(), "AGENTS.md operating contract is missing"
    text = AGENT_CONTRACT.read_text(encoding="utf-8").lower()
    assert "main is the repository authority" in text
    assert "push durable commits immediately" in text
    assert "side branch" in text and "temporary" in text
    assert "qualified sha" in text and "current `main` head" in text
    assert "side-branch-only" in text


def test_ci_contains_main_authority_continuity_gate() -> None:
    text = CI_WORKFLOW.read_text(encoding="utf-8")
    assert "main-authority-continuity:" in text
    assert "fetch-depth: 0" in text
    assert "Verify default-branch authority continuity" in text
    assert "python scripts/verify_main_continuity.py" in text
    assert "+refs/heads/main:refs/remotes/origin/main" in text
