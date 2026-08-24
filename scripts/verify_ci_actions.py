#!/usr/bin/env python3
from __future__ import annotations

import json
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
MANIFEST = ROOT / "machine-readable/ci_action_pins.json"
WORKFLOW_DIR = ROOT / ".github/workflows"
USES_RE = re.compile(r"^\s*-?\s*uses:\s*([^\s@]+)@([^\s#]+)(?:\s+#\s*(\S+))?\s*$")
SHA_RE = re.compile(r"^[0-9a-f]{40}$")


def fail(messages: list[str]) -> int:
    print("CI action pins: FAIL", file=sys.stderr)
    for message in messages:
        print(f"- {message}", file=sys.stderr)
    return 1


def main() -> int:
    try:
        manifest = json.loads(MANIFEST.read_text(encoding="utf-8"))
    except (OSError, json.JSONDecodeError) as exc:
        return fail([f"cannot read pin manifest: {exc}"])

    version = (ROOT / "VERSION").read_text(encoding="utf-8").strip()
    if manifest.get("schemaVersion") != 1:
        return fail(["pin manifest schemaVersion must be 1"])
    if manifest.get("workspaceVersion") != version:
        return fail(["pin manifest workspaceVersion must match VERSION"])

    raw_actions = manifest.get("actions")
    if not isinstance(raw_actions, dict) or not raw_actions:
        return fail(["pin manifest actions must be a non-empty object"])

    pins: dict[str, tuple[str, str]] = {}
    errors: list[str] = []
    for action, entry in sorted(raw_actions.items()):
        if not isinstance(entry, dict):
            errors.append(f"manifest entry {action!r} must be an object")
            continue
        sha = entry.get("sha")
        release = entry.get("version")
        if not isinstance(sha, str) or not SHA_RE.fullmatch(sha):
            errors.append(f"manifest SHA for {action} must be 40 lowercase hex characters")
            continue
        if not isinstance(release, str) or not release.startswith("v"):
            errors.append(f"manifest version for {action} must be a v-prefixed release")
            continue
        pins[action] = (sha, release)

    occurrences = 0
    seen: set[str] = set()
    workflow_files = sorted([*WORKFLOW_DIR.glob("*.yml"), *WORKFLOW_DIR.glob("*.yaml")])
    if not workflow_files:
        errors.append("no workflow files found")

    for workflow in workflow_files:
        for line_number, line in enumerate(workflow.read_text(encoding="utf-8").splitlines(), 1):
            if "uses:" not in line:
                continue
            match = USES_RE.match(line)
            if not match:
                errors.append(f"{workflow.relative_to(ROOT)}:{line_number}: unsupported or malformed uses reference")
                continue
            action, reference, comment_version = match.groups()
            if action.startswith("./") or action.startswith("docker://"):
                continue
            occurrences += 1
            expected = pins.get(action)
            if expected is None:
                errors.append(f"{workflow.relative_to(ROOT)}:{line_number}: external action {action!r} is not approved")
                continue
            expected_sha, expected_version = expected
            seen.add(action)
            if reference != expected_sha:
                errors.append(f"{workflow.relative_to(ROOT)}:{line_number}: {action} must use {expected_sha}, got {reference}")
            if comment_version != expected_version:
                errors.append(f"{workflow.relative_to(ROOT)}:{line_number}: {action} comment must be {expected_version}, got {comment_version!r}")

    unused = sorted(set(pins) - seen)
    if unused:
        errors.append("approved action pins are unused: " + ", ".join(unused))
    if errors:
        return fail(errors)

    print(f"CI action pins: PASS ({occurrences} external uses, {len(seen)} approved actions)")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
