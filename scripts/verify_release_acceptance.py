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

from release_evidence import validate_approval_record, validate_evidence_reference

DEFAULT_MANIFEST = ROOT / "eng" / "release-acceptance.json"
HEADED = ("windows11ExactBuild", "explorerVersion", "headedExplorerEvidence", "contextMenuScreenshotEvidence", "crashHangFailureMatrixEvidence")
FINAL = ("fuzzEvidence", "chaosEvidence", "finalSecurityReviewEvidence", "endUserAcceptanceEvidence")
EVIDENCE = HEADED[2:] + FINAL


def load_status(manifest: Path = DEFAULT_MANIFEST) -> dict:
    try:
        data = json.loads(manifest.read_text(encoding="utf-8"))
    except (OSError, json.JSONDecodeError) as exc:
        raise ValueError(f"release acceptance manifest unreadable: {exc}") from exc
    if data.get("schemaVersion") != 1:
        raise ValueError("release acceptance schemaVersion must be 1")
    if data.get("purpose") != "production-release-acceptance":
        raise ValueError("release acceptance purpose must be production-release-acceptance")
    if data.get("approvalStatus") not in {"OPEN", "APPROVED"}:
        raise ValueError("release acceptance approvalStatus must be OPEN or APPROVED")
    if not isinstance(data.get("approved"), bool):
        raise ValueError("release acceptance approved must be boolean")
    required = (*HEADED, *FINAL, "approvalRecord")
    missing = [field for field in required if not data.get(field)]
    approved = data["approvalStatus"] == "APPROVED" and data["approved"] is True and not missing
    record = None
    artifact = None
    if approved:
        for field in ("windows11ExactBuild", "explorerVersion"):
            if not isinstance(data.get(field), str) or not data[field].strip():
                raise ValueError(f"{field} must be non-empty")
        for field in EVIDENCE:
            validate_evidence_reference(data.get(field), field)
        raw_record = data.get("approvalRecord")
        if not isinstance(raw_record, dict) or not raw_record.get("artifactSha256"):
            raise ValueError("approvalRecord.artifactSha256 is required")
        record = validate_approval_record(raw_record, artifact_sha256=raw_record["artifactSha256"])
        artifact = record["artifactSha256"]
    if data["approvalStatus"] == "APPROVED" and not approved:
        raise ValueError("APPROVED release acceptance requires all evidence and approval")
    if data["approved"] and data["approvalStatus"] != "APPROVED":
        raise ValueError("approved cannot be true while approvalStatus is OPEN")
    return {
        "schemaVersion": 1,
        "approvalStatus": data["approvalStatus"],
        "approved": approved,
        "headedWindows11Approved": approved and all(data.get(field) for field in HEADED),
        "finalAcceptanceApproved": approved and all(data.get(field) for field in FINAL),
        "artifactSha256": artifact,
        "approvalRecord": record,
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
        print(f"release acceptance evidence: INVALID: {exc}", file=sys.stderr)
        return 1
    if args.as_json:
        print(json.dumps(status, sort_keys=True))
    else:
        print(f"release acceptance evidence: {'APPROVED' if status['approved'] else 'OPEN'}")
        for field in status["missingEvidence"]:
            print(f"MISSING {field}")
    return 2 if args.require_approved and not status["approved"] else 0


if __name__ == "__main__":
    raise SystemExit(main())
