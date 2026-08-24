from __future__ import annotations

import json
import subprocess
import sys
import xml.etree.ElementTree as ET
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
CURRENT = "0.1.0-dev.5"
NEXT = "0.1.0-dev.6"


def run(*args: str) -> subprocess.CompletedProcess[str]:
    return subprocess.run([sys.executable, *args], cwd=ROOT, text=True, capture_output=True, check=False)


def test_current_version_is_dev5() -> None:
    assert (ROOT / "VERSION").read_text(encoding="utf-8").strip() == CURRENT


def test_provenance_authority_files_exist() -> None:
    required = [
        ROOT / "docs/supply-chain/CI_PROVENANCE_POLICY.md",
        ROOT / "machine-readable/ci_action_pins.json",
        ROOT / "scripts/verify_ci_actions.py",
        ROOT / "scripts/verify_dependency_audit.py",
        ROOT / "build/dependency-audit.ps1",
        ROOT / "tests/fixtures/dependency-audit/clean.json",
        ROOT / "tests/fixtures/dependency-audit/vulnerable.json",
    ]
    assert all(path.is_file() for path in required)


def test_ci_actions_are_immutable_and_match_reviewed_pin_manifest() -> None:
    result = run("scripts/verify_ci_actions.py")
    assert result.returncode == 0, result.stderr or result.stdout
    assert "CI action pins: PASS" in result.stdout


def test_nuget_audit_is_explicit_all_low_and_uses_vulnerability_only_source() -> None:
    props = ET.parse(ROOT / "Directory.Build.props").getroot()
    values = {child.tag: (child.text or "").strip() for group in props for child in group}
    assert values["NuGetAudit"] == "true"
    assert values["NuGetAuditMode"] == "all"
    assert values["NuGetAuditLevel"] == "low"
    config = ET.parse(ROOT / "NuGet.Config").getroot()
    audit_sources = config.find("auditSources")
    assert audit_sources is not None
    adds = audit_sources.findall("add")
    assert len(adds) == 1
    assert adds[0].attrib["value"] == "https://data.nuget.org/v3/index.json"


def test_dependency_audit_verifier_accepts_clean_report() -> None:
    result = run("scripts/verify_dependency_audit.py", "tests/fixtures/dependency-audit/clean.json")
    assert result.returncode == 0, result.stderr or result.stdout
    assert "dependency audit: PASS" in result.stdout


def test_dependency_audit_verifier_rejects_vulnerability() -> None:
    result = run("scripts/verify_dependency_audit.py", "tests/fixtures/dependency-audit/vulnerable.json")
    assert result.returncode != 0
    output = result.stderr + result.stdout
    assert "dependency audit: FAIL" in output
    assert "Example.Vulnerable" in output
    assert "GHSA-aaaa-bbbb-cccc" in output


def test_dependency_audit_verifier_rejects_wrong_output_version() -> None:
    result = run("scripts/verify_dependency_audit.py", "tests/fixtures/dependency-audit/wrong-version.json")
    assert result.returncode != 0
    assert "version 1" in (result.stderr + result.stdout)


def test_dependency_audit_verifier_rejects_missing_projects_shape() -> None:
    result = run("scripts/verify_dependency_audit.py", "tests/fixtures/dependency-audit/missing-projects.json")
    assert result.returncode != 0
    assert "projects" in (result.stderr + result.stdout)


def test_ci_runs_dependency_audit_after_locked_restore() -> None:
    workflow = (ROOT / ".github/workflows/ci.yml").read_text(encoding="utf-8")
    assert "./build/dependency-audit.ps1" in workflow
    assert workflow.index("./build/bootstrap.ps1") < workflow.index("./build/dependency-audit.ps1")


def test_build_verify_includes_dependency_audit() -> None:
    verify = (ROOT / "build/verify.ps1").read_text(encoding="utf-8")
    assert "dependency-audit.ps1" in verify


def test_handover_and_release_authority_are_current() -> None:
    handover = json.loads((ROOT / "machine-readable/handover_state.json").read_text(encoding="utf-8"))
    evidence = json.loads((ROOT / "machine-readable/build_evidence.json").read_text(encoding="utf-8"))
    release = json.loads((ROOT / "machine-readable/release_policy.json").read_text(encoding="utf-8"))
    pins = json.loads((ROOT / "machine-readable/ci_action_pins.json").read_text(encoding="utf-8"))
    assert handover["workspaceVersion"] == CURRENT
    assert handover["nextWorkspaceVersion"] == NEXT
    assert evidence["workspaceVersion"] == CURRENT
    assert release["workspaceVersion"] == CURRENT
    assert pins["workspaceVersion"] == CURRENT


def test_ci_checkout_does_not_persist_credentials_and_jobs_have_timeouts() -> None:
    workflow = (ROOT / ".github/workflows/ci.yml").read_text(encoding="utf-8")
    assert workflow.count("persist-credentials: false") == 2
    assert "timeout-minutes: 15" in workflow
    assert "timeout-minutes: 30" in workflow


def test_release_policy_encodes_ci_credential_and_timeout_bounds() -> None:
    policy = json.loads((ROOT / "machine-readable/release_policy.json").read_text(encoding="utf-8"))
    ci = policy["ciProvenance"]
    assert ci["checkoutPersistCredentials"] is False
    assert ci["workflowPermissions"] == {"contents": "read"}
    assert ci["jobTimeoutMinutes"] == {"managed": 30, "supply-chain-static": 15}
