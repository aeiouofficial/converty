#!/usr/bin/env python3
from __future__ import annotations

import argparse
import json
import os
import subprocess
import sys
import urllib.error
import urllib.parse
import urllib.request
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
BUILD_EVIDENCE = ROOT / "machine-readable" / "build_evidence.json"
VERSION = ROOT / "VERSION"
API_ROOT = "https://api.github.com"

REQUIRED_CHECKS = {
    "candidate-base-continuity",
    "supply-chain-static",
    "managed",
    "candidate-release-readiness",
}
QUALIFICATION_CHECKS = {"candidate-base-continuity", "supply-chain-static", "managed"}
ALLOWED_POST_APPROVAL_FILES = {
    "eng/ffmpeg-production.json",
    "eng/windows-production-package.json",
    "eng/release-acceptance.json",
    "machine-readable/build_evidence.json",
    "machine-readable/handover_state.json",
    "machine-readable/package_manifest.json",
    "machine-readable/release_sbom.spdx.json",
    "machine-readable/source_sbom.spdx.json",
    "SHA256SUMS.txt",
    "CHANGELOG.md",
    "README.md",
    "docs/TASK_BACKLOG.md",
    "docs/development/IMPLEMENTATION_STATUS.md",
    "docs/HANDOVER_NEXT_AGENT.md",
    "docs/HANDOVER_PROMPT.txt",
}


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


def _main_applies(ruleset: dict) -> bool:
    if ruleset.get("enforcement") != "active" or ruleset.get("target") != "branch":
        return False
    ref = ruleset.get("conditions", {}).get("ref_name", {})
    include = set(ref.get("include") or [])
    exclude = set(ref.get("exclude") or [])
    aliases = {"refs/heads/main", "~DEFAULT_BRANCH"}
    return bool(include & aliases) and not bool(exclude & aliases)


def evaluate_main_governance(rulesets: list[dict]) -> list[str]:
    candidates = [item for item in rulesets if isinstance(item, dict) and _main_applies(item)]
    if not candidates:
        return ["liveRepositoryRuleset"]

    for ruleset in candidates:
        if ruleset.get("bypass_actors"):
            continue
        rules = [rule for rule in ruleset.get("rules", []) if isinstance(rule, dict)]
        types = {rule.get("type") for rule in rules}
        if not {"deletion", "non_fast_forward", "required_linear_history", "required_signatures"} <= types:
            continue
        status_rule = next((r for r in rules if r.get("type") == "required_status_checks"), None)
        if status_rule is None:
            continue
        checks = {
            item.get("context")
            for item in status_rule.get("parameters", {}).get("required_status_checks", [])
            if isinstance(item, dict)
        }
        if REQUIRED_CHECKS <= checks:
            return []
    return ["liveRepositoryRuleset"]


def _run_verifier(script: str) -> tuple[dict | None, str | None]:
    result = subprocess.run(
        [sys.executable, script, "--json"],
        cwd=ROOT,
        text=True,
        capture_output=True,
        check=False,
    )
    if result.returncode != 0:
        return None, (result.stderr or result.stdout).strip()
    try:
        return json.loads(result.stdout), None
    except json.JSONDecodeError as exc:
        return None, str(exc)


def _validate_binding_live(record: dict, *, repository: str, candidate_sha: str, token: str) -> list[str]:
    if not isinstance(record, dict) or record.get("repository") != repository:
        return ["releaseEvidenceCandidateBinding"]

    subject = str(record.get("subjectCommitSha") or "")
    subject_tree = str(record.get("subjectTreeSha") or "")
    blockers: list[str] = []

    try:
        commit = github_json(f"/repos/{repository}/commits/{subject}", token)
        if not isinstance(commit, dict) or commit.get("commit", {}).get("tree", {}).get("sha") != subject_tree:
            return ["releaseEvidenceCandidateBinding"]

        compare = github_json(
            f"/repos/{repository}/compare/"
            f"{urllib.parse.quote(subject, safe='')}...{urllib.parse.quote(candidate_sha, safe='')}",
            token,
        )
        if (
            not isinstance(compare, dict)
            or compare.get("behind_by", 1) != 0
            or compare.get("status") not in {"ahead", "identical"}
        ):
            return ["releaseEvidenceCandidateBinding"]

        changed = {
            item.get("filename")
            for item in compare.get("files", [])
            if isinstance(item, dict)
        }
        if any(not path or path not in ALLOWED_POST_APPROVAL_FILES for path in changed):
            blockers.append("releaseEvidencePostApprovalDrift")

        run_id = record.get("workflowRunId")
        run = github_json(f"/repos/{repository}/actions/runs/{run_id}", token)
        if not isinstance(run, dict) or run.get("head_sha") != subject:
            blockers.append("releaseEvidenceQualificationRun")
            return blockers

        jobs = github_json(f"/repos/{repository}/actions/runs/{run_id}/jobs?per_page=100", token)
        if not isinstance(jobs, dict):
            blockers.append("releaseEvidenceQualificationRun")
            return blockers
        by_name = {
            job.get("name"): job
            for job in jobs.get("jobs", [])
            if isinstance(job, dict)
        }
        if any(by_name.get(name, {}).get("conclusion") != "success" for name in QUALIFICATION_CHECKS):
            blockers.append("releaseEvidenceQualificationRun")
    except (urllib.error.HTTPError, urllib.error.URLError, TimeoutError, ValueError, json.JSONDecodeError):
        blockers.append("releaseEvidenceCandidateBinding")

    return blockers


