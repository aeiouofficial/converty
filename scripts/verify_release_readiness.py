#!/usr/bin/env python3
from __future__ import annotations

import argparse
import json
import os
import sys
import urllib.error
import urllib.request
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
BUILD_EVIDENCE = ROOT / "machine-readable" / "build_evidence.json"
VERSION = ROOT / "VERSION"
API_ROOT = "https://api.github.com"


def load_json(path: Path) -> dict:
    return json.loads(path.read_text(encoding="utf-8"))


def github_json(path: str, token: str) -> object:
    request = urllib.request.Request(
        API_ROOT + path,
        headers={
            "Accept": "application/vnd.github+json",
            "Authorization": f"Bearer {token}",
            "X-GitHub-Api-Version": "2022-11-28",
            "User-Agent": "converty-release-readiness",
        },
    )
    with urllib.request.urlopen(request, timeout=20) as response:
        return json.load(response)


def collect_status(check_live_github: bool) -> dict:
    evidence = load_json(BUILD_EVIDENCE)
    version = VERSION.read_text(encoding="utf-8").strip()
    release_blockers = evidence.get("releaseBlockers", {})
    open_blockers = sorted(
        key for key, value in release_blockers.items()
        if str(value).upper() != "PASS"
    )

    status = {
        "schemaVersion": 1,
        "workspaceVersion": version,
        "shipReady": not open_blockers,
        "openBlockers": open_blockers,
        "liveGithubChecked": False,
        "rulesetCount": None,
        "mainBranchProtected": None,
        "liveGithubError": None,
    }

    if not check_live_github:
        return status

    repository = os.environ.get("GITHUB_REPOSITORY", "").strip()
    token = os.environ.get("GITHUB_TOKEN", "").strip()
    status["liveGithubChecked"] = True

    if not repository or not token:
        status["liveGithubError"] = "GITHUB_REPOSITORY and GITHUB_TOKEN are required"
        status["openBlockers"] = sorted(set(status["openBlockers"]) | {"liveGithubEvidenceUnavailable"})
        status["shipReady"] = False
        return status

    try:
        rulesets = github_json(f"/repos/{repository}/rulesets", token)
        branch = github_json(f"/repos/{repository}/branches/main", token)
        if not isinstance(rulesets, list) or not isinstance(branch, dict):
            raise ValueError("unexpected GitHub governance response")
        status["rulesetCount"] = len(rulesets)
        status["mainBranchProtected"] = bool(branch.get("protected"))
        live_open: set[str] = set()
        if not rulesets:
            live_open.add("liveRepositoryRuleset")
        if not status["mainBranchProtected"]:
            live_open.add("mainBranchProtection")
        status["openBlockers"] = sorted(set(status["openBlockers"]) | live_open)
        status["shipReady"] = not status["openBlockers"]
    except (urllib.error.HTTPError, urllib.error.URLError, TimeoutError, ValueError, json.JSONDecodeError) as exc:
        status["liveGithubError"] = str(exc)
        status["openBlockers"] = sorted(set(status["openBlockers"]) | {"liveGithubEvidenceUnavailable"})
        status["shipReady"] = False

    return status


def main() -> int:
    parser = argparse.ArgumentParser(description="Report or enforce Converty release readiness.")
    parser.add_argument("--json", action="store_true", dest="as_json")
    parser.add_argument("--require-ready", action="store_true")
    parser.add_argument("--live-github", action="store_true")
    args = parser.parse_args()

    status = collect_status(args.live_github)

    if args.as_json:
        print(json.dumps(status, sort_keys=True))
    else:
        print(f"release readiness: {'PASS' if status['shipReady'] else 'BLOCKED'}")
        print(f"workspaceVersion={status['workspaceVersion']}")
        print(f"liveGithubChecked={status['liveGithubChecked']}")
        print(f"rulesetCount={status['rulesetCount']}")
        print(f"mainBranchProtected={status['mainBranchProtected']}")
        for blocker in status["openBlockers"]:
            print(f"BLOCKER {blocker}")
        if status["liveGithubError"]:
            print(f"liveGithubError={status['liveGithubError']}")

    if args.require_ready and not status["shipReady"]:
        return 2
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
