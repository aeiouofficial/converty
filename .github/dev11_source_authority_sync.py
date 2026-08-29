from __future__ import annotations

import json
import re
import subprocess
from pathlib import Path

VERSION = "0.1.0-dev.11"
DATE = "2026-08-29"
MAIN = "13ed46bcb5cb02f33965dace4adc5a3fb25e87fd"
DEV10 = "358d5d1faf293f217402af6ad1ae7e53f33f8183"
B2_HEAD = "1d4c22733c43f4eee4a4a4cd6751608e60359561"
PRE_VERSION = "0d37afdba33abcd9ca31f3e59d0d6dc8a1bb7e5d"
TREE = "7560366cb059c1ff90c539f497903e84df1b2141"
BRANCH = "dev/0.1.0-dev.11-b2-auth"
PFN = "Converty.Dev_yr4ybytcyx7nj"


def load(path: str) -> dict:
    return json.loads(Path(path).read_text(encoding="utf-8"))


def save(path: str, data: dict) -> None:
    Path(path).write_text(json.dumps(data, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")


def prepend_section(path: str, heading: str, body: str) -> None:
    p = Path(path)
    text = p.read_text(encoding="utf-8")
    if heading in text:
        return
    first, rest = text.split("\n", 1)
    p.write_text(first + "\n\n" + heading + "\n" + body.rstrip() + "\n\n" + rest.lstrip("\n"), encoding="utf-8")


subprocess.run(["git", "merge-base", "--is-ancestor", PRE_VERSION, "HEAD"], check=True)
count = int(subprocess.check_output(["git", "rev-list", "--count", f"{PRE_VERSION}..HEAD"], text=True).strip())
if count > 2:
    raise SystemExit(f"unexpected commits after qualified parent: {count}")

Path("VERSION").write_text(VERSION + "\n", encoding="utf-8")

release = load("machine-readable/release_policy.json")
release["workspaceVersion"] = VERSION
save("machine-readable/release_policy.json", release)

pins = load("machine-readable/ci_action_pins.json")
pins["workspaceVersion"] = VERSION
pins["reviewedAt"] = DATE
save("machine-readable/ci_action_pins.json", pins)

toolchain = load("eng/toolchain.json")
toolchain["workspaceVersion"] = VERSION
toolchain["notes"] = [
    "Dev.11 closes development B2 connected-server identity/authentication without routing normal conversion through Host.",
    "Pre-version exact-tree run 33260905467 at 0d37afdba33abcd9ca31f3e59d0d6dc8a1bb7e5d passed 192/192 managed, 66/66 static, 5/5 vectors, zero-vulnerability audit, Release 0 warnings/errors, native/package/COM/product smokes, deterministic workspace and verified delivery.",
    f"Registered package COM shell launch gives exact Bridge PFN {PFN}; the exact Host child preserves it and BridgeClient authenticates Host PID/path/PFN before the first application frame.",
    "Host staging is part of the development package; Bridge/Host remain media/process neutral and FFmpeg execution remains worker/provider-only.",
    "Headed Win11, production signed-package B2 requalification, production FFmpeg redistribution, signed MSIX/clean-VM lifecycle and final release audit remain open.",
]
save("eng/toolchain.json", toolchain)

state = {
    "schemaVersion": 1,
    "project": "Converty",
    "workspaceVersion": VERSION,
    "baselineDate": DATE,
    "implementationStatus": "DEV11_B2_DEVELOPMENT_AUTH_QUALIFIED_SOURCE_AUTHORITY_SYNCED_GENERATED_AUTHORITY_REGEN_PENDING",
    "batchesTouched": ["B0", "B1", "B2", "B3", "B4", "B5"],
    "nextWorkspaceVersion": "0.1.0-dev.12",
    "nextBatch": "DEV11_GENERATED_AUTHORITY_REGEN_AND_EXACT_HEAD_FREEZE",
    "targetOs": "Windows 11 x64",
    "toolchain": {"dotnetSdk": "10.0.400", "targetFramework": "net10.0", "csharp": "14.0", "dotnetTestRunner": "Microsoft.Testing.Platform", "xunit": "xunit.v3.mtp-v2 4.0.0", "native": "C++20 / CMake 3.28+ / MSVC"},
    "frozenPriorAuthority": {"mainSha": MAIN, "dev10HeadSha": DEV10, "dev10BehaviorHeadSha": "f221563c790057344a94b4e60c309d4512a77c38", "dev10QualificationRunId": 33028554361},
    "dev11B2Evidence": {
        "sourceAuthorityHeadSha": B2_HEAD,
        "hostStagingRedRunId": 33202365348,
        "hostStagingRedJobId": 98954716457,
        "parentChildIdentityRunId": 33211928010,
        "parentChildIdentityJobId": 98986920905,
        "explorerBridgeIdentityRunId": 33218030168,
        "explorerBridgeIdentityJobId": 99005949641,
        "packagedBridgeHostAuthRunId": 33218498644,
        "packagedBridgeHostAuthJobId": 99007347897,
        "developmentPackageFamilyName": PFN,
        "packagedBridgeHostJobId": "5bd48925-8c88-48d2-bbd7-a62c2ba03e3e",
        "status": "PASS_DEVELOPMENT_FULL_PACKAGE_MODEL",
    },
    "preVersionAuthorityQualification": {
        "headSha": PRE_VERSION,
        "treeSha": TREE,
        "workflowRunId": 33260905467,
        "managedJobId": 99122561963,
        "staticJobId": 99122562067,
        "managedTests": "192/192 PASS; 0 skipped",
        "staticTests": "66/66 PASS",
        "contractVectors": "5/5 PASS",
        "dependencyAudit": "PASS; 18 projects; 18 frameworks; 0 vulnerable-result packages",
        "releaseBuild": "PASS; 0 warnings; 0 errors",
        "generatedAuthorityDiff": "CLEAN_AT_PRE_VERSION_TREE",
        "workspaceZip": {"name": "Converty_0.1.0-dev.10_full_workspace.zip", "sha256": "de6b43a9343591fc451b488b65e73049f60759a9be0072be11495c24604946ab", "bytes": 382689, "files": 329},
        "verifiedDeliveryArtifactId": 9717244755,
        "verifiedDeliveryArtifactDigest": "sha256:50d6cfc6443356bc70210f6b465ccddea47733b1a3e0a31d3fe19c0e3ea80c09",
    },
    "implementedSource": [
        "Native C++20 IExplorerCommand and exact app-local Bridge handoff",
        "Strict disposable EngineWorker/provider FFmpeg conversion with private staging and transactional numbered publication",
        "Development package stages exact Converty.Host.exe beside Bridge",
        "Connected-server probe verifies actual pipe server PID, exact Host image, exact PFN and stable PID",
        "BridgeClient authenticates the connected Host before writing the first application request frame",
        "Actual packaged COM shell→Bridge CreateProcessW and packaged parent→Host Process.Start preserve package identity",
    ],
    "shippingOpen": [
        "Headed Windows 11 modern Explorer acceptance and exact-build screenshots",
        "Explorer crash/hang/failure headed matrix",
        "Production signed-package B2 identity/authentication requalification",
        "Status/cancel and replay/disconnect/reconnect/session acceptance",
        "Production FFmpeg redistribution/license/notices/signature/hash approval",
        "Signed production MSIX and clean Windows 11 VM lifecycle acceptance",
        "Final security/fuzz/chaos/release audit and end-user acceptance",
    ],
    "nextActions": [
        "Regenerate dev.11 source/release SBOM, package manifest and SHA256SUMS from this source-authority tree",
        "Commit exact runner-generated authority bytes only",
        "Require exact-head zero-diff authority plus all managed/static/product/package gates and verified dev.11 delivery",
        "Do not merge/promote merely because CI is green",
    ],
    "recursiveHandoverRequired": True,
    "recursiveHandoverRule": "Every agent must end its tranche with a new full-context copy-paste handover containing exact repository/branch/main/current SHA/tree, immutable qualification evidence, all run/job/artifact IDs, exact changes and reasons, RED/GREEN history, executed test/build/security outcomes, workspace hashes, unresolved blockers, explicitly unverified claims, ONE precise next task, all non-negotiables, and this same recursive rule until Converty is shipped.",
    "authorityFiles": ["docs/HANDOVER_NEXT_AGENT.md", "docs/HANDOVER_PROMPT.txt", "docs/Converty_Master_Build_Plan.md", "docs/adr/ADR-013-dev9-functional-product-spike.md", "docs/SECURITY_THREAT_MODEL.md", "docs/TEST_AND_RELEASE_GATES.md", "docs/TASK_BACKLOG.md", "docs/development/IMPLEMENTATION_STATUS.md", "docs/security/B2_SERVER_AUTH_GATE.md", "machine-readable/build_evidence.json", "machine-readable/release_policy.json"],
}
save("machine-readable/handover_state.json", state)

build = {
    "schemaVersion": 1,
    "workspaceVersion": VERSION,
    "evidenceDate": DATE,
    "frozenDev10": {"headSha": DEV10, "behaviorHeadSha": "f221563c790057344a94b4e60c309d4512a77c38", "qualificationRunId": 33028554361, "managedJobId": 98375493893, "staticJobId": 98375494099, "postAuditQualificationRunId": 33044395097},
    "dev11B2Qualification": {
        "sourceAuthorityHeadSha": B2_HEAD,
        "hostStagingRedRunId": 33202365348,
        "hostStagingRedJobId": 98954716457,
        "parentChildIdentityRunId": 33211928010,
        "parentChildIdentityJobId": 98986920905,
        "explorerBridgeIdentityRunId": 33218030168,
        "explorerBridgeIdentityJobId": 99005949641,
        "packagedBridgeHostAuthRunId": 33218498644,
        "packagedBridgeHostAuthJobId": 99007347897,
        "developmentPackageFamilyName": PFN,
        "result": "PASS_DEVELOPMENT_FULL_PACKAGE_MODEL",
        "normalProductPathRoutedThroughHost": False,
    },
    "preVersionExactHeadQualification": {
        "headSha": PRE_VERSION,
        "treeSha": TREE,
        "workflowRunId": 33260905467,
        "managedJobId": 99122561963,
        "staticJobId": 99122562067,
        "lockedRestore": "18/18 PASS",
        "dependencyAudit": "PASS; 18 projects; 18 frameworks; 0 vulnerable-result packages",
        "releaseBuild": "PASS; 0 warnings; 0 errors",
        "nativeExplorer": "PASS",
        "developmentPackageAndMakeAppx": "PASS; Host staged",
        "directAndPackagedComInvoke": "PASS",
        "productBridgeStrictWorkerFfmpeg": "PASS; mp3 / 320000 bit/s; Unicode/metacharacter path; source and existing destination preserved; numbered publication",
        "managedTests": {"total": 192, "succeeded": 192, "failed": 0, "skipped": 0},
        "staticTests": "66/66 PASS",
        "contractVectors": "5/5 PASS",
        "generatedAuthorityDiff": "CLEAN",
        "workspace": {"name": "Converty_0.1.0-dev.10_full_workspace.zip", "sha256": "de6b43a9343591fc451b488b65e73049f60759a9be0072be11495c24604946ab", "bytes": 382689, "files": 329, "packageManifestEntries": 327, "shaManifestEntries": 328, "crc": "PASS", "deterministicDoubleBuild": "PASS", "exclusionPolicy": "PASS"},
        "generatedAuthorityArtifact": {"id": 9717218608, "digest": "sha256:27dca1681ea4837c8523efe8b88a49f8ca783008abfdf29d77596b5c7a5159f4"},
        "verifiedDeliveryArtifact": {"id": 9717244755, "digest": "sha256:50d6cfc6443356bc70210f6b465ccddea47733b1a3e0a31d3fe19c0e3ea80c09"},
    },
    "knownHistoricalIntermittent": {"test": "StrictWorkerIsTerminatedWhenStagingGrowthExceedsOutputBudget", "historicalRunId": 33044741340, "dev11PreVersionInitialManagedJobId": 99007945036, "observed": "191/192 once; unchanged-head reruns later passed 192/192", "status": "OPEN_FLAKE_HISTORY_NOT_ERASED"},
    "currentAuthorityState": "DEV11_SOURCE_VERSION_AUTHORITY_SYNCHRONIZED; GENERATED_SBOM_PACKAGE_HASH_AUTHORITY_REGENERATION_REQUIRED",
    "headedWindows11Acceptance": "OPEN",
    "productionSignedPackageB2Requalification": "OPEN",
    "productionFfmpegRedistributionApproval": "OPEN",
}
save("machine-readable/build_evidence.json", build)

changelog = Path("CHANGELOG.md").read_text(encoding="utf-8")
if "## 0.1.0-dev.11 — 2026-08-29" not in changelog:
    section = """## 0.1.0-dev.11 — 2026-08-29
- Closed development B2 connected-server identity/authentication without rerouting normal conversion through Host.
- Development package staging now includes exact sibling `Converty.Host.exe`.
- Proved real registered package COM shell `CreateProcessW` gives exact Bridge PFN `Converty.Dev_yr4ybytcyx7nj` (run `33218030168`, job `99005949641`).
- Proved package-identified parent→exact Host `Process.Start` preserves PFN (run `33211928010`, job `98986920905`).
- Proved packaged Bridge authenticates connected Host PID/path/PFN/stable PID before first application frame; Host accepted job `5bd48925-8c88-48d2-bbd7-a62c2ba03e3e` (run `33218498644`, job `99007347897`).
- Removed temporary diagnostics/invalid unpackaged-PowerShell positive smoke while retaining immutable Actions evidence.
- Pre-version exact tree `0d37afdba33abcd9ca31f3e59d0d6dc8a1bb7e5d` passed run `33260905467`: 192/192 managed, 66/66 static, 5/5 vectors, product/package/COM smokes, zero-diff authority, deterministic workspace and verified delivery.
- Source/version authority now identifies dev.11; generated dev.11 SBOM/package/hash authority regeneration and exact-head freeze remain next.

"""
    changelog = changelog.replace("# Changelog\n\n", "# Changelog\n\n" + section, 1)
    Path("CHANGELOG.md").write_text(changelog, encoding="utf-8")

readme = Path("README.md").read_text(encoding="utf-8")
current = f"""## Workspace version
**{VERSION}** — development B2 connected-server identity/authentication is qualified; generated dev.11 authority regeneration and exact-head freeze are the current repository step.

## Current evidence-backed state
The product path remains:

`IExplorerCommand → fixed Converty.Bridge.exe → Strict Converty.EngineWorker.exe → typed preset/provider → fixed app-local ffmpeg.exe → private staging → validated no-overwrite numbered publication`

Dev.11 adds no Host routing to normal conversion. The development package stages exact sibling `Converty.Host.exe` for dormant B2 infrastructure. Real registered package COM invocation proved the shell-launched exact Bridge receives PFN `{PFN}`; package-identified parent→Host preserves it; and a real packaged Bridge→Host session authenticates connected server PID, exact image, PFN and stable PID before the first application frame. Positive acceptance: run `33218498644`, job `99007347897`.

The pre-version exact tree `{PRE_VERSION}` passed run `33260905467`: 18/18 locked restore, zero vulnerable-result packages, Release 0 warnings/errors, native Explorer, unsigned MakeAppx including Host, direct and packaged COM Invoke, Bridge→Strict Worker→FFmpeg conversion, Unicode/metacharacter paths, source/existing-destination preservation, numbered publication, MP3 exactly 320000 bit/s, 192/192 managed, 66/66 static, 5/5 vectors, zero-diff authority, deterministic double workspace and verified delivery.

## What dev.11 still does not claim
- headed Windows 11 modern Explorer UI acceptance, exact-build screenshots or crash/hang/failure matrix;
- production signed-package B2 requalification;
- status/cancel and replay/disconnect/reconnect/session acceptance;
- production FFmpeg redistribution/license/notices/signature/hash approval;
- signed production MSIX and clean Windows 11 VM lifecycle;
- final security/fuzz/chaos/release audit or end-user acceptance.

"""
readme = re.sub(r"## Workspace version\n.*?(?=## Start here\n)", current, readme, flags=re.S)
Path("README.md").write_text(readme, encoding="utf-8")

handover = f"""# Converty {VERSION} — Next-Agent Handover

## Exact current authority
- Repository: `https://github.com/aeiouofficial/converty`.
- Working branch: `{BRANCH}`.
- Frozen main: `{MAIN}`.
- Frozen dev.10 final HEAD: `{DEV10}`.
- Development B2 source-authority head: `{B2_HEAD}`.
- Pre-version exact-tree qualifier: `{PRE_VERSION}`; run `33260905467`, managed `99122561963`, static `99122562067`.
- Workspace version authority is `{VERSION}`; generated dev.11 SBOM/package/hash authority is pending regeneration from this source-authority tree.

## Dev.11 B2 result
- Host is staged beside Bridge in the development package.
- Registered package COM `IExplorerCommand::Invoke` → production shell `CreateProcessW` gives exact Bridge PFN `{PFN}`.
- Package-identified parent→existing exact-path Host launch preserves the PFN.
- Existing `BridgeClient` authenticates actual connected server PID, exact Host image, exact PFN and stable PID before first application request write.
- Real packaged Bridge→Host acceptance: run `33218498644`, job `99007347897`, Host job `5bd48925-8c88-48d2-bbd7-a62c2ba03e3e`.
- Normal conversion remains Explorer→Bridge→Strict EngineWorker→FFmpeg. Do not route it through Host.

## Pre-version exact-tree qualification
Run `33260905467` passed 18/18 locked restore, zero-vulnerability audit, Release 0 warnings/errors, native/package/direct+packaged COM/product smokes, 192/192 managed, 66/66 static, 5/5 vectors, generated-authority zero diff, deterministic workspace and verified delivery artifact `9717244755`.

## Single highest-priority next task
Regenerate and freeze **dev.11 generated authority** without product changes:
1. regenerate `machine-readable/source_sbom.spdx.json`, `machine-readable/release_sbom.spdx.json`, `machine-readable/package_manifest.json`, and `SHA256SUMS.txt` from this version-synchronized tree;
2. commit exact runner-generated bytes only;
3. require no-tree-change exact-head CI with generated-authority freshness CLEAN plus all restore/audit/build/native/package/COM/product/test/static/deterministic-workspace/delivery gates;
4. record final SHA/run/job/artifact metadata externally in the recursive handover without creating self-reference;
5. do not merge/promote merely because CI is green.

## Open shipping gates
Headed Win11 UI/screenshots; Explorer failure matrix; production signed-package B2 requalification; status/cancel and session acceptance; FFmpeg redistribution approval; signed MSIX/clean-VM lifecycle; final security/fuzz/chaos/release audit and end-user acceptance.

## Non-negotiables
RIGHT CLICK → CONVERSION MENU → FFmpeg → OUTPUT FILE. No source/external-destination overwrite; deterministic numbering; no shell/raw FFmpeg passthrough/user-selected converter/PATH lookup/network requirement; no silent Strict→Compatibility fallback; Explorer never parses hostile media; Host/Bridge stay media/process neutral; codecs/parsers/plugins stay disposable worker/provider-side; signing keys stay outside repo; Gyan FFmpeg is development-only; no headed-Win11 claims without a real headed Win11 environment.

## Recursive handover rule
End every tranche with a complete copy-paste continuation handover containing exact repo/branch/main/current SHA/tree, immutable evidence, run/job/artifact IDs, exact changes/reasons, RED/GREEN history, executed test/build/security outcomes, workspace hashes, blockers, unverified claims, ONE precise next task, all invariants, and this same rule.
"""
Path("docs/HANDOVER_NEXT_AGENT.md").write_text(handover, encoding="utf-8")
Path("docs/HANDOVER_PROMPT.txt").write_text("# CONVERTY — CONTINUATION HANDOVER\n" + handover, encoding="utf-8")

status = f"""# Implementation status — {VERSION}

## Tranche result
Development B2 connected-server identity/authentication is executable-qualified while the normal product remains `IExplorerCommand → fixed Bridge → Strict EngineWorker → typed preset/provider → fixed FFmpeg → private staging → numbered publication`. Host is staged for dormant IPC/security infrastructure only.

## B2 evidence
- Host-missing package RED: run `33202365348`, job `98954716457`.
- Package-identified parent→Host PFN preserved: run `33211928010`, job `98986920905`.
- Real registered Explorer COM→Bridge PFN preserved: run `33218030168`, job `99005949641`.
- Real packaged Bridge→authenticated Host accepted: run `33218498644`, job `99007347897`; Host job `5bd48925-8c88-48d2-bbd7-a62c2ba03e3e`.
- Development PFN `{PFN}`. Negative wrong/missing PFN, wrong path, PID race, unpackaged server and pre-frame-write cases remain fail-closed.

## Pre-version exact-tree qualification
Head `{PRE_VERSION}`, tree `{TREE}`, run `33260905467`, managed `99122561963`, static `99122562067`: 18/18 locked restore, 0 vulnerable-result packages, Release 0 warnings/errors, native/package/COM/product PASS, 192/192 managed, 66/66 static, 5/5 vectors, zero-diff authority, deterministic workspace and verified delivery.

## Current authority state
Source/version authority is `{VERSION}`. Regenerate the four generated authority files and exact-head-qualify before freezing dev.11.

## Remaining shipping gates
Headed Win11 UI/screenshots and Explorer failure matrix; production signed-package B2 requalification; status/cancel + session acceptance; FFmpeg redistribution approval; signed MSIX/clean-VM lifecycle; final security/fuzz/chaos/release/end-user acceptance.

## Historical intermittent
`StrictWorkerIsTerminatedWhenStagingGrowthExceedsOutputBudget` has intermittently produced 191/192 on prior runs; unchanged-head reruns passed 192/192. Preserve this history until independently eliminated.
"""
Path("docs/development/IMPLEMENTATION_STATUS.md").write_text(status, encoding="utf-8")

prepend_section("docs/SECURITY_THREAT_MODEL.md", "## Dev.11 B2 connected-server authentication status — 2026-08-29", f"Development full-package B2 identity/authentication is qualified: actual registered package COM shell invocation gives exact Bridge PFN `{PFN}`; exact Host child preserves it; `BridgeClient` validates actual connected server PID, exact Host image, PFN and stable PID before the first request frame. Production signed-package PFN/publisher requalification remains mandatory. Normal conversion remains Bridge→Strict EngineWorker→FFmpeg.")
prepend_section("docs/TASK_BACKLOG.md", "## Dev.11 current priority — 2026-08-29", "- Development B2 Explorer→Bridge package identity: QUALIFIED.\n- Development B2 Bridge→Host connected-server authentication: QUALIFIED.\n- CURRENT: regenerate/freeze dev.11 generated authority after source/version synchronization.\n- OPEN: headed Win11 UI, production signed-package B2 requalification, status/cancel + session acceptance, FFmpeg redistribution approval, signed MSIX/clean-VM lifecycle, final release audit.")
prepend_section("docs/TEST_AND_RELEASE_GATES.md", "## Dev.11 development qualification — 2026-08-29", "Development B2 positive acceptance is PASS in run `33218498644` / job `99007347897`; pre-version whole-tree qualification is PASS in run `33260905467` with 192/192 managed, 66/66 static, 5/5 vectors and zero-diff generated authority. Final dev.11 authority still requires regenerated dev.11 SBOM/package/hash files and exact-head green delivery. Headed Windows 11 and production signed-package acceptance remain open.")

Path(".github/workflows/dev11-source-authority-sync.yml").unlink()
Path(".github/dev11_source_authority_sync.py").unlink()

expected = {
    ".github/workflows/dev11-source-authority-sync.yml", ".github/dev11_source_authority_sync.py",
    "CHANGELOG.md", "README.md", "VERSION", "docs/HANDOVER_NEXT_AGENT.md", "docs/HANDOVER_PROMPT.txt",
    "docs/SECURITY_THREAT_MODEL.md", "docs/TASK_BACKLOG.md", "docs/TEST_AND_RELEASE_GATES.md",
    "docs/development/IMPLEMENTATION_STATUS.md", "eng/toolchain.json", "machine-readable/build_evidence.json",
    "machine-readable/ci_action_pins.json", "machine-readable/handover_state.json", "machine-readable/release_policy.json",
}
actual = set(subprocess.check_output(["git", "diff", "--name-only"], text=True).splitlines())
if actual != expected:
    raise SystemExit(f"unexpected dev11 source-authority delta: {sorted(actual)}")

subprocess.run(["git", "config", "user.name", "github-actions[bot]"], check=True)
subprocess.run(["git", "config", "user.email", "41898282+github-actions[bot]@users.noreply.github.com"], check=True)
subprocess.run(["git", "add", "-A"], check=True)
subprocess.run(["git", "commit", "-m", "docs: synchronize dev11 B2 authority"], check=True)
subprocess.run(["git", "push", "origin", f"HEAD:{BRANCH}"], check=True)