def collect_status(check_live_github: bool) -> dict:
    evidence = load_json(BUILD_EVIDENCE)
    version = VERSION.read_text(encoding="utf-8").strip()
    release_blockers = evidence.get("releaseBlockers", {})
    open_blockers = {
        key
        for key, value in release_blockers.items()
        if str(value).upper() != "PASS"
    }

    engine, engine_error = _run_verifier("scripts/verify_production_engine.py")
    package, package_error = _run_verifier("scripts/verify_production_package.py")
    acceptance, acceptance_error = _run_verifier("scripts/verify_release_acceptance.py")

    engine_approved = bool(engine and engine.get("approved"))
    package_approved = bool(package and package.get("approved"))
    headed_approved = bool(acceptance and acceptance.get("headedWindows11Approved"))
    final_approved = bool(acceptance and acceptance.get("finalAcceptanceApproved"))

    if not engine_approved:
        open_blockers.add("productionFfmpegRedistributionApproval")
    if not package_approved:
        open_blockers.update({"productionSignedPackageB2Requalification", "signedProductionMsixLifecycle"})
    if not headed_approved:
        open_blockers.add("headedWindows11ExplorerAcceptance")
    if not final_approved:
        open_blockers.add("finalSecurityFuzzChaosReleaseEndUserAcceptance")

    status = {
        "schemaVersion": 1,
        "workspaceVersion": version,
        "shipReady": False,
        "openBlockers": sorted(open_blockers),
        "liveGithubChecked": False,
        "rulesetCount": None,
        "mainBranchProtected": None,
        "liveGithubError": None,
        "productionEngineApproved": engine_approved,
        "productionEngineError": engine_error,
        "productionPackageApproved": package_approved,
        "productionPackageError": package_error,
        "headedWindows11Approved": headed_approved,
        "finalAcceptanceApproved": final_approved,
        "releaseAcceptanceError": acceptance_error,
    }

    if not check_live_github:
        open_blockers.add("liveGithubEvidenceUnavailable")
        status["openBlockers"] = sorted(open_blockers)
        return status

    repository = os.environ.get("GITHUB_REPOSITORY", "").strip()
    token = os.environ.get("GITHUB_TOKEN", "").strip()
    candidate_sha = os.environ.get("GITHUB_SHA", "").strip()
    status["liveGithubChecked"] = True

    if not repository or not token or not candidate_sha:
        status["liveGithubError"] = "GITHUB_REPOSITORY, GITHUB_TOKEN and GITHUB_SHA are required"
        open_blockers.add("liveGithubEvidenceUnavailable")
        status["openBlockers"] = sorted(open_blockers)
        return status

    try:
        summaries = github_json(f"/repos/{repository}/rulesets", token)
        branch = github_json(f"/repos/{repository}/branches/main", token)
        if not isinstance(summaries, list) or not isinstance(branch, dict):
            raise ValueError("unexpected GitHub governance response")

        details: list[dict] = []
        for item in summaries:
            if isinstance(item, dict) and isinstance(item.get("id"), int):
                detail = github_json(f"/repos/{repository}/rulesets/{item['id']}", token)
                if isinstance(detail, dict):
                    details.append(detail)

        status["rulesetCount"] = len(details)
        status["mainBranchProtected"] = bool(branch.get("protected"))
        open_blockers.update(evaluate_main_governance(details))
        if not status["mainBranchProtected"]:
            open_blockers.add("mainBranchProtection")

        records = []
        for payload in (engine, package, acceptance):
            if payload and payload.get("approved") and payload.get("approvalRecord"):
                records.append(payload["approvalRecord"])
        for record in records:
            open_blockers.update(
                _validate_binding_live(
                    record,
                    repository=repository,
                    candidate_sha=candidate_sha,
                    token=token,
                )
            )

        if package_approved and acceptance and acceptance.get("approved"):
            if package.get("artifactSha256") != acceptance.get("artifactSha256"):
                open_blockers.add("releaseAcceptanceArtifactMismatch")
    except (urllib.error.HTTPError, urllib.error.URLError, TimeoutError, ValueError, json.JSONDecodeError) as exc:
        status["liveGithubError"] = str(exc)
        open_blockers.add("liveGithubEvidenceUnavailable")

    status["openBlockers"] = sorted(open_blockers)
    status["shipReady"] = not open_blockers
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
    return 2 if args.require_ready and not status["shipReady"] else 0


if __name__ == "__main__":
    raise SystemExit(main())
