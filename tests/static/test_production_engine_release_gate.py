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
    return subprocess.run([sys.executable, str(VERIFIER), *args], cwd=ROOT, text=True, capture_output=True, check=False)


def test_production_engine_manifest_has_explicit_release_boundary() -> None:
    data = json.loads(MANIFEST.read_text(encoding="utf-8"))
    assert data["schemaVersion"] == 1
    assert data["purpose"] == "production-release"
    assert data["approvalStatus"] in {"OPEN", "APPROVED"}
    if data["approvalStatus"] == "OPEN":
        assert data["approvedForRedistribution"] is False
    assert "eng/ffmpeg-development.json" not in json.dumps(data.get("sourceProvenance"))


def test_tracked_production_engine_state_is_structurally_valid_and_release_fail_closed_when_open() -> None:
    report = run("--json")
    assert report.returncode == 0, report.stderr
    payload = json.loads(report.stdout)
    required = run("--require-approved")
    assert required.returncode == (0 if payload["approved"] else 2)


def test_release_preflight_validates_production_engine_contract_without_claiming_approval() -> None:
    text = PREFLIGHT.read_text(encoding="utf-8")
    assert "scripts/verify_production_engine.py" in text
    assert "verify_production_engine.py\", \"--require-approved" not in text


def test_release_readiness_consumes_production_engine_status() -> None:
    text = READINESS.read_text(encoding="utf-8")
    assert "verify_production_engine.py" in text
    assert "productionEngineApproved" in text
    assert "productionFfmpegRedistributionApproval" in text
