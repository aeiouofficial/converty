from __future__ import annotations

import json
import subprocess
from pathlib import Path

VERSION = "0.1.0-dev.12"
NEXT_VERSION = "0.1.0-dev.13"
DATE = "2026-08-29"
PRIOR_MAIN = "ac475b5a51e19c7618a424ca689657cdf75edcaa"
PRIOR_TREE = "a0d81da9a4d593c9ba7ec23dd59073eb0e501dc9"
BEHAVIOR_HEAD = "f4c241b0895d06d2e44d72f31e07f141cdc74577"
BEHAVIOR_TREE = "30ef2ff8ebfb7f89c8f91b3c18c08432c4fdfbd1"
BEHAVIOR_RUN = 33271379504
MANAGED_JOB = 99150338647
STATIC_JOB = 99150338472
CONTINUITY_JOB = 99150338602
RED_HEAD = "ad223384400be1c5749e0b09e301f7ddd5565eda"
RED_RUN = 33271012596
RED_MANAGED_JOB = 99149375331


def load(path: str) -> dict:
    return json.loads(Path(path).read_text(encoding="utf-8"))


def save(path: str, value: dict) -> None:
    Path(path).write_text(json.dumps(value, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")


def prepend(path: str, heading: str, body: str) -> None:
    p = Path(path)
    text = p.read_text(encoding="utf-8")
    if heading in text:
        return
    first, rest = text.split("\n", 1)
    p.write_text(first + "\n\n" + heading + "\n" + body.rstrip() + "\n\n" + rest.lstrip("\n"), encoding="utf-8")


subprocess.run(["git", "merge-base", "--is-ancestor", BEHAVIOR_HEAD, "HEAD"], check=True)

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
    "Dev.12 removes the scheduler-dependent output-budget test overshoot assumption without changing production worker limits or containment.",
    f"Behavior head {BEHAVIOR_HEAD} run {BEHAVIOR_RUN} passed 192/192 managed tests plus native/package/COM/product conversion gates; the run stopped later only because tracked generated authority still described pre-change bytes.",
    "The deterministic canary writes exactly 64 KiB + 4 KiB then holds; strict launcher output-growth monitoring must detect the breach and terminate the worker.",
    "WindowsWorkerProcessLauncher, WorkerResourceLimits, AppContainer/Job Object policy, polling interval and production conversion path are unchanged.",
    "Headed Win11, production signed-package B2 requalification, production FFmpeg redistribution, signed MSIX/clean-VM lifecycle and final release audit remain open.",
]
save("eng/toolchain.json", toolchain)

state = {
    "schemaVersion": 1,
    "project": "Converty",
    "workspaceVersion": VERSION,
    "baselineDate": DATE,
    "implementationStatus": "DEV12_OUTPUT_BUDGET_TEST_DETERMINISM_BEHAVIOR_QUALIFIED_GENERATED_AUTHORITY_REGEN_PENDING",
    "defaultBranch": "main",
    "liveSourceAuthorityHeadSha": BEHAVIOR_HEAD,
    "liveSourceAuthorityTreeSha": BEHAVIOR_TREE,
    "nextWorkspaceVersion": NEXT_VERSION,
    "nextBatch": "DEV13_STATUS_CANCEL_WIRE_DECISION_AND_MINIMAL_PROTOCOL_SLICE",
    "targetOs": "Windows 11 x64",
    "priorAuthority": {
        "headSha": PRIOR_MAIN,
        "treeSha": PRIOR_TREE,
        "workflowRunId": 33268569169,
        "managedJobId": 99142859488,
        "staticJobId": 99142859587,
        "continuityJobId": 99142859660,
    },
    "dev12OutputBudgetDeterminism": {
        "planCommitSha": "e2d50ca1279631a1dd1a51ab35fb170ef5357a02",
        "redHeadSha": RED_HEAD,
        "redRunId": RED_RUN,
        "redManagedJobId": RED_MANAGED_JOB,
        "firstGreenExperimentHeadSha": "8ae59ddb87411f64c647a66a0a7f941b13d37a78",
        "firstGreenExperimentRunId": 33271120406,
        "diagnosticHeadSha": "5dfaabebaacacde1d6b99a8146d8302b7845ad6e",
        "diagnosticRunId": 33271235790,
        "diagnosticCultureFixHeadSha": "139c1058bb560756fb6139b01c87f4b277bd28f2",
        "diagnosticCultureFixRunId": 33271302074,
        "behaviorHeadSha": BEHAVIOR_HEAD,
        "behaviorTreeSha": BEHAVIOR_TREE,
        "qualificationRunId": BEHAVIOR_RUN,
        "managedJobId": MANAGED_JOB,
        "staticJobId": STATIC_JOB,
        "continuityJobId": CONTINUITY_JOB,
        "managedTests": "192/192 PASS; 0 skipped",
        "staticTests": "72/72 PASS before generated-authority zero-diff check",
        "contractVectors": "5/5 PASS",
        "releaseBuild": "PASS; 0 warnings; 0 errors",
        "dependencyAudit": "PASS; 18 projects; 18 frameworks; 0 vulnerable-result packages",
        "productPath": "PASS; native Explorer; package/MakeAppx; direct and registered COM Invoke; Bridge→Strict Worker→FFmpeg; mp3 320000 bit/s",
        "productionWorkerLimitChanged": False,
        "productionContainmentChanged": False,
        "rootCause": "The historical 512 KiB post-kill file-size ceiling was a test-only scheduler assumption. A correct budget exception had already been thrown in the historical 610304-byte failure. Dev.12 bounds the canary itself to maximum+4096 and holds it alive so detection/termination is deterministic.",
        "generatedAuthorityState": "REGEN_REQUIRED_AFTER_SOURCE_AUTHORITY_SYNC",
    },
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
        "Regenerate dev.12 source/release SBOM, package manifest and SHA256SUMS from this source-authority tree.",
        "Commit exact runner-generated authority bytes only.",
        "Require exact-main zero-diff authority plus all managed/static/product/package gates and deterministic verified dev.12 delivery.",
        "Then begin dev.13 status/cancel wire decision without changing the normal Bridge→Worker→FFmpeg product path unnecessarily.",
    ],
    "recursiveHandoverRequired": True,
    "recursiveHandoverRule": "Every completed tranche must end with a full copy-paste handover containing repository/default branch/live SHA/tree, prior authority, all commits, RED/GREEN history, run/job/artifact IDs, exact changes/reasons, executed tests/build/security outcomes, workspace hashes/counts, blockers, explicitly unverified claims, ONE precise next task, invariants, headed limitation, production signing/FFmpeg limitation, and this same recursive rule.",
}
save("machine-readable/handover_state.json", state)

