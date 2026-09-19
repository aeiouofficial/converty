#!/usr/bin/env python3
from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
MANIFEST = ROOT / "eng" / "windows-production-package.json"
EVIDENCE_FIELDS = (
    "publisherSubject",
    "signerCertificateSha256",
    "packageFamilyName",
    "msixSha256",
    "authenticodeEvidence",
    "msixSignatureEvidence",
    "timestampEvidence",
    "b2ProductionIdentityEvidence",
    "cleanWindows11LifecycleEvidence",
    "approvalRecord",
)


def load_status() -> dict:
    try:
        data = json.loads(MANIFEST.read_text(encoding="utf-8"))
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

    forbidden_identity = (
        str(data.get("publisherSubject") or "") == "CN=Converty Development"
        or str(data.get("packageFamilyName") or "").startswith("Converty.Dev_")
    )
    if forbidden_identity:
        raise ValueError("development package identity cannot satisfy production package evidence")

    missing = [field for field in EVIDENCE_FIELDS if not data.get(field)]
    approved = data["approvalStatus"] == "APPROVED" and data["approved"] is True and not missing

    if data["approvalStatus"] == "APPROVED" and not approved:
        raise ValueError("APPROVED production package requires complete signed lifecycle evidence")
    if data["approved"] and data["approvalStatus"] != "APPROVED":
        raise ValueError("approved cannot be true while approvalStatus is OPEN")

    return {
        "schemaVersion": 1,
        "approvalStatus": data["approvalStatus"],
        "approved": approved,
        "missingEvidence": missing,
        "manifest": "eng/windows-production-package.json",
    }


def main() -> int:
    parser = argparse.ArgumentParser(description="Validate Converty signed production MSIX/B2 release evidence.")
    parser.add_argument("--json", action="store_true", dest="as_json")
    parser.add_argument("--require-approved", action="store_true")
    args = parser.parse_args()

    try:
        status = load_status()
    except ValueError as exc:
        print(f"production package evidence: INVALID: {exc}", file=sys.stderr)
        return 1

    if args.as_json:
        print(json.dumps(status, sort_keys=True))
    else:
        print(f"production package evidence: {'APPROVED' if status['approved'] else 'OPEN'}")
        for field in status["missingEvidence"]:
            print(f"MISSING {field}")

    if args.require_approved and not status["approved"]:
        return 2
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
