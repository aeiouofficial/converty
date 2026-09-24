#!/usr/bin/env python3
from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
SCRIPTS = ROOT / "scripts"
if str(SCRIPTS) not in sys.path:
    sys.path.insert(0, str(SCRIPTS))

from release_evidence import require_sha256, validate_approval_record, validate_evidence_reference

DEFAULT_MANIFEST = ROOT / "eng" / "windows-production-package.json"
EVIDENCE_FIELDS = (
    "authenticodeEvidence", "msixSignatureEvidence", "timestampEvidence",
    "b2ProductionIdentityEvidence", "cleanWindows11LifecycleEvidence",
)


def load_status(manifest: Path = DEFAULT_MANIFEST) -> dict:
    try:
        data = json.loads(manifest.read_text(encoding="utf-8"))
    except (OSError, json.JSONDecodeError) as exc:
        raise ValueError(f"production package manifest unreadable: {exc}") from exc
    if data.get("schemaVersion") != 1:
        raise ValueError("production package schemaVersion must be 1")
    if data.get("purpose") != "production-msix":
        raise ValueError("production package purpose must be production-msix")
    if data.get("approvalStatus") not in {"OPEN", "APPROVED"}:
        raise ValueError("production package approvalStatus must be OPEN or APPROVED")
    if not isinstance(data.get("approved"), bool):
        raise ValueError("production package approved must be boolean")

    publisher = str(data.get("publisherSubject") or "")
    family = str(data.get("packageFamilyName") or "")
    if publisher == "CN=Converty Development" or family.startswith("Converty.Dev_"):
        raise ValueError("development package identity cannot satisfy production package evidence")

    required = ("publisherSubject", "signerCertificateSha256", "packageFamilyName", "msixSha256", *EVIDENCE_FIELDS, "approvalRecord")
    missing = [field for field in required if not data.get(field)]
    approved = data["approvalStatus"] == "APPROVED" and data["approved"] is True and not missing
    approval_record = None
    artifact_sha256 = None
    if approved:
        if not publisher.strip():
            raise ValueError("publisherSubject must be non-empty")
        if not family.strip():
            raise ValueError("packageFamilyName must be non-empty")
        require_sha256(data.get("signerCertificateSha256"), "signerCertificateSha256")
        artifact_sha256 = require_sha256(data.get("msixSha256"), "msixSha256")
        for field in EVIDENCE_FIELDS:
            validate_evidence_reference(data.get(field), field)
        approval_record = validate_approval_record(data.get("approvalRecord"), artifact_sha256=artifact_sha256)

    if data["approvalStatus"] == "APPROVED" and not approved:
        raise ValueError("APPROVED production package requires complete signed lifecycle evidence")
    if data["approved"] and data["approvalStatus"] != "APPROVED":
        raise ValueError("approved cannot be true while approvalStatus is OPEN")
    return {
        "schemaVersion": 1,
        "approvalStatus": data["approvalStatus"],
        "approved": approved,
        "artifactSha256": artifact_sha256,
        "approvalRecord": approval_record,
        "missingEvidence": missing,
        "manifest": str(manifest),
    }


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--manifest", type=Path, default=DEFAULT_MANIFEST)
    parser.add_argument("--json", action="store_true", dest="as_json")
    parser.add_argument("--require-approved", action="store_true")
    args = parser.parse_args()
    try:
        status = load_status(args.manifest.resolve())
    except ValueError as exc:
        print(f"production package evidence: INVALID: {exc}", file=sys.stderr)
        return 1
    if args.as_json:
        print(json.dumps(status, sort_keys=True))
    else:
        print(f"production package evidence: {'APPROVED' if status['approved'] else 'OPEN'}")
        for field in status["missingEvidence"]:
            print(f"MISSING {field}")
    return 2 if args.require_approved and not status["approved"] else 0


if __name__ == "__main__":
    raise SystemExit(main())
