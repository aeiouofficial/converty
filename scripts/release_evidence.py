from __future__ import annotations

import re
from datetime import datetime
from typing import Any

_SHA40 = re.compile(r"^[0-9a-fA-F]{40}$")
_SHA256 = re.compile(r"^[0-9a-fA-F]{64}$")
_EVIDENCE_KINDS = {"file", "url", "github-artifact", "external-review"}


def require_sha40(value: Any, field: str) -> str:
    if not isinstance(value, str) or _SHA40.fullmatch(value) is None:
        raise ValueError(f"{field} must be a 40-character hexadecimal Git SHA")
    return value.lower()


def require_sha256(value: Any, field: str) -> str:
    if not isinstance(value, str) or _SHA256.fullmatch(value) is None:
        raise ValueError(f"{field} must be a 64-character hexadecimal SHA-256")
    return value.lower()


def validate_evidence_reference(value: Any, field: str) -> dict:
    if not isinstance(value, dict):
        raise ValueError(f"{field} must be a structured evidence object")
    if set(value) != {"kind", "reference", "sha256"}:
        raise ValueError(f"{field} must contain exactly kind, reference and sha256")
    if value.get("kind") not in _EVIDENCE_KINDS:
        raise ValueError(f"{field}.kind is not supported")
    reference = value.get("reference")
    if not isinstance(reference, str) or not reference.strip() or len(reference) > 2048:
        raise ValueError(f"{field}.reference must be a bounded non-empty string")
    require_sha256(value.get("sha256"), f"{field}.sha256")
    return value


def validate_approval_record(value: Any, *, artifact_sha256: str) -> dict:
    if not isinstance(value, dict):
        raise ValueError("approvalRecord must be a structured object")
    required = {
        "repository", "subjectCommitSha", "subjectTreeSha", "artifactSha256",
        "workflowRunId", "approvedBy", "approvedAt",
    }
    if set(value) != required:
        raise ValueError("approvalRecord must contain exactly the required binding fields")
    repository = value.get("repository")
    if not isinstance(repository, str) or repository.count("/") != 1 or any(not p.strip() for p in repository.split("/")):
        raise ValueError("approvalRecord.repository must be owner/repository")
    require_sha40(value.get("subjectCommitSha"), "approvalRecord.subjectCommitSha")
    require_sha40(value.get("subjectTreeSha"), "approvalRecord.subjectTreeSha")
    record_artifact = require_sha256(value.get("artifactSha256"), "approvalRecord.artifactSha256")
    if record_artifact != require_sha256(artifact_sha256, "artifactSha256"):
        raise ValueError("approvalRecord.artifactSha256 does not match the approved artifact")
    run_id = value.get("workflowRunId")
    if not isinstance(run_id, int) or isinstance(run_id, bool) or run_id <= 0:
        raise ValueError("approvalRecord.workflowRunId must be a positive integer")
    approved_by = value.get("approvedBy")
    if not isinstance(approved_by, str) or not approved_by.strip() or len(approved_by) > 256:
        raise ValueError("approvalRecord.approvedBy must be a bounded non-empty string")
    approved_at = value.get("approvedAt")
    if not isinstance(approved_at, str) or len(approved_at) > 64:
        raise ValueError("approvalRecord.approvedAt must be an ISO-8601 timestamp")
    try:
        parsed = datetime.fromisoformat(approved_at.replace("Z", "+00:00"))
    except ValueError as exc:
        raise ValueError("approvalRecord.approvedAt must be an ISO-8601 timestamp") from exc
    if parsed.tzinfo is None:
        raise ValueError("approvalRecord.approvedAt must include a timezone")
    return value
