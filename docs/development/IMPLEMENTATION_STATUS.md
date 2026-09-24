# Implementation status — dev.22 audit remediation over qualified 0.1.0-dev.21

## Frozen release baseline
The sole frozen release remains `0.1.0-dev.20` at exact main `8a1f46603aa842728247bc11b34fcccf121858fd`, tree `4bd6f8d7acbadd60a3488870c773d2eafd67ba26`, exact-main CI `33671671714` SUCCESS.

## Dev.21 implementation status
Branch: `dev/0.1.0-dev.21`.

Approved design: `d80cc33a2e7c38738f113856e95f1451fd2df1b0`.  
Implementation plan: `e54061368b476184a89dcadb1d6b8a8f6fb6cf68`.

Tasks 1–11 are implementation/evidence complete on the development line. Current pre-authority engineering head is `277f6c30f5fd22b3107604304717e56839e76641`, tree `b8c1f9ea203e532e235a94174740bb23821c7d86`. Task-11 run `35299376030` produced:
- dependency audit: 19 projects / 19 frameworks / 0 vulnerable-result packages;
- Release build: 0 warnings / 0 errors;
- 392/392 managed tests;
- 132/132 static tests;
- 5/5 raw contract vectors;
- native/package/ProbeWorker/COM/product acceptance PASS;
- Audio 36, Image 24 and Video 27 matrices plus negative/mixed isolation PASS;
- actual ffprobe/ffmpeg descendant containment PASS;
- packaged managed Copy, packet-preserving Remux and codec-changing Transcode witnesses PASS;
- hash-locked Python CI install and immutable Action-pin gates PASS.

Deterministic pre-authority workspace: SHA-256 `e2b1be0a4f5c8ab1f4cb0b936e3289795cabc81f0144d974c8e5a036b6bf1986`, 613591 bytes, 442 files. Verification then correctly failed against stale tracked generated authority, so delivery was skipped.

## Task 12 authority state
Generated artifact `10529710374` / `sha256:0029b1def03e329572ef221edc8779c89158a695b0be26ce02a05487e01e99ec` was independently rechecked:
- archive SHA-256: PASS;
- CRC: PASS;
- exact four generated members only: PASS;
- package/SBOM semantic version: **0.1.0-dev.20**.

It is therefore **not** eligible for final dev.21 synchronization. This curation advances the non-generated workspace authority to `0.1.0-dev.21`; ordinary CI must generate a new exact artifact from this curated state before any guarded sync.

Generated files that remain CI-only:
- `SHA256SUMS.txt`
- `machine-readable/package_manifest.json`
- `machine-readable/release_sbom.spdx.json`
- `machine-readable/source_sbom.spdx.json`

## Promotion/freeze rule
After guarded generated-authority sync, require exact-candidate zero-diff + complete managed deterministic workspace/delivery qualification and independent artifact verification. Promotion must stop if required live governance, signing/provenance or headed-release prerequisites remain open. Never weaken gates or create a merge/squash/post-qualification SHA merely to satisfy governance.

## Open release prerequisites
Live rulesets/main protection, headed Windows 11 Explorer acceptance, production FFmpeg/ffprobe approval, production signed-package B2, signed MSIX clean-VM lifecycle, UX/settings, Plugin SDK and final fuzz/chaos/security/release/end-user acceptance remain open.

Converty is **NOT CUSTOMER SHIP-READY**.


## Dev.22 audit remediation state — 2026-09-21
Active branch: `dev/0.1.0-dev.22-shipping-readiness`.  
Audit baseline: `1bfb6a409976abdbf781ba027f556818fb791fa7`.

A fresh critical audit confirmed concrete release-authority, batch-failure, cross-volume publication and staging-recovery defects. Canonical plan: `docs/superpowers/plans/2026-09-21-dev22-audit-remediation-shipping-hardening.md`.

The dev.21 product candidate remains immutable development evidence; dev.22 is the repair line. No audited defect is closed until a targeted RED reproducer fails for the expected reason and the corresponding GREEN plus required regression gates pass.

External release evidence remains explicitly OPEN. Completing repository logic does not authorize a customer release.


## Dev.22 pre-authority behavior qualification — 2026-09-21