build = {
    "schemaVersion": 1,
    "workspaceVersion": VERSION,
    "evidenceDate": DATE,
    "priorFinalMain": {
        "headSha": PRIOR_MAIN,
        "treeSha": PRIOR_TREE,
        "workflowRunId": 33268569169,
        "managedJobId": 99142859488,
        "staticJobId": 99142859587,
        "continuityJobId": 99142859660,
        "workspaceSha256": "8270fdf0598f73881a674fbc7dddfe5e4727ff09884aedd68d41cfffae9ae395",
    },
    "dev12BehaviorQualification": {
        "headSha": BEHAVIOR_HEAD,
        "treeSha": BEHAVIOR_TREE,
        "workflowRunId": BEHAVIOR_RUN,
        "managedJobId": MANAGED_JOB,
        "staticJobId": STATIC_JOB,
        "continuityJobId": CONTINUITY_JOB,
        "lockedRestore": "18/18 PASS",
        "dependencyAudit": "PASS; 18 projects; 18 frameworks; 0 vulnerable-result packages",
        "releaseBuild": "PASS; 0 warnings; 0 errors",
        "nativeExplorer": "PASS",
        "developmentPackageAndMakeAppx": "PASS; 33 files",
        "directAndPackagedComInvoke": "PASS",
        "productBridgeStrictWorkerFfmpeg": "PASS; mp3 / 320000 bit/s; Unicode/metacharacter path; source and existing destination preserved; numbered publication",
        "managedTests": {"total": 192, "succeeded": 192, "failed": 0, "skipped": 0},
        "staticTests": "72/72 PASS",
        "contractVectors": "5/5 PASS",
        "workspaceDoubleBuild": {"sha256": "2d441ab980035dd63a4101ffb0548a3c47f6adc3049f88055bc4dd6f41b8326e", "bytes": 391329, "files": 335, "byteIdentical": True, "integrityCheck": "EXPECTED_FAIL_STALE_PACKAGE_MANIFEST"},
        "generatedAuthorityDiff": "EXPECTED_STALE_AFTER_SOURCE_CHANGE",
    },
    "dev12TddHistory": {
        "red": {"headSha": RED_HEAD, "runId": RED_RUN, "managedJobId": RED_MANAGED_JOB, "result": "191/192; target failed because requested canary mode did not exist"},
        "failedUnboundedExperiment": {"headSha": "8ae59ddb87411f64c647a66a0a7f941b13d37a78", "runId": 33271120406, "result": "191/192; abandoned"},
        "diagnosticAnalyzerFailure": {"headSha": "5dfaabebaacacde1d6b99a8146d8302b7845ad6e", "runId": 33271235790, "result": "Release analyzer CA1305 RED before tests"},
        "diagnosticCultureFix": {"headSha": "139c1058bb560756fb6139b01c87f4b277bd28f2", "runId": 33271302074},
        "greenBehavior": {"headSha": BEHAVIOR_HEAD, "runId": BEHAVIOR_RUN, "managedJobId": MANAGED_JOB, "result": "192/192; product path PASS; only generated-authority/workspace-manifest freshness remained RED"},
    },
    "historicalIntermittent": {
        "test": "StrictWorkerIsTerminatedWhenStagingGrowthExceedsOutputBudget",
        "dev10RunId": 33044741340,
        "dev11PreVersionInitialManagedJobId": 99007945036,
        "dev11FinalInitialManagedJobId": 99124384946,
        "historicalObservedBytes": 610304,
        "historicalTestOnlyCeilingBytes": 524288,
        "status": "ROOT_CAUSE_ELIMINATED_IN_TEST_HARNESS; PRODUCTION LIMIT/CONTAINMENT UNCHANGED",
    },
    "currentAuthorityState": "DEV12_SOURCE_VERSION_AUTHORITY_SYNCHRONIZED; GENERATED_SBOM_PACKAGE_HASH_AUTHORITY_REGENERATION_REQUIRED",
    "headedWindows11Acceptance": "OPEN",
    "productionSignedPackageB2Requalification": "OPEN",
    "productionFfmpegRedistributionApproval": "OPEN",
}
save("machine-readable/build_evidence.json", build)

