# Converty Implementation Backlog

## Dev.21 B8 Copy/Remux/Transcode — Tasks 1–11
- [x] Approved architecture/security design committed: `d80cc33a2e7c38738f113856e95f1451fd2df1b0`.
- [x] Implementation plan committed: `e54061368b476184a89dcadb1d6b8a8f6fb6cf68`.
- [x] Bounded typed MediaProbe facts and strict serialization.
- [x] Streaming-bounded worker stdout and read-only ProbeWorker file scope.
- [x] Strict ProbeWorker + fixed app-local ffprobe boundary.
- [x] Provider-only fixed FFmpeg token ownership.
- [x] Deterministic VideoPlanningPolicy over bounded probe facts.
- [x] Mode-aware EngineWorker and managed byte-exact Copy/SHA-256 proof.
- [x] Stage → probe → plan → explicit-mode execute → post-probe TargetMediaContract → transactional publish.
- [x] Real ffprobe/ffmpeg descendant Job/AppContainer filesystem/network/orphan containment canaries.
- [x] Packaged real Copy/Remux/Transcode witnesses plus Audio36/Image24/Video27 recursive regressions.
- [x] Governance/supply-chain source hardening: hash-locked Python static dependencies, immutable Action pins, least-privilege permanent CI.

## Dev.21 Task 12 — generated authority / exact-candidate qualification
- [x] Fresh-read frozen main, dev.21 head, CI, Slack and Drive; GitHub authority reconciled.
- [x] Independently verify Task-11 generated artifact `10529710374`: digest/CRC/exact four members PASS.
- [x] Detect and reject the artifact for final dev.21 sync because its workspace/SBOM version is still `0.1.0-dev.20`.
- [x] Curate non-generated dev.21 version/toolchain/release/evidence/handover documentation before final generation.
- [x] Dev.21 ordinary generation/guarded synchronization/exact-candidate qualification completed; immutable qualified candidate is `c3bc042aea3154e30ce2720db4722cbda27bcb34`.
- [x] Dev.21 guarded generated-authority synchronization completed.
- [x] Dev.21 synchronized candidate qualified with exact generated-authority/delivery evidence.
- [x] Dev.21 final generated-authority and delivery artifacts independently verified.
- [ ] Dev.21 promotion intentionally stopped because live governance and external production release prerequisites remain OPEN.

## Frozen release authority
- [x] `0.1.0-dev.20` remains exact-main frozen at `8a1f46603aa842728247bc11b34fcccf121858fd` / tree `4bd6f8d7acbadd60a3488870c773d2eafd67ba26` / CI `33671671714` SUCCESS.

## Release blockers that remain open
- [ ] Live GitHub ruleset/main branch-protection enforcement.
- [ ] Headed Windows 11 modern Explorer exact-build UI/screenshots and crash/hang/failure matrix.
- [ ] Production signed-package B2 identity/authentication requalification.
- [ ] Production FFmpeg/ffprobe redistribution/license/notices/signature/hash approval.
- [ ] Signed production MSIX clean Windows 11 lifecycle acceptance.
- [ ] UX/settings and Plugin SDK release gates.
- [ ] Final fuzz/chaos/security/release/end-user acceptance.

## Execution rule
Check a box only when matching evidence exists. Preserve historical evidence. Never hand-edit generated SBOM/package/hash authority, never force-push main, and never convert a development-branch CI result into release authority.


## Dev.22 audit remediation / shipping hardening — 2026-09-21
Plan: `docs/superpowers/plans/2026-09-21-dev22-audit-remediation-shipping-hardening.md`

- [x] Critical read-only audit completed against current GitHub authority; confirmed defect set documented.
- [x] Task 1: release-gate state machine, live-authority requirement and exact qualified-subject/artifact evidence binding.
- [x] Task 2: governance/readiness CI integration and non-circular candidate-base/promotion continuity checks.
- [x] Task 3: production FFmpeg evidence validation and development-evidence rejection.
- [x] Task 4: production MSIX/B2 exact-artifact evidence contract and cryptographic verification path.
- [x] Task 5: content-level secret/private-key scanning in release preflight/CI.
- [x] Task 6: complete per-member batch failure isolation and structured partial-success results.
- [x] Task 7: destination-volume durable atomic publication and owned stale staging recovery.
- [x] Task 8: dev.22 version/evidence curation, guarded generated-authority sync, exact subject `1aea099a2878f72f8c95a5cd3bd1dee98a1f98b3` / CI `35937520600` qualification and independent artifacts PASS.
- [ ] This post-qualified documentation/storage-policy amendment must regenerate/synchronize the four CI-generated authority files and pass fresh exact-PR-head CI; final evidence belongs in draft PR #14.
- [ ] Final whole-branch review after exact synchronized dev.22 candidate qualification.

### Dev.22 completion rule
Code remediation may be complete while customer shipping remains blocked. Do not mark production FFmpeg, signed MSIX/B2, headed Windows 11 acceptance, final fuzz/chaos/security/end-user acceptance, or repository governance PASS without fresh real evidence.

## Current shipping / local storage — 2026-09-24
- [x] Create draft development review PR [#14](https://github.com/aeiouofficial/converty/pull/14), without merging into frozen main.
- [x] Create explicit external shipping gates issue [#13](https://github.com/aeiouofficial/converty/issues/13); all production/legal/governance/headed/final-acceptance evidence remains OPEN.
- [x] Move two verified old local Converty temp directories (13 files, ~2.6 MB) from C: into `D:\converty\_temp\migrated-from-C`; per-file SHA-256 and counts matched before original deletion.
- [x] Add `AGENTS.md` non-C storage rule, project-root PowerShell temp/cache bootstrap and static regression guards.
- [ ] Obtain genuine independent production approvals and actual signed/clean-headed Windows 11 evidence in issue #13. Do not mark a customer release while any item remains OPEN.
