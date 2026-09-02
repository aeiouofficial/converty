# Changelog

## 0.1.0-dev.19 — 2026-09-02
- Added focused Image multi-file failure-isolation coverage to prove later valid selections survive ordinary malformed/truncated members.
- Added a real packaged Windows Image mixed-batch acceptance smoke using one Bridge process for valid PNG → malformed JPG → valid WebP → truncated BMP → valid JPEG, repeated twice.
- The acceptance contract requires aggregate Bridge exit code 4 after the full selection, later valid outputs to publish with numbered no-overwrite semantics, invalid members to publish nothing, sources and pre-existing destinations to remain byte-identical, no `.converty-*.partial.*` residue and no test-package converter-worker/FFmpeg orphan processes.
- Added the dev.19 managed CI gate and static contract; existing Audio and Image single-file acceptance gates remain mandatory regression gates.
- Preserved the full granular historical changelog and machine-readable evidence vocabulary; generated SBOM/package/hash authority remains CI-derived.

## 0.1.0-dev.18 — 2026-08-31
- Added a dedicated packaged Windows Image acceptance component for the already-existing fixed Image product surface; no second image engine, raw FFmpeg argument surface or new executable path was introduced.
- Qualified all advertised Image source extensions (`png`, `jpg`, `jpeg`, `webp`, `bmp`, `gif`, `tif`, `tiff`) against `image.png`, `image.jpeg`, and `image.webp`: 24 real Bridge→Strict Worker/provider→FFmpeg conversions.
- Every successful conversion verifies expected ffprobe codec and 64×48 dimensions, preserves the source and a pre-existing destination byte-for-byte, uses numbered no-overwrite publication, and leaves no partial output.
- Added repeated malformed and physically truncated Image rejection; both return deterministic Bridge exit code 4, publish nothing, preserve existing files and leave zero partial residue.
- RED `7388aec6ffb673e0101b09106d646d417f77a7b3` / run `33349908668`: 92 existing static tests PASS and exactly 3 new dev.18 assertions FAIL because the Image acceptance component did not exist.
- Added the Image matrix at `0841395904960945a1988dcedb8b6ccf352a57e0` and ordinary CI wiring at behavior head `6075aa3973b75e170cb5f9b812a8ca3b9b71f528`.
- GREEN behavior run `33350141373`: 24/24 Image conversions plus both repeated negative cases PASS; existing Audio product/matrix/batch gates PASS; 254/254 managed, 95/95 static, 5/5 vectors, dependency audit 0 vulnerable-result packages, Release build 0 warnings/errors. Pre-authority deterministic workspace `50324f542c263cb7b23f43a6e9c87b68ec773f08c1a7b828dc41da7e369cdda2`, 459142 bytes, 369 entries, then expected stale tracked-authority failure.

## 0.1.0-dev.17 — 2026-08-31
- Closed Audio mixed-valid/invalid multi-file failure isolation without adding a second batch subsystem or changing the native one-Bridge-per-selection topology.
- `ConversionBatchRunner` now catches ordinary per-file `ConversionFailedException`, always cleans that file's private staging, continues later selected files, and rethrows the first media conversion failure only after the batch has been attempted. Cancellation, contract/programmer faults and global infrastructure errors remain fail-fast.
- Added a real packaged Windows batch smoke using one Bridge process for valid WAV → malformed WAV → valid FLAC → truncated FLAC → valid WAV. The five-file selection is executed twice and must return aggregate exit code 4 while all three valid files publish numbered MP3 outputs and both bad inputs publish nothing.
- The gate proves source and pre-existing destination hashes are preserved for every item, successful outputs are ffprobe-verified, collision numbering advances from `(1)` to `(2)`, and no `.converty-*.partial.*` files remain.
- Preserved RED evidence at `053e086fab6fcea1da83ab109e1a986379e0b82a` / run `33346968020`: existing product gates green, 254 managed tests with exactly one new failure because only two of three worker calls occurred. Added the independent static product-gate RED at `285585107795045a41d85199c22fd971b1ed6191` / run `33346976504`.
- Fixed two acceptance-harness defects test-first: unsafe `$attempt:` interpolation (`fe2886897dc03eec3942c046973e04558acaf860` → `355e5fdc47ad6d7090678a8b32461fb177a0db63`) and PowerShell inline array-concatenation binding (`6fd23d346ddf5b2acecc34fef5974b559df31289` → `5829c868c5d192c70f21ea0da9337250a8d9c961`).
- GREEN behavior run `33347652162`: real mixed batch PASS twice, 254/254 managed, 91/91 static, 5/5 vectors, Release 0 warnings/errors; pre-authority deterministic workspace `4af24ae6f866c6389a3010642504aea13952ecb17d717c9974d05161fb8f6ba0`, 447903 bytes, 364 entries, then expected stale generated-authority failure.

