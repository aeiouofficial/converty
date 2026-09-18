# Implementation status — 0.1.0-dev.21

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
