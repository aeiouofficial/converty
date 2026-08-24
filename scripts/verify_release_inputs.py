#!/usr/bin/env python3
from __future__ import annotations

import json
import subprocess
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
PRIVATE_SUFFIXES = {".pfx", ".p12", ".key", ".pem"}
PRIVATE_NAMES = {".env"}


def main() -> int:
    failures: list[str] = []
    projects = sorted(ROOT.rglob("*.csproj"))
    for project in projects:
        if any(part in {"bin", "obj", "artifacts"} for part in project.parts):
            continue
        lock = project.parent / "packages.lock.json"
        if not lock.is_file():
            failures.append(f"packages.lock.json missing for {project.relative_to(ROOT).as_posix()}")

    ci_result = subprocess.run(
        [sys.executable, "scripts/verify_ci_actions.py"],
        cwd=ROOT,
        text=True,
        capture_output=True,
        check=False,
    )
    if ci_result.returncode != 0:
        failures.append("CI action provenance verification failed: " + (ci_result.stderr or ci_result.stdout).strip())

    policy_path = ROOT / "machine-readable" / "release_policy.json"
    try:
        policy = json.loads(policy_path.read_text(encoding="utf-8"))
        if policy.get("hashAlgorithm") != "SHA-256":
            failures.append("release policy must require SHA-256")
        if policy.get("signing", {}).get("privateKeysInWorkspace") is not False:
            failures.append("release policy must forbid private keys in workspace")
        if policy.get("ciProvenance", {}).get("externalActionsPinnedToFullSha") is not True:
            failures.append("release policy must require immutable external GitHub Action pins")
        audit = policy.get("dependencyAudit", {})
        if audit.get("enabled") is not True or audit.get("mode") != "all" or audit.get("level") != "low":
            failures.append("release policy must require NuGet audit all/low")
    except (OSError, json.JSONDecodeError) as exc:
        failures.append(f"release policy unreadable: {exc}")

    for path in ROOT.rglob("*"):
        if not path.is_file():
            continue
        if any(part in {".git", "bin", "obj", "artifacts", ".pytest_cache", "__pycache__"} for part in path.parts):
            continue
        if path.name in PRIVATE_NAMES or path.suffix.lower() in PRIVATE_SUFFIXES:
            failures.append(f"private-key/secret-like workspace file present: {path.relative_to(ROOT).as_posix()}")

    if failures:
        print("release preflight: FAIL", file=sys.stderr)
        for failure in failures:
            print(f"- {failure}", file=sys.stderr)
        return 2

    print("release preflight: PASS")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
