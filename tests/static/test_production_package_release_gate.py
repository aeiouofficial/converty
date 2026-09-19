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
    return subprocess.run(
        [sys.executable, str(VERIFIER), *args],
        cwd=ROOT,
        text=True,
        capture_output=True,
        check=False,
    )


def test_production_package_manifest_is_explicitly_open_and_contains_no_fake_identity() -> None:
    assert MANIFEST.is_file()
    data = json.loads(MANIFEST.read_text(encoding="utf-8"))
    assert data["schemaVersion"] == 1
    assert data["purpose"] == "production-msix"
    assert data["approvalStatus"] == "OPEN"
    assert data["approved"] is False
    for field in (
        "publisherSubject",
        "signerCertificateSha256",
        "packageFamilyName",
        "msixSha256",
        "authenticodeEvidence",
        "msixSignatureEvidence",
        "timestampEvidence",
        "b2ProductionIdentityEvidence",
        "cleanWindows11LifecycleEvidence",
    ):
        assert data[field] is None
    text = MANIFEST.read_text(encoding="utf-8")
    assert "CN=Converty Development" not in text
    assert "Converty.Dev_" not in text


def test_production_package_verifier_fails_closed_until_signed_lifecycle_evidence_exists() -> None:
    assert VERIFIER.is_file()
    report = run("--json")
    assert report.returncode == 0, report.stderr
    payload = json.loads(report.stdout)
    assert payload["approved"] is False
    assert payload["approvalStatus"] == "OPEN"
    assert payload["missingEvidence"]
    required = run("--require-approved")
    assert required.returncode == 2
    output = (required.stdout + required.stderr).lower()
    for token in (
        "publishersubject",
        "signercertificatesha256",
        "packagefamilyname",
        "msixsha256",
        "authenticodeevidence",
        "msixsignatureevidence",
        "timestampevidence",
        "b2productionidentityevidence",
        "cleanwindows11lifecycleevidence",
    ):
        assert token.lower() in output


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
