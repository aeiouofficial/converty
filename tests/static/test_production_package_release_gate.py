from __future__ import annotations

import json
import subprocess
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
MANIFEST = ROOT / "eng/windows-production-package.json"
VERIFIER = ROOT / "scripts/verify_production_package.py"
READINESS = ROOT / "scripts/verify_release_readiness.py"
PREFLIGHT = ROOT / "scripts/verify_release_inputs.py"


def run(*args: str) -> subprocess.CompletedProcess[str]:
    return subprocess.run([sys.executable, str(VERIFIER), *args], cwd=ROOT, text=True, capture_output=True, check=False)


def test_production_package_manifest_has_explicit_release_boundary() -> None:
    data = json.loads(MANIFEST.read_text(encoding="utf-8"))
    assert data["schemaVersion"] == 1
    assert data["purpose"] == "production-msix"
    assert data["approvalStatus"] in {"OPEN", "APPROVED"}
    if data["approvalStatus"] == "OPEN":
        assert data["approved"] is False
    assert data.get("publisherSubject") != "CN=Converty Development"
    assert not str(data.get("packageFamilyName") or "").startswith("Converty.Dev_")


def test_tracked_production_package_state_is_structurally_valid_and_release_fail_closed_when_open() -> None:
    report = run("--json")
    assert report.returncode == 0, report.stderr
    payload = json.loads(report.stdout)
    required = run("--require-approved")
    assert required.returncode == (0 if payload["approved"] else 2)


def test_release_preflight_validates_production_package_contract_without_claiming_approval() -> None:
    text = PREFLIGHT.read_text(encoding="utf-8")
    assert "scripts/verify_production_package.py" in text
    assert "verify_production_package.py\", \"--require-approved" not in text


def test_release_readiness_consumes_signed_package_and_b2_status() -> None:
    text = READINESS.read_text(encoding="utf-8")
    assert "verify_production_package.py" in text
    assert "productionPackageApproved" in text
    assert "productionSignedPackageB2Requalification" in text
    assert "signedProductionMsixLifecycle" in text
