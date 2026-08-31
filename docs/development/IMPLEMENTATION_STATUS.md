# Implementation status — 0.1.0-dev.18

## Dev.18 fixed Image input/action acceptance — 2026-08-31
- Added `build/image-input-acceptance-smoke.ps1` as a dedicated Windows product acceptance component. All fixture media/output is recreated below excluded `artifacts/image-input-acceptance-smoke`; no generated media/log debris is committed.
- Exercised all advertised Image source extensions (`png`, `jpg`, `jpeg`, `webp`, `bmp`, `gif`, `tif`, `tiff`) against all three existing fixed Image actions (`image.png`, `image.jpeg`, `image.webp`) for 24 real packaged Bridge→Strict Worker/provider→FFmpeg conversions.
- Every success verifies the expected codec (`png`, `mjpeg`, `webp`) and 64×48 dimensions, preserves source and a pre-existing destination byte-for-byte, publishes numbered output only, and leaves zero `.converty-*.partial.*` residue.
- Repeated malformed and physically truncated Image inputs reject deterministically with Bridge exit code 4 and do not publish output.
- No production-code change was necessary: the existing typed registry, native Explorer commands, Bridge, Strict Worker and FFmpeg provider already satisfy the tested Image path. Dev.18 therefore adds evidence and regression coverage rather than duplicating media execution logic.
- Existing Audio product, six-by-six source/action, malformed/truncated and mixed-batch gates remain green.

## TDD evidence
- RED `7388aec6ffb673e0101b09106d646d417f77a7b3`, run `33349908668`, static job `99361086262`: 92 existing static tests PASS; exactly 3 new dev.18 assertions FAIL because the Image acceptance smoke was absent.
- Acceptance implementation `0841395904960945a1988dcedb8b6ccf352a57e0` added the dedicated smoke.
- CI wiring/behavior head `6075aa3973b75e170cb5f9b812a8ca3b9b71f528`, run `33350141373`: new Image gate and all prior product behavior PASS; generated-authority/workspace freshness remains intentionally stale until version authority sync.

## Observed behavior qualification
- Windows Server 2025 / `windows-2025-vs2026` / .NET SDK 10.0.400.
- 18/18 locked restore; dependency audit PASS across 18 projects/18 frameworks with 0 vulnerable-result packages.
- Release build PASS with 0 warnings / 0 errors.
- Native Explorer, unsigned development MakeAppx package, direct staged class-factory Invoke, loose-package COM activation/Invoke and existing product conversion smoke PASS.
- Audio source/action matrix 36/36 PASS; malformed/truncated Audio rejection PASS; mixed Audio batch PASS twice.
- Image matrix 24/24 PASS; repeated malformed/truncated Image rejection PASS with deterministic exit 4; source/destination/no-partial invariants PASS.
- Managed tests 254/254 PASS, 0 skipped; static tests 95/95 PASS; contract vectors 5/5 PASS.
- Pre-authority deterministic A/B workspace SHA-256 `50324f542c263cb7b23f43a6e9c87b68ec773f08c1a7b828dc41da7e369cdda2`, 459142 bytes, 369 entries; verification then stops only because tracked generated authority still describes dev.17.

## Prior frozen authority
Dev.17 exact main `8b2756910b58b678745e6fda89866ed3bf545474`, tree `c8066d768aeaeb1b2541cc6ca24217ff882e6048`, exact-main run `33349604621`; jobs managed `99360233371`, continuity `99360233503`, static `99360233566`. Workspace SHA-256 `46949a7f0caa4675ce1573987eedbdca609a0ef4f2d331385fb5d8fe401d9eea`, 454470 bytes, 367 entries. Generated artifact `9743140776`; verified-delivery artifact `9743178417`.

## Authority rule
Dev.18 is frozen only after version-aligned generated authority is synchronized from one exact CI artifact, branch zero-diff qualification passes, `main` is non-force fast-forwarded from unchanged dev.17, and ordinary CI on exact current `main` has continuity + managed + supply-chain-static all SUCCESS with deterministic workspace verification and verified delivery upload.

## Remaining shipping gates
Headed Windows 11 UI/screenshots and Explorer crash/hang/failure matrix; Image multi-file/mixed-input closure; production signed-package B2 requalification; production FFmpeg redistribution approval; signed production MSIX/clean-VM lifecycle; final security/fuzz/chaos/release/end-user acceptance.
