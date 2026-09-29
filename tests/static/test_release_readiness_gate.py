from __future__ import annotations

import json
import subprocess
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
SCRIPT = ROOT / "scripts/verify_release_readiness.py"
CI = ROOT / ".github/workflows/ci.yml"


def test_release_readiness_is_fail_closed_offline() -> None:
    result = subprocess.run([sys.executable, str(SCRIPT), "--require-ready"], cwd=ROOT, text=True, capture_output=True, check=False)
    assert result.returncode == 2
    assert "liveGithubEvidenceUnavailable".lower() in (result.stdout + result.stderr).lower()


def test_release_readiness_verifier_emits_non_authoritative_offline_status() -> None:
    result = subprocess.run([sys.executable, str(SCRIPT), "--json"], cwd=ROOT, text=True, capture_output=True, check=False)
    assert result.returncode == 0, result.stderr
    payload = json.loads(result.stdout)
    assert payload["schemaVersion"] == 1
    assert payload["workspaceVersion"] == (ROOT / "VERSION").read_text(encoding="utf-8").strip()
    assert payload["shipReady"] is False
    assert payload["liveGithubChecked"] is False
    assert "liveGithubEvidenceUnavailable" in payload["openBlockers"]


def test_candidate_readiness_ci_is_read_only_and_hash_locked() -> None:
    text = CI.read_text(encoding="utf-8")
    assert "candidate-release-readiness:" in text
    assert "contents: read" in text
    assert "actions: read" in text
    assert "actions/checkout@3d3c42e5aac5ba805825da76410c181273ba90b1" in text
    assert "actions/setup-python@5fda3b95a4ea91299a34e894583c3862153e4b97" in text
    assert "python scripts/verify_release_readiness.py --require-ready --live-github" in text
    assert "GITHUB_TOKEN: ${{ github.token }}" in text
    assert "contents: write" not in text


def test_release_readiness_verifier_uses_only_read_requests() -> None:
    text = SCRIPT.read_text(encoding="utf-8")
    for token in ("/rulesets", "/branches/main", "GITHUB_REPOSITORY", "GITHUB_TOKEN", "GITHUB_SHA"):
        assert token in text
    for forbidden in ("method=\"POST\"", "method=\"PATCH\"", "method=\"PUT\"", "method=\"DELETE\""):
        assert forbidden not in text