## 0.1.0-dev.16 — 2026-08-31
- Added a dedicated Windows Audio input acceptance matrix covering WAV, FLAC, MP3, M4A/AAC, Ogg/Vorbis and Opus sources against all six fixed Audio actions (36 real product-path conversions).
- Every matrix conversion enters through packaged `Converty.Bridge.exe` → Strict `Converty.EngineWorker`/typed FFmpeg provider, uses Unicode/metacharacter paths, preserves source and pre-existing destination bytes, uses numbered no-overwrite publication, leaves no partial output and is ffprobe codec-verified.
- Added repeated malformed-WAV and truncated-FLAC negative acceptance. The first RED integration run exposed a real failure-lifecycle hang: Bridge reached its error handler but synchronous `MessageBoxW` blocked noninteractive callers.
- Added explicit automation-only `CONVERTY_BRIDGE_NONINTERACTIVE=1` error reporting. Explorer does not set the variable and retains the normal modal error dialog; automation receives the same bounded error on stderr and the normal nonzero Bridge exit.
- Both malformed and truncated cases now reject deterministically with exit code 4 across repeated attempts, without source mutation, destination overwrite, numbered publication or partial-file residue.
- Preserved TDD RED evidence: `251b1c54901d212e03961e6bed947bc828df6bc7` / run `33339926916` (3 new failures, 81 existing static tests green), and lifecycle RED `673f92e43738554db364a8db5ea44a00cdd903b7` / run `33340234688` (1 new failure, 84 existing tests green).
- GREEN behavior head `061ad75600fee6fd4b34e4a24bd8d571ac17ce90` / run `33340338502`: 36 valid conversions + 2 repeated negative cases PASS, 253/253 managed, 85/85 static, 5/5 vectors, Release 0 warnings/errors; pre-authority deterministic workspace `27b6f96ea8c42afee8de2d67a2ea9d43f48607ab13a4b124cbab6acd3b55a643`, 436519 bytes, 358 entries, then expected stale generated-authority failure.

## 0.1.0-dev.15 — 2026-08-30
- Expanded the fixed typed Audio action matrix with `audio.m4a.aac`, `audio.opus`, and `audio.ogg.vorbis`, preserving MP3, FLAC and WAV.
- Mirrored all fixed Audio preset IDs in the native Explorer submenu with stable canonical GUIDs and qualified MP3/M4A-AAC/Opus/Ogg-Vorbis through the packaged product path.

## 0.1.0-dev.14 — 2026-08-30
- Added replay/disconnect/reconnect acceptance for authenticated one-shot Host IPC and idempotent admission by `requestId`.

## 0.1.0-dev.13 — 2026-08-30
- Added typed one-shot `status` and `cancel` requests/responses on the authenticated Host named pipe.

## 0.1.0-dev.12 — 2026-08-29
- Eliminated the historical scheduler-dependent containment test assumption without changing production output limits or containment.

## 0.1.0-dev.11 — 2026-08-29
- Closed development B2 connected-server identity/authentication without rerouting normal conversion through Host.

## 0.1.0-dev.10 — 2026-08-27
- Moved conversion execution into fixed app-local `Converty.EngineWorker` and `Converty.Provider.FFmpeg` with private per-job staging and strict containment.

## 0.1.0-dev.9 — 2026-08-26
- Delivered the first automated functional Windows product path: packaged native Explorer command → fixed Bridge → typed preset → fixed app-local FFmpeg → same-folder numbered output.

Earlier foundation history remains available in repository history and prior handovers.
