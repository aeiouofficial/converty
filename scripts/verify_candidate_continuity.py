#!/usr/bin/env python3
from __future__ import annotations

import json
import os
import subprocess
import sys
from collections.abc import Callable
from pathlib import Path
from typing import NamedTuple

ROOT = Path(__file__).resolve().parents[1]
EVIDENCE = ROOT / "machine-readable" / "build_evidence.json"


class ContinuityResult(NamedTuple):
    ok: bool
    message: str


def verify_candidate_continuity(
    *,
    head_sha: str,
    expected_main_sha: str,
    actual_main_sha: str,
    is_ancestor: Callable[[str, str], bool],
) -> ContinuityResult:
    if not head_sha or not expected_main_sha or not actual_main_sha:
        return ContinuityResult(False, "candidate continuity requires head and main SHAs")
    if actual_main_sha != expected_main_sha:
        return ContinuityResult(False, "live main moved away from the frozen release-authority base")
    if not is_ancestor(expected_main_sha, head_sha):
        return ContinuityResult(False, "candidate does not descend from the exact frozen main authority")
    return ContinuityResult(True, "candidate descends from the exact unchanged frozen main authority")


def git_is_ancestor(base_sha: str, head_sha: str) -> bool:
    return subprocess.run(
        ["git", "merge-base", "--is-ancestor", base_sha, head_sha],
        check=False,
        stdout=subprocess.DEVNULL,
        stderr=subprocess.DEVNULL,
    ).returncode == 0


def main() -> int:
    try:
        expected = json.loads(EVIDENCE.read_text(encoding="utf-8"))["releaseAuthority"]["mainSha"]
    except (OSError, json.JSONDecodeError, KeyError) as exc:
        print(f"candidate-base-continuity: FAIL: unreadable frozen authority: {exc}")
        return 1
    head = os.environ.get("CANDIDATE_HEAD_SHA", "").strip()
    actual = subprocess.run(["git", "rev-parse", "origin/main"], check=False, capture_output=True, text=True)
    if actual.returncode != 0:
        print("candidate-base-continuity: FAIL: origin/main is unavailable")
        return 1
    result = verify_candidate_continuity(
        head_sha=head,
        expected_main_sha=expected,
        actual_main_sha=actual.stdout.strip(),
        is_ancestor=git_is_ancestor,
    )
    print(f"candidate-base-continuity: {'PASS' if result.ok else 'FAIL'}: {result.message}")
    return 0 if result.ok else 1


if __name__ == "__main__":
    sys.exit(main())
