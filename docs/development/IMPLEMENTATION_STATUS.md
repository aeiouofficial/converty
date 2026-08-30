# Implementation status — 0.1.0-dev.16

## Dev.16 Audio source/malformed-input acceptance — 2026-08-31
- Added `build/audio-input-acceptance-smoke.ps1` as a dedicated Windows acceptance component. Disposable fixtures/results are recreated under excluded `artifacts/audio-input-acceptance-smoke`; no generated media or logs are committed.
- Qualified six representative source formats (WAV, FLAC, MP3, M4A/AAC, Ogg/Vorbis, Opus) against all six fixed Audio actions, for 36 real packaged Bridge→Strict Worker/provider→FFmpeg conversions.
- Every success proves Unicode/metacharacter path safety, byte-identical source preservation, byte-identical pre-existing destination preservation, numbered `(1)` publication, zero partial outputs and expected ffprobe codec.
- Added malformed WAV and physically truncated FLAC cases, each exercised twice. Both now return deterministic Bridge exit code 4, preserve all existing files, publish no output and leave no partials.
- The first negative integration exposed synchronous `MessageBoxW` blocking noninteractive callers after a legitimate conversion failure. `BridgeErrorDialog` now supports explicit `CONVERTY_BRIDGE_NONINTERACTIVE=1`: automation writes the same bounded error to stderr and returns; Explorer keeps modal UI by default.
- No codec/parser execution moved into Bridge/Host; strict worker/provider containment and fixed executable/preset boundaries are unchanged.

## TDD evidence
- RED #1 `251b1c54901d212e03961e6bed947bc828df6bc7`, run `33339926916`: 81 existing static tests PASS, exactly 3 new dev.16 contract assertions FAIL because the acceptance component/CI wiring did not exist.
- Initial matrix implementation `86c6352ce12f2c492d4065b4c15a78223d1d2aab`, run `33340046236`: 84/84 static PASS; all 36 valid Windows conversions PASS; malformed input exposed a real >30s modal-blocking failure lifecycle.
- RED #2 `673f92e43738554db364a8db5ea44a00cdd903b7`, run `33340234688`: 84 existing static tests PASS, exactly 1 new assertion FAIL for the missing explicit noninteractive reporter.
- Root-cause fix `066925fa5c90f4bcaf581590c5193a44b64cb4e9`; acceptance caller wiring `061ad75600fee6fd4b34e4a24bd8d571ac17ce90`.
- GREEN behavior run `33340338502`: managed `99334697033`, static `99334696969`; all behavior gates green before the expected generated-authority/workspace freshness boundary.

## Observed behavior qualification
- Windows Server 2025 / `windows-2025-vs2026` / .NET SDK 10.0.400.
- 18/18 locked restore; dependency audit PASS across 18 projects/18 frameworks with 0 vulnerable-result packages.
- Release build PASS with 0 warnings / 0 errors.
- Native Explorer, unsigned development MakeAppx package, direct staged class-factory Invoke and loose-package COM activation/Invoke PASS.
- Existing MP3/M4A-AAC/Opus/Ogg-Vorbis product smoke PASS.
- Audio source matrix PASS: 36/36; malformed and truncated repeated negatives PASS with deterministic exit 4; preservation/no-partial invariants PASS.
- Managed tests 253/253 PASS, 0 skipped; Python static tests 85/85 PASS; contract vectors 5/5 PASS.
- Pre-authority deterministic A/B workspace SHA-256 `27b6f96ea8c42afee8de2d67a2ea9d43f48607ab13a4b124cbab6acd3b55a643`, 436519 bytes, 358 entries; integrity then failed only because tracked generated authority still described dev.15.

## Prior frozen authority
Dev.15 exact main `dc46bd4dd25fe672f1695a0895cdb06152a743a7`, tree `b170701a80377280065ce758f56076aa8eb044f0`, run `33339019327`; workspace SHA-256 `8f518bf3b70ca0e51f13d554e9a6966bb17f2897dcf249624cd48fe43ee69c52`, 431528 bytes, 356 entries; generated artifact `9739931738`, verified-delivery artifact `9739962466`.

## Authority rule
Dev.16 is frozen only after version-aligned generated authority is synchronized from one exact CI artifact, branch qualification reaches generated-authority zero-diff with managed/static behavior green, `main` is non-force fast-forwarded, and ordinary CI on exact current `main` has continuity + managed + supply-chain-static all SUCCESS with deterministic workspace verification and verified delivery upload.

## Remaining shipping gates
Headed Windows 11 UI/screenshots and Explorer crash/hang/failure matrix; final Audio multi-file/mixed-input isolation; production signed-package B2 requalification; production FFmpeg redistribution approval; signed production MSIX/clean-VM lifecycle; final security/fuzz/chaos/release/end-user acceptance.
