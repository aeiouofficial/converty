# Converty — GitHub-only continuation: dev.22 exact candidate / storage-policy closure

Date: 2026-09-24 (Europe/Amsterdam). Repository: `aeiouofficial/converty`. Current development branch: `dev/0.1.0-dev.22-shipping-readiness`. Current workspace version: `0.1.0-dev.22`. **NOT CUSTOMER SHIP-READY.**

## Immutable authorities

- Frozen `main`: `8a1f46603aa842728247bc11b34fcccf121858fd`, tree `4bd6f8d7acbadd60a3488870c773d2eafd67ba26`, exact-main CI 33671671714 SUCCESS. Never move it while production gates are open.
- Immutable dev.21 development candidate: `c3bc042aea3154e30ce2720db4722cbda27bcb34`, tree `47ecb6f1f952fcdf286e60dd81935a0d81af56ee`.
- Last fully qualified synchronized dev.22 *subject*: `1aea099a2878f72f8c95a5cd3bd1dee98a1f98b3`, tree `8369b929f3e8b48f76f720ae3a954818acf2a25a`, CI `35937520600`. This documentation/storage-policy amendment creates a newer tree and **must be independently requalified**; fresh-read the actual branch HEAD before any action.

## What passed on the qualified subject

- Managed 395/395, Static 162/162, contract vectors 5/5; exact source version `0.1.0-dev.22`; Release build 0 warnings/errors and dependency audit PASS.
- Native Explorer/package/ProbeWorker/COM/Bridge; Audio36, Image24, Video27, negative/mixed batches, Copy/Remux/Transcode PASS.
- Candidate-base continuity and generated-authority zero-diff PASS.
- Authority artifact ID `10783376753`, digest `sha256:c3b79e9db86c52585c9c3db00d05504d43a0b6991f6c323c9f6cdf302eb1df59`.
- Delivery artifact ID `10783368416`, digest `sha256:2697a85aa3b2a5f51f1446afb37e348bb242177dc29b11dbf3b8416b422ba207`.
- Independent nested workspace verification: ZIP SHA-256 `337bdb018274866803e8b6e15f7b9ea0fad4436ac3d2b912d61f340cfc76cf47`; 646844 bytes / 462 entries; CRC PASS, 460 manifest hashes and 461 SHA256SUMS rows PASS; four authority members byte-match generated artifact.

## What changed after the qualification subject

- A verified local-laptop cleanup copied two old Converty temporary folders (13 files, about 2.6 MB) off C: into `D:\converty\_temp\migrated-from-C`; counts and SHA-256s matched before originals were deleted.
- The local bootstrap `D:\converty\setup-workspace-env.ps1` configures per-process TEMP/TMP/Python/NuGet/npm/.NET caches under D:.
- Repository `AGENTS.md` now distinguishes frozen main from development qualification and prohibits local Converty scratch/cache/logs on C:. `build/use-workspace-temp.ps1` and static policy regression tests provide a reproducible local workflow.

## Exact next executable action

1. Fresh-read current branch HEAD, frozen main, open PR #14, issue #13, and all CI for the **newest** SHA; do not mistake `1aea099a` for the newest HEAD after this amendment.
2. Run ordinary CI on the exact post-amendment source commit. Read Static/Managed/continuity/readiness; only generated-authority drift is expected before sync.
3. Independently download/verify newly generated artifact: GitHub digest, CRC, exact four members, every member hash and version `0.1.0-dev.22`.
4. Synchronize **only** `SHA256SUMS.txt`, `machine-readable/package_manifest.json`, `machine-readable/release_sbom.spdx.json`, `machine-readable/source_sbom.spdx.json` via guarded exact-parent/branch update; never hand-edit.
5. If bot sync does not trigger ordinary CI, create a no-tree-change qualification trigger and require exact current SHA: all managed/native/product/static/contract gates green, generated-authority zero-diff and deterministic verified delivery; independently inspect both final artifacts.
6. Update draft PR #14 and issue #13 with exact final SHA/tree/CI/jobs/artifact IDs/digests and reviewer outcome. Do not move `main` while production evidence and live governance are OPEN.

## Remaining production blockers

See [shipping issue #13](https://github.com/aeiouofficial/converty/issues/13) and [draft PR #14](https://github.com/aeiouofficial/converty/pull/14): active main ruleset/protection, production FFmpeg redistribution/provenance, real signed production MSIX/B2/certificate/timestamp, clean headed exact Windows 11 Explorer lifecycle, UX/settings and Plugin SDK release gates, final security/fuzz/chaos/end-user approvals. Dev-only BtbN FFmpeg and unsigned Converty.Dev package are not production evidence.

## Architecture and lifecycle

`Explorer → fixed Bridge → private staging → strict RO ProbeWorker/fixed ffprobe → typed bounded facts → Core planner → strict EngineWorker → managed Copy or provider-fixed Remux/Transcode → fixed app-local FFmpeg → post-probe TargetMediaContract → durable destination-volume temp → transactional numbered no-overwrite publication`.

No shell/raw FFmpeg arguments, PATH/CWD discovery, ordinary conversion network, silent Strict→Compatibility, hardware acceleration, private signing keys or invented release evidence. Preserve every historical RED and Changelog entry. GitHub is the authority for code/CI/release evidence; current user scope is GitHub-only. Update canonical repository docs in place after meaningful blocks; historical Slack/Drive Handover #12 is stale and is not the current GitHub continuation authority. On handoff fresh-read GitHub, reconcile roadmap/plan/tasks/changelog/evidence, and publish exactly one context-free successor repository handover.