Exact behavior head before metadata curation: `7e50fa9a1df4d0a8336d16596af488335424b792`, tree `a3a6deec5d753c6176fb40368a56d263b3beaa62`. CI `35664296931` established:
- 395/395 managed tests PASS; 162/162 static tests PASS; 5/5 contract vectors PASS;
- dependency audit: 19 projects / 19 frameworks / 0 vulnerable-result packages;
- Release build: 0 warnings / 0 errors;
- Native Explorer, development package, ProbeWorker, COM activation and Bridge product smoke PASS;
- Audio 36-case + mixed isolation PASS; Image 24-case + mixed isolation PASS; Video 27-case + mixed isolation PASS;
- packaged Copy/Remux/Transcode witnesses PASS;
- development FFmpeg exact tagged/hash-locked BtbN payload executes as `n9.0.2-3-ga5923073bf-20260921`;
- deterministic workspace ZIP built twice byte-identically at SHA-256 `526588af945a4eeb04984dc17792af8dd10fc1d7f2a9e9006ab3976d448b2b0a`, 642060 bytes / 462 files.

Archive semantic verification then failed exactly on stale tracked generated authority (`.github/workflows/ci.yml` hash mismatch). This is the expected pre-authority boundary. No generated authority file has been hand-edited.

Tasks 1-7 of the dev.22 remediation plan are implementation/evidence complete. Task 8 is now non-generated metadata curation followed by ordinary-CI authority generation, independent artifact verification, guarded synchronization and exact-candidate qualification.

Converty remains **NOT CUSTOMER SHIP-READY** because live GitHub governance and external production signing/provenance/headed/final-acceptance gates remain OPEN.

## Dev.22 exact synchronized candidate — 2026-09-24

Qualified subject: `1aea099a2878f72f8c95a5cd3bd1dee98a1f98b3` / tree `8369b929f3e8b48f76f720ae3a954818acf2a25a`. Ordinary CI `35937520600` proved:
- Managed 395/395 PASS; Static 162/162 PASS; vectors 5/5 PASS; dependency audit PASS, zero vulnerable-result packages; Release 0 warnings/errors.
- Native Explorer, development package, ProbeWorker, COM, Bridge, Audio36/Image24/Video27, negative/mixed, Copy/Remux/Transcode PASS.
- Candidate-base continuity and tracked generated-authority zero-diff PASS.
- Generated artifact 10783376753, SHA-256 `c3b79e9db86c52585c9c3db00d05504d43a0b6991f6c323c9f6cdf302eb1df59`; verified delivery 10783368416, SHA-256 `2697a85aa3b2a5f51f1446afb37e348bb242177dc29b11dbf3b8416b422ba207`.
- Independently inspected nested deterministic 462-file workspace ZIP `337bdb018274866803e8b6e15f7b9ea0fad4436ac3d2b912d61f340cfc76cf47` (646844 bytes): CRC, 460 manifest entries, 461 SHA256SUMS entries and equality of four authority members PASS.

Development qualification is evidence-backed; the branch is not a signed/released production candidate. Draft PR #14 and shipping issue #13 track the distinct review and genuinely OPEN external approvals.

Local user storage mandate: all local Converty project scratch/cache/logs stay under the non-C workspace `D:\converty`; previously identified C: Converty temp was relocated after per-file hash verification. The post-qualification repository storage-policy documentation amendment requires fresh exact-head qualification before it can supersede the subject above.

## dev.22 local requalification — 2026-09-24
GitHub Actions is exhausted by user instruction; run the equivalent locally without triggering Actions. This block reproduced two unprivileged-Windows test-fixture errors and a generated-authority CRLF drift, then repaired the fixtures (junctions) and the three deterministic generators (explicit LF). Local verified: Release 0 warnings/errors, 395/395 managed, 168/168 static, 5/5 vectors, 19-project dependency audit zero vulnerable-result packages and native MSVC Explorer DLL. Details: `docs/development/DEV22_LOCAL_CI_EVIDENCE_2026-09-24.md`. The latest branch head after this code change needs its own evidence; historic 1aea099a Actions artifacts remain prior-subject evidence only. Production gates remain OPEN.

## Local packaged continuation — 2026-09-24
Exact pinned BtbN development FFmpeg/ffprobe restored and verified by SHA-256 and size. On code subject `6292ccb`, packaged ProbeWorker, direct Explorer DLL COM invoke, product smoke, Audio36, Image24, Video27, malformed/truncated and mixed batches, and Copy/Remux/Transcode all PASS. Unsigned MSIX layout PASS. Registered unsigned MSIX activation fails on this laptop due Windows `0x80073CFF` sideload policy; it is not acceptance evidence. See same-day local evidence ledger. Production signing, headed lifecycle, independent review and governance are still OPEN.
