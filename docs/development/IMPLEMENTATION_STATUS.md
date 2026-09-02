# Implementation status — 0.1.0-dev.20

## Frozen baseline: dev.19
- Exact-main authority: `eb0ce66dab646427d5bef1548c12e5cc4765b2f1`, tree `337a4e11fb41bab6b6eeb462c3755381580f06c1`, run `33597504612`.
- Continuity `100143814059`, supply-chain/static `100143814189`, managed `100143814261`: SUCCESS.
- 255/255 managed, 99/99 static, 5/5 contract vectors PASS.
- Workspace SHA-256 `167b4695cca6810fe0e36e57c45a7bf11483105c0e71b955a948604f2cd9e584`, 484507 bytes, 378 entries; generated-authority artifact `9833901138`; verified-delivery artifact `9833955082`.

## Dev.20 Video qualification closure
Design A is qualification of the existing fixed Video surface only. Branch `dev/0.1.0-dev.20-video-foundation` was created from the exact frozen dev.19 main SHA.

Implemented qualification surfaces:
- `tests/static/test_dev20_video_qualification.py` — RED-first static contract.
- `tests/Converty.Core.Tests/Presets/ProductPresetRegistryTests.cs` — exact nine-source/three-action/token/path characterization.
- `build/video-input-acceptance-smoke.ps1` — 27 real packaged conversions plus repeated malformed/truncated negatives and ffprobe codec validation.
- `build/video-batch-isolation-smoke.ps1` — twice-run valid/invalid five-member batch isolation.
- `.github/workflows/ci.yml` — Video acceptance and mixed-batch gates after Image gates and before managed tests.
- design `docs/superpowers/specs/2026-09-02-dev20-video-qualification-design.md`.
- plan `docs/superpowers/plans/2026-09-02-dev20-video-qualification.md`.

No production source-code modification was required: the existing typed Video registry/provider path already met the approved behavior contract.

## TDD / behavior evidence
RED: `ea6987ddea57de71661b73e9f073cddb1c3f0bd3`, run `33668918551`, static job `100377398869`.

GREEN behavior head: `b8019ecf926fce9813fdcd2cbd74e5f59e439d08`, run `33669379940`:
- locked restore/dependency audit/Release build/native/package/COM/product gates PASS;
- Audio 36/36 + negatives + mixed batch PASS;
- Image 24/24 + negatives + mixed batch PASS;
- Video 9 sources × 3 actions = 27/27 PASS;
- ffprobe contracts: H.264+AAC MP4, VP9+Opus WebM, MP3 audio-only PASS;
- Unicode/metachar paths, source/existing-destination preservation, numbered publication and partial cleanup PASS;
- malformed and truncated inputs repeat deterministically with exit 4;
- mixed valid MP4 → malformed AVI → valid MOV → truncated MKV → valid WebM repeated twice; aggregate exit 4 after all members, later valids publish, no orphan converter processes;
- 260/260 managed tests and 103/103 static tests PASS within managed qualification; 5/5 vectors PASS.

The behavior-run workspace ZIP was byte-identical across two builds: SHA-256 `5db1e06c084e58354aa2445f3645d1b53c760fc63f565fe7960224c2eec2467f`, 505672 bytes, 383 entries. Semantic verification correctly failed against stale tracked generated authority; no final delivery was produced.

## Current pre-authority state
`VERSION` and curated CI pin authority are aligned to dev.20. Run `33669979101` generated artifact `9862090305`, digest `7cb43be8a0a67cf5e0deb19c79827c9df786a26777ddd7f11bdc9cf95d5edba2`, but it predates this full repository curation and is therefore not eligible for final synchronization.

## Required dev.20 freeze sequence
1. Run ordinary CI on the exact curated head and obtain a fresh generated-authority artifact.
2. Independently verify digest, CRC, exact four members and dev.20 alignment.
3. Guarded exact-parent/self-deleting synchronization; never hand-edit generated authority.
4. Require branch generated-authority zero-diff and complete managed deterministic workspace/delivery qualification.
5. Re-read live `main`; non-force fast-forward only if frozen dev.19 main is unchanged and candidate is a strict descendant.
6. Require fresh exact-main continuity + supply-chain/static + managed SUCCESS.
7. Independently verify exact-main generated authority and delivery artifacts; re-read refs before declaring dev.20 frozen.

## Remaining implementation/release work
After dev.20 freeze, current roadmap target is dev.21 B8 Video Copy/Remux/Transcode planning/qualification; then UX/settings, plugin SDK, production FFmpeg approval, production signed-package B2 requalification, signed MSIX lifecycle, headed Windows 11 Explorer acceptance, and final fuzz/chaos/security/release/end-user acceptance.

Automated CI does not close headed UI, production signing, production FFmpeg redistribution, signed MSIX lifecycle, or final end-user/security gates.
