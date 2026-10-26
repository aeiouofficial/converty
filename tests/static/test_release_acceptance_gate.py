from __future__ import annotations

import json
import subprocess
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
MANIFEST = ROOT / "eng/release-acceptance.json"
VERIFIER = ROOT / "scripts/verify_release_acceptance.py"
READINESS = ROOT / "scripts/verify_release_readiness.py"
PREFLIGHT = ROOT / "scripts/verify_release_inputs.py"


def run(*args: str) -> subprocess.CompletedProcess[str]:
    return subprocess.run([sys.executable, str(VERIFIER), *args], cwd=ROOT, text=True, capture_output=True, check=False)


def test_release_acceptance_manifest_has_explicit_boundary() -> None:
    data = json.loads(MANIFEST.read_text(encoding="utf-8"))
    assert data["schemaVersion"] == 1
    assert data["purpose"] == "production-release-acceptance"
    assert data["approvalStatus"] in {"OPEN", "APPROVED"}
    if data["approvalStatus"] == "OPEN":
        assert data["approved"] is False


def test_tracked_acceptance_state_is_structurally_valid_and_release_fail_closed_when_open() -> None:
    report = run("--json")
    assert report.returncode == 0, report.stderr
    payload = json.loads(report.stdout)
    required = run("--require-approved")
    assert required.returncode == (0 if payload["approved"] else 2)
    if payload["approved"]:
        assert payload["headedWindows11Approved"] is True
        assert payload["finalAcceptanceApproved"] is True


def test_release_preflight_validates_acceptance_contract_without_claiming_approval() -> None:
    text = PREFLIGHT.read_text(encoding="utf-8")
    assert "scripts/verify_release_acceptance.py" in text
    assert "verify_release_acceptance.py\", \"--require-approved" not in text


def test_release_readiness_consumes_headed_and_final_acceptance_status() -> None:
    text = READINESS.read_text(encoding="utf-8")
    assert "verify_release_acceptance.py" in text
    assert "headedWindows11Approved" in text
    assert "finalAcceptanceApproved" in text
    assert "headedWindows11ExplorerAcceptance" in text
    assert "finalSecurityFuzzChaosReleaseEndUserAcceptance" in text
