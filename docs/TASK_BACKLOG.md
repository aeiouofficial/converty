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
- [x] Live GitHub main ruleset #24086373 ACTIVE and main protected=true as verified 2026-09-27. Deletion/non-fast-forward prohibited, linear history and signatures required, one independent PR approval with stale-review dismissal, four exact named CI checks strictly required, no bypass. Frozen main untouched.
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
- [x] Locally requalified dev.22 source `6292ccb`: managed 395, static 168, 5 vectors, pinned real packaged matrices, native, deterministic generated authority and independent committed-source workspace archives. No hosted Actions.
- [x] Historical pinned package evidence/authority was regenerated and pushed at 99144b. Subsequent actual-FFmpeg cancellation test failed 394/395 RED and isolated PID startup repeat exposed another bounded-fixture failure; test-only correction passed 3/3 targeted and 395/395 full managed GREEN.
- [x] Verified exact committed test-only subject `29ce1b260adbb4d84e8e0e720c19186d0db4e84d`: local Release 0 warnings/errors, 395/395 managed, 168/168 static, 5/5 vectors; four generated authority files zero-diff; two identical committed-HEAD ZIPs SHA-256 `1aec74d58dc8586deec6b4b9e96e0ee2ff35266a91f2166afa99036450c89a1a`, 667625 bytes / 466 entries, CRC, 464/464 manifest, 465/465 sums, four exact authority bytes PASS. Results published in PR #14 and issue #13.
- [ ] Controlled Windows 11 registered MSIX COM and headed lifecycle plus independently reviewed production prerequisites remain OPEN. Live main governance is already verified PASS.
- [x] Exact pinned development FFmpeg archive verified (192925997 bytes; SHA-256 matches), unsigned MSIX with ffmpeg/ffprobe validated, packaged ProbeWorker/direct staged COM/product + Audio36/Image24/Video27/Copy/Remux/Transcode and negative/mixed acceptance PASS locally on source subject `6292ccb`. Historical corrupt downloads remain rejected.
- [ ] Unsigned **registered** COM path on local Windows is blocked by `0x80073CFF` (sideload/developer policy). Validate on explicitly configured controlled test VM or approved host; never silently weaken user machine policy.
- [ ] Final whole-branch review after exact synchronized dev.22 candidate qualification.
- [x] 2026-09-29 continuation-audit finding: runtime private staging ignored CONVERTY_WORKSPACE_ROOT and could write project staging under local C:. Fixed workspace-first with installed-product fallback; RED committed-source witness + 395/395 managed + 169/169 static + 5/5 vectors PASS. Exact pushed subject 3ddac2be5a16ab93dcd383786fc648aeca69271a and double ZIP 67ba2ffc12bb8ebea22fb252482cd42024493b1c440e16938e13634188443b7d verified; Slack #13 / canonical Drive mirrors reconciled.

### Dev.22 completion rule
Code remediation may be complete while customer shipping remains blocked. Do not mark production FFmpeg, signed MSIX/B2, headed Windows 11 acceptance, final fuzz/chaos/security/end-user acceptance, or repository governance PASS without fresh real evidence.

## Current shipping / local storage — 2026-09-24
- [x] Create draft development review PR [#14](https://github.com/aeiouofficial/converty/pull/14), without merging into frozen main.
- [x] Create explicit external shipping gates issue [#13](https://github.com/aeiouofficial/converty/issues/13); all production/legal/governance/headed/final-acceptance evidence remains OPEN.
- [x] Move two verified old local Converty temp directories (13 files, ~2.6 MB) from C: into `D:\converty\_temp\migrated-from-C`; per-file SHA-256 and counts matched before original deletion.
- [x] Add `AGENTS.md` non-C storage rule, project-root PowerShell temp/cache bootstrap and static regression guards.
- [ ] Obtain genuine independent production approvals and actual signed/clean-headed Windows 11 evidence in issue #13. Do not mark a customer release while any item remains OPEN.

## 2026-09-27 authenticated main governance
- [x] Repository admin permissions confirmed; created ruleset [#24086373](https://github.com/aeiouofficial/converty/rules/24086373), enforced exclusively for refs/heads/main with no bypass; GitHub API returned main protected=true and six effective main rules. The repository's own readiness verifier accepted the live ruleset.
- [ ] Registered package COM and headed clean Windows 11 acceptance on an authorized host; local laptop blocks unsigned registration by 0x80073CFF. Production FFmpeg licensing, signed MSIX/B2, final security/fuzz/chaos/end-user and UX/Plugin SDK approvals and independent PR review still OPEN.
- [ ] Once external gates and GitHub Actions capacity genuinely available, require all four real check contexts on the exact release candidate; DO NOT remove rules or force-promote frozen main.

## 2026-10-26 verification follow-up
- [x] Without an SDK update, run the seven built xUnit/MTP executables again: **395/395 PASS, zero skips** on exact `d22a00c`.
- [x] Confirm source/release SBOM, package manifest and SHA256SUMS regenerate byte-identically (4/4).
- [x] GitHub d301068 exact committed-HEAD double ZIP verified: SHA-256 a76e705cf2c5461f883a21ab1501fe17087df91efd8c15cce5c0d00ec918a800; 681964 bytes/466 entries, CRC/464 manifest/465 hashes/4 authority PASS. Slack #13 PROCESSED, #14 TS 1793027934.740809 OPEN.
- [ ] Preserve draft PR #14, frozen main and OPEN release gates; rotate Slack Handover #13 only after exact committed-HEAD evidence and cloud reconciliation.

## 2026-10-20 release-audit continuation
- [x] Recreate the local workspace under `D:\converty` and verify exact source `d22a00c0430d781ff3fe3f1264bf9e67f9b0a879`.
- [x] Without updating the SDK, prove compatibility build 0 warnings/errors, managed 395/395, static 170/170, vectors 5/5, dependency audit 19/19 with zero vulnerable-result packages, repo/release/action-pin preflight PASS.
- [x] Compile native Explorer targets with the installed VS 2022/MSVC toolchain without installing Ninja.
- [x] Validate unsigned MSIX layout/schema with MakeAppx without converter binaries.
- [ ] Restore reproducible development FFmpeg input. The historical immutable BtbN URL and asset ID now return 404. Recover the exact trusted bytes or deliberately repin a new immutable development payload and fully requalify; never use latest/PATH fallback.
- [ ] Run fresh exact-head packaged ProbeWorker/product Audio36/Image24/Video27/negative/mixed/Copy/Remux/Transcode matrices after development FFmpeg reproducibility is repaired.
- [ ] Run final repository-pinned .NET 10.0.400 exact-path qualification before any release claim; current 10.0.112 evidence is compatibility-only.
- [ ] Implement B9 Settings + notifications + queue/progress/error/cancel UI; `src/Converty.Settings` currently has no executable implementation.
- [ ] Complete B10 Plugin SDK proof, B11 security/fuzz/chaos acceptance and B12 production signing/provenance/headed Windows lifecycle.
- [ ] Obtain independent PR/release artifact review and only then perform protected exact-candidate promotion plus fresh exact-main verification.