readme = Path("README.md")
text = readme.read_text(encoding="utf-8")
start = text.index("## Workspace version")
end = text.index("## Start here")
replacement = f'''## Workspace version
**{VERSION}** — output-budget containment verification is deterministic without weakening production limits or containment.

## Current evidence-backed state
The product path remains:

`IExplorerCommand → fixed Converty.Bridge.exe → Strict Converty.EngineWorker.exe → typed preset/provider → fixed app-local ffmpeg.exe → private staging → validated no-overwrite numbered publication`

Dev.12 fixes the historical output-budget test intermittent at the harness boundary. The canary now writes exactly 64 KiB + 4 KiB and then holds, so the existing strict launcher must detect a bounded breach and terminate the worker. `WindowsWorkerProcessLauncher`, production `WorkerResourceLimits`, AppContainer/Job Object containment, poll interval, and normal conversion routing are unchanged.

Behavior head `{BEHAVIOR_HEAD}` run `{BEHAVIOR_RUN}` passed 18/18 locked restore, zero vulnerable-result packages, Release 0 warnings/errors, native Explorer, unsigned MakeAppx, direct and registered COM Invoke, Bridge→Strict Worker→FFmpeg conversion, Unicode/metacharacter paths, source/existing-destination preservation, numbered publication, MP3 exactly 320000 bit/s, 192/192 managed, 72/72 static and 5/5 vectors. The run stopped only at tracked generated-authority/workspace-integrity freshness because the source bytes had changed; dev.12 generated authority regeneration is the current closure step.

## What dev.12 still does not claim
- headed Windows 11 modern Explorer UI acceptance, exact-build screenshots or crash/hang/failure matrix;
- production signed-package B2 requalification;
- status/cancel and replay/disconnect/reconnect/session acceptance;
- production FFmpeg redistribution/license/notices/signature/hash approval;
- signed production MSIX and clean Windows 11 VM lifecycle;
- final security/fuzz/chaos/release audit or end-user acceptance.

'''
readme.write_text(text[:start] + replacement + text[end:], encoding="utf-8")

