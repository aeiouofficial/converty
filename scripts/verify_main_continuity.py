from __future__ import annotations

import os
import subprocess
import sys
from collections.abc import Callable
from typing import NamedTuple


class ContinuityResult(NamedTuple):
    ok: bool
    message: str


def verify_main_continuity(
    *,
    event_name: str,
    ref_name: str,
    head_sha: str,
    is_ancestor: Callable[[str, str], bool],
) -> ContinuityResult:
    """Decide whether a workflow run can represent repository authority."""
    if event_name != "push":
        return ContinuityResult(
            True,
            "Pull-request/review event; push authority gate is not applicable.",
        )

    if ref_name == "main":
        return ContinuityResult(True, "Current push is on main repository authority.")

    if is_ancestor(head_sha, "origin/main"):
        return ContinuityResult(True, "Branch HEAD is already contained in main.")

    return ContinuityResult(
        False,
        "Development-only branch is ahead of main; promote the durable work to main "
        "and qualify that exact main SHA before any completion claim.",
    )


def git_is_ancestor(head_sha: str, main_ref: str) -> bool:
    completed = subprocess.run(
        ["git", "merge-base", "--is-ancestor", head_sha, main_ref],
        check=False,
        stdout=subprocess.DEVNULL,
        stderr=subprocess.DEVNULL,
    )
    return completed.returncode == 0


def main() -> int:
    event_name = os.environ.get("CONTINUITY_EVENT_NAME", "")
    ref_name = os.environ.get("CONTINUITY_REF_NAME", "")
    head_sha = os.environ.get("CONTINUITY_HEAD_SHA", "")

    missing = [
        name
        for name, value in (
            ("CONTINUITY_EVENT_NAME", event_name),
            ("CONTINUITY_REF_NAME", ref_name),
            ("CONTINUITY_HEAD_SHA", head_sha),
        )
        if not value
    ]
    if missing:
        print(f"main-authority-continuity: FAIL: missing environment: {', '.join(missing)}")
        return 1

    result = verify_main_continuity(
        event_name=event_name,
        ref_name=ref_name,
        head_sha=head_sha,
        is_ancestor=git_is_ancestor,
    )
    state = "PASS" if result.ok else "FAIL"
    print(f"main-authority-continuity: {state}: {result.message}")
    return 0 if result.ok else 1


if __name__ == "__main__":
    sys.exit(main())
