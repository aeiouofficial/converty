from __future__ import annotations

import json
import subprocess
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
MANIFEST = ROOT / "eng/ffmpeg-production.json"
VERIFIER = ROOT / "scripts/verify_production_engine.py"
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


def test_production_engine_manifest_is_explicit_and_not_the_development_pin() -> None:
    assert MANIFEST.is_file()
    data = json.loads(MANIFEST.read_text(encoding="utf-8"))
    assert data["schemaVersion"] == 1
    assert data["purpose"] == "production-release"
    assert data["approvalStatus"] == "OPEN"
    assert data["approvedForRedistribution"] is False
    assert data["version"] is None
    assert data["archiveUrl"] is None
    assert data["archiveSha256"] is None
    assert data["ffmpegSha256"] is None
    assert data["ffprobeSha256"] is None
    assert data["signatureEvidence"] is None
    assert data["licenseEvidence"] is None
    assert data["noticeEvidence"] is None
    assert data["sourceProvenance"] is None
    assert "gyan.dev/ffmpeg/builds/packages/ffmpeg-9.0.1-essentials_build.zip" not in MANIFEST.read_text(encoding="utf-8")


def test_production_engine_verifier_reports_open_and_fails_closed_for_release() -> None:
    assert VERIFIER.is_file()
    report = run("--json")
    assert report.returncode == 0, report.stderr
    payload = json.loads(report.stdout)
    assert payload["schemaVersion"] == 1
    assert payload["approved"] is False
    assert payload["approvalStatus"] == "OPEN"
    assert payload["missingEvidence"]
    required = run("--require-approved")
    assert required.returncode == 2
    output = (required.stdout + required.stderr).lower()
    for token in ("archiveurl", "archivesha256", "ffmpegsha256", "ffprobesha256", "signatureevidence", "licenseevidence", "noticeevidence", "sourceprovenance"):
        assert token.lower() in output


def test_release_preflight_validates_production_engine_contract_without_claiming_approval() -> None:
    text = PREFLIGHT.read_text(encoding="utf-8")
    assert "scripts/verify_production_engine.py" in text
    assert "--require-approved" not in text


def test_release_readiness_consumes_production_engine_status() -> None:
    text = READINESS.read_text(encoding="utf-8")
    assert "verify_production_engine.py" in text
    assert "productionEngineApproved" in text
    assert "productionFfmpegRedistributionApproval" in text