changelog = Path("CHANGELOG.md")
text = changelog.read_text(encoding="utf-8")
heading = "## 0.1.0-dev.12 — 2026-08-29"
if heading not in text:
    section = f'''{heading}
- Eliminated the historical `StrictWorkerIsTerminatedWhenStagingGrowthExceedsOutputBudget` scheduler-dependent test assumption without changing production output limits or containment.
- Replaced the arbitrary 512 KiB post-kill ceiling with a canary that writes exactly 64 KiB + 4 KiB and then holds until the existing launcher detects the breach and terminates the Job Object.
- Preserved RED evidence at `{RED_HEAD}` / run `{RED_RUN}` and failed intermediate experiments rather than rewriting history.
- Behavior head `{BEHAVIOR_HEAD}` / run `{BEHAVIOR_RUN}` passed 192/192 managed tests, 72/72 static tests, 5/5 vectors, Release 0 warnings/errors, native/package/COM/product conversion gates; only generated-authority freshness remained intentionally open.
- Production `WindowsWorkerProcessLauncher`, resource limits, AppContainer/Job Object isolation, output polling and normal Bridge→Worker→FFmpeg path are unchanged.

'''
    text = text.replace("# Changelog\n\n", "# Changelog\n\n" + section, 1)
    changelog.write_text(text, encoding="utf-8")

handover = f'''# CONVERTY — CONTINUATION HANDOVER
# PRODUCT-FIRST ROADMAP — {VERSION} OUTPUT-BUDGET DETERMINISM

Continue development directly in:

https://github.com/aeiouofficial/converty

Default branch: `main`.

Repository `main` is the only durable authority. Never treat local files, chat state, or side-branch-only work as completed authority. Re-fetch live `main` before every write and again before every completion claim.

## CURRENT SOURCE AUTHORITY

Version: `{VERSION}`
Source behavior head before generated-authority regeneration: `{BEHAVIOR_HEAD}`
Tree: `{BEHAVIOR_TREE}`
Prior final main: `{PRIOR_MAIN}` / tree `{PRIOR_TREE}`

## DEV.12 ROOT CAUSE AND FIX

Historical failures of `StrictWorkerIsTerminatedWhenStagingGrowthExceedsOutputBudget` had already received the expected `WorkerOutputLimitExceededException`; one recorded 610304 staged bytes while the test imposed an unrelated 524288-byte ceiling. The product security contract requires finite output growth, termination and no publication, not a 512 KiB scheduler-dependent overshoot ceiling.

Dev.12 changes only the test harness. The canary writes exactly 69632 bytes (64 KiB configured test budget + one 4096-byte block), flushes incrementally, then holds for two minutes. The existing strict launcher must observe 65537–69632 bytes and terminate it. Production worker limits, AppContainer/Job Object containment, poll interval and launcher code are unchanged.

TDD history:
- plan: `e2d50ca1279631a1dd1a51ab35fb170ef5357a02`
- RED: `{RED_HEAD}`, run `{RED_RUN}`, managed `{RED_MANAGED_JOB}` — 191/192 because the new canary mode intentionally did not exist.
- failed unbounded experiment: `8ae59ddb87411f64c647a66a0a7f941b13d37a78`, run `33271120406` — preserved.
- diagnostic analyzer RED: `5dfaabebaacacde1d6b99a8146d8302b7845ad6e`, run `33271235790` — CA1305 before tests; preserved.
- diagnostic culture fix: `139c1058bb560756fb6139b01c87f4b277bd28f2`, run `33271302074`.
- bounded write-and-hold behavior head: `{BEHAVIOR_HEAD}`, run `{BEHAVIOR_RUN}`.

Behavior-head evidence:
- continuity job `{CONTINUITY_JOB}` PASS
- managed job `{MANAGED_JOB}`: 18/18 restore PASS; dependency audit 18 projects/18 frameworks/0 vulnerable-result packages; Release 0 warnings/0 errors; native Explorer PASS; development package/MakeAppx PASS; direct and registered COM Invoke PASS; product Bridge→Strict Worker→FFmpeg PASS; MP3 320000 bit/s; Unicode/metacharacter path; source and existing destination preserved; numbered collision publication; 192/192 managed PASS.
- static job `{STATIC_JOB}`: 72/72 static and 5/5 vectors PASS before expected generated-authority zero-diff failure.
- deterministic package A/B were byte-identical at SHA `2d441ab980035dd63a4101ffb0548a3c47f6adc3049f88055bc4dd6f41b8326e`; integrity check then correctly rejected the stale package manifest for the modified test file.

## HARD PRODUCT / SECURITY INVARIANTS

Preserve Explorer → native `IExplorerCommand` → fixed app-local Bridge → strict disposable EngineWorker/provider → fixed app-local FFmpeg → private staging → transactional numbered publication.

Preserve Unicode/metacharacter filenames; source preservation; existing-destination preservation; deterministic collisions; no shell command construction; no raw FFmpeg passthrough; no user-selected converter; no PATH lookup; no network dependency; no silent Strict→Compatibility fallback; no hostile media parsing in Explorer; Bridge/Host media/process neutrality; worker/provider-only parsers/codecs/plugins; no signing private keys in repo.

Gyan FFmpeg is development qualification input only and is NOT production redistribution approval.

Do not redesign the direct shell Bridge launch or direct Host launcher. Development B2 package identity/authentication remains qualified; production signed-package B2 must still be requalified.

## HEADED WINDOWS 11 LIMITATION

There is no real headed Windows 11 Explorer environment here. Do not claim modern submenu visual acceptance, mouse-driven acceptance, screenshots, headed Explorer failure matrix or end-user UI acceptance.

## OPEN SHIPPING BLOCKERS

1. headed Windows 11 modern Explorer acceptance
2. exact-build screenshots
3. Explorer crash/hang/failure headed matrix
4. production signed-package B2 requalification
5. status/cancel wire decision
6. replay/disconnect/reconnect/session acceptance
7. production FFmpeg redistribution/license/notices/signature/hash approval
8. signed production MSIX
9. clean Windows 11 VM install/update/uninstall
10. final security/fuzz/chaos/release audit
11. end-user acceptance

## ONE PRECISE NEXT TASK

After dev.12 generated authority and exact-main delivery are frozen, start `{NEXT_VERSION}` with the status/cancel wire decision and the smallest protocol slice needed to expose real job status/cancellation semantics without rerouting normal conversion through Host unnecessarily. Work test-first and keep durable commits directly on GitHub `main` under `AGENTS.md`.

## RECURSIVE HANDOVER RULE

Every completed tranche must end with a new full copy-paste handover containing repo/default branch/live SHA/tree, prior authority, all commits, RED/GREEN history, run/job/artifact IDs, exact changes/reasons, executed tests/build/security outcomes, workspace hashes/counts, blockers, explicitly unverified claims, ONE precise next task, invariants, headed limitation, production signing/FFmpeg limitation, and this same recursive rule.
'''
Path("docs/HANDOVER_NEXT_AGENT.md").write_text(handover, encoding="utf-8")
Path("docs/HANDOVER_PROMPT.txt").write_text(handover, encoding="utf-8")

