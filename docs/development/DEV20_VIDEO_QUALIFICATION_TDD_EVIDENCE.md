# Dev.20 Video Qualification — TDD Evidence

Date: 2026-09-02  
Branch: `dev/0.1.0-dev.20-video-foundation`  
Frozen base: dev.19 exact-main `eb0ce66dab646427d5bef1548c12e5cc4765b2f1`

## Scope
Dev.20 closes evidence for the already-existing fixed Video surface. It does not introduce new Video actions, remux/copy/transcode planning, hardware acceleration, HDR/subtitle/metadata expansion, or a second media architecture.

## Design / plan
- Design: `9da11ee8a1c8063fb21493cbdde3ce5197c7d968`
- Plan: `5295ae7b7304f82e567b58095b4600ea664b4b48`

## RED
Static contract commit: `ea6987ddea57de71661b73e9f073cddb1c3f0bd3`  
Run: `33668918551`  
Static job: `100377398869`

The new dev.20 static contract failed because `build/video-input-acceptance-smoke.ps1`, `build/video-batch-isolation-smoke.ps1`, and their CI wiring did not yet exist. This is the preserved intended RED.

## Implementation commits
- `680ed9ff3b3012b4848667124257924b2f95d7b6` — pin exact Video registry/action/token/path behavior in managed tests.
- `57a18c7f3c237dffb8fab056acf635f848e0a097` — real 27-case packaged Video acceptance harness.
- `f0be9311cf84dfc9ed63266db8464cab779c933e` — repeated mixed Video batch failure-isolation harness.
- `b8019ecf926fce9813fdcd2cbd74e5f59e439d08` — wire Video gates into Windows CI.
- `ddaac6adeabfb30266ec4d210c9ff1313be40d32` — bump workspace version to dev.20.
- `4121f91da887dee5f873e42f6699904c7c0aa429` — align curated CI pin manifest workspaceVersion with dev.20.

No production source-code change was required by behavior qualification.

## GREEN behavior evidence
Run `33669379940` on exact `b8019ecf926fce9813fdcd2cbd74e5f59e439d08`:

- locked restore: PASS;
- dependency audit: PASS, 18 projects / 18 frameworks / 0 vulnerable-result packages;
- Release build: PASS, 0 warnings / 0 errors;
- native Explorer/package/COM/product Bridge→FFmpeg gates: PASS;
- Audio 36 source/action conversions + repeated malformed/truncated + mixed batch: PASS;
- Image 24 source/action conversions + repeated malformed/truncated + mixed batch: PASS;
- Video 9 source extensions × 3 fixed actions = 27/27 real packaged conversions: PASS;
- `video.mp4.h264`: ffprobe H.264 video + AAC audio: PASS;
- `video.webm.vp9`: ffprobe VP9 video + Opus audio: PASS;
- `extract.audio.mp3`: ffprobe MP3 audio with no video: PASS;
- Unicode/metacharacter paths: PASS;
- source/pre-existing destination byte preservation and numbered collision publication: PASS;
- malformed Video repeated twice with deterministic exit 4: PASS;
- physically truncated Video repeated twice with deterministic exit 4: PASS;
- mixed batch `valid MP4 → malformed AVI → valid MOV → truncated MKV → valid WebM` repeated twice in one Bridge process per attempt: aggregate exit 4 after all members; later valid members publish; invalid members publish nothing; no partials/orphan converter processes: PASS;
- managed tests: 260/260 PASS;
- static tests executed in managed qualification: 103/103 PASS;
- raw contract vectors: 5/5 PASS.

The supply-chain/static job at this pre-authority behavior point passed the new static gate but intentionally failed later on stale tracked generated authority. Side-branch continuity failure is expected by repository policy.

## Pre-authority workspace
The Windows job built two byte-identical workspace ZIPs: SHA-256 `5db1e06c084e58354aa2445f3645d1b53c760fc63f565fe7960224c2eec2467f`, 505672 bytes, 383 entries. Independent semantic archive verification then failed on the stale tracked package manifest. This is expected pre-authority evidence, not final workspace authority or verified delivery.

## Pre-curation generation marker
Run `33669979101` on head `4121f91da887dee5f873e42f6699904c7c0aa429` generated artifact `9862090305`, digest `sha256:7cb43be8a0a67cf5e0deb19c79827c9df786a26777ddd7f11bdc9cf95d5edba2`, with 381 generated package-manifest entries and 382 generated SHA entries. Repository curation in the next commit changes the workspace, so this artifact is explicitly NOT eligible for final synchronization.

## Open closure gates
1. ordinary CI on exact curated dev.20 head;
2. independent verification of fresh generated-authority artifact digest, CRC, exact four members and dev.20 alignment;
3. guarded exact-parent/self-deleting generated-authority synchronization;
4. branch generated-authority zero-diff and full managed workspace/delivery qualification;
5. non-force promotion only if frozen dev.19 main is unchanged and candidate is a strict descendant;
6. fresh exact-main three-job SUCCESS;
7. independent exact-main generated-authority/workspace/delivery verification;
8. final refs/documentation reconciliation.

Dev.20 is not frozen yet. Converty is not customer ship-ready; headed Windows 11, production FFmpeg redistribution, production signing/MSIX, production B2 identity/authentication, final security/fuzz/chaos and end-user gates remain open.
