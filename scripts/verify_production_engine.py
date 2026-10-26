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

DEFAULT_MANIFEST = ROOT / "eng" / "ffmpeg-production.json"
EVIDENCE_FIELDS = ("signatureEvidence", "licenseEvidence", "noticeEvidence", "sourceProvenance")


def load_status(manifest: Path = DEFAULT_MANIFEST) -> dict:
    try:
        data = json.loads(manifest.read_text(encoding="utf-8"))
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

    required = ("version", "vendor", "archiveUrl", "archiveSha256", "ffmpegSha256", "ffprobeSha256", *EVIDENCE_FIELDS, "approvalRecord")
    missing = [field for field in required if not data.get(field)]
    approved = data["approvalStatus"] == "APPROVED" and data["approvedForRedistribution"] is True and not missing
    approval_record = None
    artifact_sha256 = None

    if approved:
        for field in ("version", "vendor"):
            value = data.get(field)
            if not isinstance(value, str) or not value.strip() or len(value) > 256:
                raise ValueError(f"{field} must be a bounded non-empty string")
        archive_url = data.get("archiveUrl")
        if not isinstance(archive_url, str) or not archive_url.startswith("https://") or len(archive_url) > 2048:
            raise ValueError("archiveUrl must be a bounded https URL")
        artifact_sha256 = require_sha256(data.get("archiveSha256"), "archiveSha256")
        require_sha256(data.get("ffmpegSha256"), "ffmpegSha256")
        require_sha256(data.get("ffprobeSha256"), "ffprobeSha256")
        for field in EVIDENCE_FIELDS:
            validate_evidence_reference(data.get(field), field)
        provenance = data["sourceProvenance"]["reference"].replace("\\", "/").lower()
        if provenance.endswith("eng/ffmpeg-development.json"):
            raise ValueError("development FFmpeg manifest cannot be production source provenance")
        approval_record = validate_approval_record(data.get("approvalRecord"), artifact_sha256=artifact_sha256)

    if data["approvalStatus"] == "APPROVED" and not approved:
        raise ValueError("APPROVED production engine requires complete evidence and redistribution approval")
    if data["approvedForRedistribution"] and data["approvalStatus"] != "APPROVED":
        raise ValueError("redistribution approval cannot be true while approvalStatus is OPEN")

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
        print(f"production engine evidence: INVALID: {exc}", file=sys.stderr)
        return 1
    if args.as_json:
        print(json.dumps(status, sort_keys=True))
    else:
        print(f"production engine evidence: {'APPROVED' if status['approved'] else 'OPEN'}")
        for field in status["missingEvidence"]:
            print(f"MISSING {field}")
    return 2 if args.require_approved and not status["approved"] else 0


if __name__ == "__main__":
    raise SystemExit(main())
