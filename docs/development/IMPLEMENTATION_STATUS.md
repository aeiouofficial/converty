# Implementation status — 0.1.0-dev.18

## Frozen dev.18 Image input/action acceptance — 2026-08-31
- `0.1.0-dev.18` is frozen as an evidence-backed product tranche. The qualification anchor is exact-main commit `ef079f7e7923e399624067c4d54b9ce7577bf090`, tree `0af729f150897d170eac9f9aebfd5bc7d5d4083a`, run `33390111824`.
- Exact-main jobs all succeeded: managed `99481546832`, supply-chain-static `99481546572`, main-authority-continuity `99481546655`.
- The tracked generated-authority zero-diff gate passed on exact `main`.
- Deterministic verified workspace: `Converty_0.1.0-dev.18_full_workspace.zip`, SHA-256 `4be8d5a2f503a8a885347b647bbd0aa0b61ce6d56b3ac39d9af7b11fb801628a`, 463162 bytes, 372 entries, 370 package-manifest entries, 371 SHA-manifest entries; CRC/double-build/exclusion policy PASS.
- Exact-main generated-authority artifact `9757102986`, digest `sha256:a8eb7d4a9d70044d2d6125404de7de686de41a97fd7dada1b2cf814d6f2c50d5`.
- Exact-main verified-delivery artifact `9757169067`, digest `sha256:97931ae9cbee196fb3cc6c3b0c2d792055ad04365d6dc6609e659301edaa0f92`.

## Dev.18 product acceptance
- Added `build/image-input-acceptance-smoke.ps1` as a dedicated Windows product acceptance component. All fixture media/output is recreated below excluded `artifacts/image-input-acceptance-smoke`; no generated media/log debris is committed.
- Exercised all advertised Image source extensions (`png`, `jpg`, `jpeg`, `webp`, `bmp`, `gif`, `tif`, `tiff`) against all three existing fixed Image actions (`image.png`, `image.jpeg`, `image.webp`) for 24 real packaged Bridge→Strict Worker/provider→FFmpeg conversions.
- Every success verifies the expected codec (`png`, `mjpeg`, `webp`) and 64×48 dimensions, preserves source and a pre-existing destination byte-for-byte, publishes numbered output only, and leaves zero `.converty-*.partial.*` residue.
- Repeated malformed and physically truncated Image inputs reject deterministically with Bridge exit code 4 and do not publish output.
- No production-code change was necessary: the existing typed registry, native Explorer commands, Bridge, Strict Worker and FFmpeg provider already satisfy the tested Image path. Dev.18 therefore adds evidence and regression coverage rather than duplicating media execution logic.
- Existing Audio product, six-by-six source/action, malformed/truncated and mixed-batch gates remain green.

## TDD evidence
- RED `7388aec6ffb673e0101b09106d646d417f77a7b3`, run `33349908668`, static job `99361086262`: 92 existing static tests PASS; exactly 3 new dev.18 assertions FAIL because the Image acceptance smoke was absent.
- Acceptance implementation `0841395904960945a1988dcedb8b6ccf352a57e0` added the dedicated smoke.
- CI wiring/behavior head `6075aa3973b75e170cb5f9b812a8ca3b9b71f528`, run `33350141373`: new Image gate and all prior product behavior PASS.
- Final generated-authority qualification and exact-main freeze completed at the anchor above.

## Observed recursive qualification
- Windows Server 2025 / `windows-2025-vs2026` / .NET SDK 10.0.400.
- 18/18 locked restore; dependency audit PASS across 18 projects/18 frameworks with 0 vulnerable-result packages.
- Release build PASS with 0 warnings / 0 errors.
- Native Explorer, unsigned development MakeAppx package, direct staged class-factory Invoke, loose-package COM activation/Invoke and product conversion smoke PASS.
- Audio source/action matrix 36/36 PASS; malformed/truncated Audio rejection PASS; mixed Audio batch PASS twice.
- Image matrix 24/24 PASS; repeated malformed/truncated Image rejection PASS with deterministic exit 4; source/destination/no-partial invariants PASS.
- Managed tests 254/254 PASS, 0 skipped; static tests 95/95 PASS; contract vectors 5/5 PASS.

## Next product tranche
`0.1.0-dev.19`: Image multi-file/mixed-valid-invalid failure-isolation acceptance and Image matrix closure. Exercise valid Images before and after malformed/truncated inputs in one same-family selection; later valid files must still publish, aggregate failure must be reported after the batch, source/pre-existing destinations must remain unchanged, numbered collision publication must remain deterministic, and no partial output/orphan worker may remain. Do not begin Video expansion until this Image closure tranche is evidence-backed and frozen.

## Remaining shipping gates
Headed Windows 11 UI/screenshots and Explorer crash/hang/failure matrix; Image mixed-batch closure; production signed-package B2 requalification; production FFmpeg redistribution approval; signed production MSIX/clean-VM lifecycle; final security/fuzz/chaos/release/end-user acceptance.
