from __future__ import annotations

import importlib.util
import json
import subprocess
import sys
from pathlib import Path
from types import SimpleNamespace

ROOT = Path(__file__).resolve().parents[2]
ENGINE = ROOT / "scripts" / "verify_production_engine.py"
PACKAGE = ROOT / "scripts" / "verify_production_package.py"
READINESS = ROOT / "scripts" / "verify_release_readiness.py"
CANDIDATE = ROOT / "scripts" / "verify_candidate_continuity.py"
SECRETS = ROOT / "scripts" / "verify_workspace_secrets.py"
CI = ROOT / ".github" / "workflows" / "ci.yml"
POLICY = ROOT / "docs" / "supply-chain" / "REPOSITORY_GOVERNANCE_POLICY.md"
PACKAGE_VERIFY = ROOT / "build" / "verify-production-package.ps1"


def load_module(path: Path, name: str):
    spec = importlib.util.spec_from_file_location(name, path)
    assert spec is not None and spec.loader is not None
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


def evidence_ref(reference: str) -> dict:
    return {
        "kind": "external-review",
        "reference": reference,
        "sha256": "a" * 64,
    }


def approval(artifact_sha256: str) -> dict:
    return {
        "repository": "aeiouofficial/converty",
        "subjectCommitSha": "1" * 40,
        "subjectTreeSha": "2" * 40,
        "artifactSha256": artifact_sha256,
        "workflowRunId": 123456,
        "approvedBy": "release-review",
        "approvedAt": "2026-09-21T16:00:00Z",
    }


def approved_engine() -> dict:
    artifact = "3" * 64
    return {
        "schemaVersion": 1,
        "purpose": "production-release",
        "approvalStatus": "APPROVED",
        "approvedForRedistribution": True,
        "version": "9.0.1",
        "vendor": "Approved Vendor",
        "archiveUrl": "https://example.invalid/ffmpeg.zip",
        "archiveSha256": artifact,
        "ffmpegSha256": "4" * 64,
        "ffprobeSha256": "5" * 64,
        "signatureEvidence": evidence_ref("release/signature.json"),
        "licenseEvidence": evidence_ref("release/license.txt"),
        "noticeEvidence": evidence_ref("release/notice.txt"),
        "sourceProvenance": evidence_ref("release/source-provenance.json"),
        "approvalRecord": approval(artifact),
    }


def approved_package() -> dict:
    artifact = "6" * 64
    return {
        "schemaVersion": 1,
        "purpose": "production-msix",
        "approvalStatus": "APPROVED",
        "approved": True,
        "publisherSubject": "CN=Converty Production",
        "signerCertificateSha256": "7" * 64,
        "packageFamilyName": "Converty_abcdefghijk",
        "msixSha256": artifact,
        "authenticodeEvidence": evidence_ref("release/authenticode.json"),
        "msixSignatureEvidence": evidence_ref("release/msix-signature.json"),
        "timestampEvidence": evidence_ref("release/timestamp.json"),
        "b2ProductionIdentityEvidence": evidence_ref("release/b2.json"),
        "cleanWindows11LifecycleEvidence": evidence_ref("release/windows11-lifecycle.json"),
        "approvalRecord": approval(artifact),
    }


def run_manifest(verifier: Path, tmp_path: Path, payload: dict) -> subprocess.CompletedProcess[str]:
    manifest = tmp_path / "manifest.json"
    manifest.write_text(json.dumps(payload), encoding="utf-8")
    return subprocess.run(
        [sys.executable, str(verifier), "--manifest", str(manifest), "--require-approved"],
        cwd=ROOT,
        text=True,
        capture_output=True,
        check=False,
    )


def test_production_engine_rejects_malformed_hash(tmp_path: Path) -> None:
    data = approved_engine()
    data["archiveSha256"] = "not-a-sha256"
    result = run_manifest(ENGINE, tmp_path, data)
    assert result.returncode == 1
    assert "archiveSha256" in result.stderr


def test_production_engine_rejects_development_manifest_as_provenance(tmp_path: Path) -> None:
    data = approved_engine()
    data["sourceProvenance"] = evidence_ref("eng/ffmpeg-development.json")
    result = run_manifest(ENGINE, tmp_path, data)
    assert result.returncode == 1
    assert "development" in result.stderr.lower()


def test_production_package_rejects_malformed_msix_hash(tmp_path: Path) -> None:
    data = approved_package()
    data["msixSha256"] = "fake"
    result = run_manifest(PACKAGE, tmp_path, data)
    assert result.returncode == 1
    assert "msixSha256" in result.stderr


