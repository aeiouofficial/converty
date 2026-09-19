#!/usr/bin/env python3
from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
MANIFEST = ROOT / "eng" / "ffmpeg-production.json"
EVIDENCE_FIELDS = (
    "version",
    "vendor",
    "archiveUrl",
    "archiveSha256",
    "ffmpegSha256",
    "ffprobeSha256",
    "signatureEvidence",
    "licenseEvidence",
    "noticeEvidence",
    "sourceProvenance",
    "approvalRecord",
)


def load_status() -> dict:
    try:
        data = json.loads(MANIFEST.read_text(encoding="utf-8"))
    except (OSError, json.JSONDecodeError) as exc:
        raise ValueError(f"production engine manifest unreadable: {exc}") from exc

    if data.get("schemaVersion") != 1:
        raise ValueError("production engine schemaVersion must be 1")
    if data.get("purpose") != "production-release":
        raise ValueError("production engine purpose must be production-release")
    if data.get("approvalStatus") not in {"OPEN", "APPROVED"}:
        raise ValueError("production engine approvalStatus must be OPEN or APPROVED")
    if not isinstance(data.get("approvedForRedistribution"), bool):
        raise ValueError("approvedForRedistribution must be boolean")

    missing = [field for field in EVIDENCE_FIELDS if not data.get(field)]
    approved = (
        data["approvalStatus"] == "APPROVED"
        and data["approvedForRedistribution"] is True
        and not missing
    )

    if data["approvalStatus"] == "APPROVED" and not approved:
        raise ValueError("APPROVED production engine requires complete evidence and redistribution approval")
    if data["approvedForRedistribution"] and data["approvalStatus"] != "APPROVED":
        raise ValueError("redistribution approval cannot be true while approvalStatus is OPEN")

    return {
        "schemaVersion": 1,
        "approvalStatus": data["approvalStatus"],
        "approved": approved,
        "missingEvidence": missing,
        "manifest": "eng/ffmpeg-production.json",
    }


def main() -> int:
    parser = argparse.ArgumentParser(description="Validate Converty production FFmpeg/ffprobe release evidence.")
    parser.add_argument("--json", action="store_true", dest="as_json")
    parser.add_argument("--require-approved", action="store_true")
    args = parser.parse_args()

    try:
        status = load_status()
    except ValueError as exc:
        print(f"production engine evidence: INVALID: {exc}", file=sys.stderr)
        return 1

    if args.as_json:
        print(json.dumps(status, sort_keys=True))
    else:
        print(f"production engine evidence: {'APPROVED' if status['approved'] else 'OPEN'}")
        for field in status["missingEvidence"]:
            print(f"MISSING {field}")

    if args.require_approved and not status["approved"]:
        return 2
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