prepend("docs/development/IMPLEMENTATION_STATUS.md", "## Dev.12 output-budget determinism — 2026-08-29", f'''- Behavior head `{BEHAVIOR_HEAD}` / run `{BEHAVIOR_RUN}` passed the full product path and 192/192 managed tests.
- Historical 512 KiB post-kill ceiling was a test-only scheduler assumption; historical 610304-byte failure had already thrown the correct output-limit exception.
- Canary now writes exactly 64 KiB + 4 KiB then holds, making breach detection/termination deterministic.
- Production launcher/resource limits/AppContainer/Job Object/poll interval are unchanged.
- Generated dev.12 SBOM/package/hash authority and final exact-main delivery remain the closure step.''')

prepend("docs/TASK_BACKLOG.md", "## Dev.12 closure / dev.13 next", f'''- [x] Replace scheduler-dependent output-budget overshoot assertion with bounded write-and-hold canary.
- [x] Preserve production output limit and strict containment unchanged.
- [x] Behavior qualification at `{BEHAVIOR_HEAD}`: 192/192 managed, 72/72 static, 5/5 vectors, product/package/COM gates PASS.
- [ ] Regenerate/synchronize dev.12 generated authority and require exact-main zero-diff CI + deterministic verified delivery.
- [ ] Dev.13: status/cancel wire decision and smallest protocol slice.''')

prepend("docs/TEST_AND_RELEASE_GATES.md", "## Dev.12 output-budget determinism gate", f'''Behavior head `{BEHAVIOR_HEAD}` / run `{BEHAVIOR_RUN}` proves the strict worker output-growth gate with a bounded test producer: configured maximum 65536 bytes, producer target 69632 bytes then hold, required `WorkerOutputLimitExceededException`, exact max metadata, and observed/final growth constrained to 65537–69632 bytes. Production limits and containment are unchanged. Final dev.12 still requires generated-authority zero diff and deterministic exact-main delivery.''')

for helper in [Path(".github/dev12_source_authority_sync.py"), Path(".github/workflows/dev12-source-authority-sync.yml")]:
    helper.unlink(missing_ok=True)