def test_production_package_requires_real_artifact_verification_script() -> None:
    assert PACKAGE_VERIFY.is_file()
    text = PACKAGE_VERIFY.read_text(encoding="utf-8")
    assert "Get-FileHash" in text
    assert "Get-AuthenticodeSignature" in text
    assert "signtool" in text.lower()


def test_offline_readiness_cannot_be_ship_authority(tmp_path: Path, monkeypatch) -> None:
    module = load_module(READINESS, "dev22_readiness")
    evidence = tmp_path / "build_evidence.json"
    evidence.write_text(json.dumps({"releaseBlockers": {}}), encoding="utf-8")
    version = tmp_path / "VERSION"
    version.write_text("0.1.0-dev.22\n", encoding="utf-8")
    monkeypatch.setattr(module, "BUILD_EVIDENCE", evidence)
    monkeypatch.setattr(module, "VERSION", version)

    outputs = [
        {"approved": True, "approvalRecord": approval("3" * 64)},
        {"approved": True, "approvalRecord": approval("6" * 64)},
        {
            "approved": True,
            "headedWindows11Approved": True,
            "finalAcceptanceApproved": True,
            "approvalRecord": approval("6" * 64),
        },
    ]
    calls = iter(outputs)

    def fake_run(*args, **kwargs):
        payload = next(calls)
        return SimpleNamespace(returncode=0, stdout=json.dumps(payload), stderr="")

    monkeypatch.setattr(module.subprocess, "run", fake_run)
    status = module.collect_status(False)
    assert status["shipReady"] is False
    assert "liveGithubEvidenceUnavailable" in status["openBlockers"]


def test_governance_requires_active_main_ruleset_with_exact_checks() -> None:
    module = load_module(READINESS, "dev22_governance")
    assert hasattr(module, "evaluate_main_governance")
    weak = [{
        "enforcement": "evaluate",
        "target": "branch",
        "conditions": {"ref_name": {"include": ["refs/heads/main"], "exclude": []}},
        "bypass_actors": [],
        "rules": [],
    }]
    assert module.evaluate_main_governance(weak)

    strong = [{
        "enforcement": "active",
        "target": "branch",
        "conditions": {"ref_name": {"include": ["refs/heads/main"], "exclude": []}},
        "bypass_actors": [],
        "rules": [
            {"type": "deletion"},
            {"type": "non_fast_forward"},
            {"type": "required_linear_history"},
            {"type": "required_signatures"},
            {
                "type": "required_status_checks",
                "parameters": {
                    "required_status_checks": [
                        {"context": "candidate-base-continuity"},
                        {"context": "supply-chain-static"},
                        {"context": "managed"},
                        {"context": "candidate-release-readiness"},
                    ]
                },
            },
        ],
    }]
    assert module.evaluate_main_governance(strong) == []


def test_candidate_pre_promotion_continuity_is_not_circular() -> None:
    assert CANDIDATE.is_file()
    module = load_module(CANDIDATE, "dev22_candidate_continuity")
    result = module.verify_candidate_continuity(
        head_sha="b" * 40,
        expected_main_sha="a" * 40,
        actual_main_sha="a" * 40,
        is_ancestor=lambda base, head: (base, head) == ("a" * 40, "b" * 40),
    )
    assert result.ok is True


def test_ci_and_policy_require_candidate_readiness_without_manual_bootstrap() -> None:
    ci = CI.read_text(encoding="utf-8")
    assert "candidate-base-continuity:" in ci
    assert "candidate-release-readiness:" in ci
    assert "verify_release_readiness.py --require-ready --live-github" in ci
    policy = POLICY.read_text(encoding="utf-8")
    for check in (
        "candidate-base-continuity",
        "supply-chain-static",
        "managed",
        "candidate-release-readiness",
    ):
        assert check in policy


def test_workspace_secret_scan_catches_private_key_in_plain_text(tmp_path: Path) -> None:
    assert SECRETS.is_file()
    marker = "-----BEGIN " + "PRIVATE KEY-----"
    (tmp_path / "notes.txt").write_text(marker + "\nnot-real-key-material\n", encoding="utf-8")
    result = subprocess.run(
        [sys.executable, str(SECRETS), "--root", str(tmp_path)],
        cwd=ROOT,
        text=True,
        capture_output=True,
        check=False,
    )
    assert result.returncode != 0
    assert "notes.txt" in (result.stdout + result.stderr)
