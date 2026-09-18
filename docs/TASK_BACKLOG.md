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
- [ ] Run ordinary CI on the exact curated dev.21 head and independently verify fresh dev.21 generated authority.
- [ ] Guarded exact-parent/self-deleting synchronization of the exact four generated files only.
- [ ] Require synchronized-candidate generated-authority zero-diff and complete Windows deterministic workspace/delivery qualification.
- [ ] Independently verify final generated-authority and delivery artifacts.
- [ ] Promote/freeze only if all required live governance/release prerequisites are genuinely green and exact-candidate compatible.

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
