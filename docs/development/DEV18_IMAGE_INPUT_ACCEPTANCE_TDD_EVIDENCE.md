# Dev.18 Image input acceptance — TDD evidence

## Scope
Qualify the already-existing fixed Image product actions through the real packaged product path. No product-code change is acceptable merely to manufacture a dev.18 diff; production changes are required only if the acceptance matrix exposes a defect.

## RED
Commit `7388aec6ffb673e0101b09106d646d417f77a7b3`, run `33349908668`, static job `99361086262`.

Result: 92 existing static tests passed and exactly three new dev.18 tests failed. All three failed because `build/image-input-acceptance-smoke.ps1` did not exist. Existing restore/build/native/package/Audio behavior remained green up to the new contract boundary.

## Implementation
Commit `0841395904960945a1988dcedb8b6ccf352a57e0` added the dedicated Image acceptance component. Commit `6075aa3973b75e170cb5f9b812a8ca3b9b71f528` wired it into ordinary Windows CI.

No production source changed. This is intentional: the existing typed Image registry/native Explorer surface and strict worker/provider execution already satisfied the contract.

## GREEN behavior evidence
Run `33350141373`; managed job `99361743241`; static job `99361743276`.

- Windows Server 2025 / `windows-2025-vs2026`; .NET SDK 10.0.400.
- 18/18 locked restore PASS.
- Dependency audit PASS: 18 projects, 18 frameworks, 0 vulnerable-result packages.
- Release build PASS: 0 warnings, 0 errors.
- Native Explorer, development package, direct/registered COM and baseline product smoke PASS.
- Audio regression: 36 source/action conversions, malformed/truncated rejection and mixed-valid/invalid batch isolation PASS.
- Image sources: PNG, JPG, JPEG, WebP, BMP, GIF, TIF, TIFF.
- Image actions: PNG, JPEG, WebP.
- 24/24 Image product conversions PASS; expected codecs `png`, `mjpeg`, `webp`; dimensions 64×48.
- Malformed Image rejection repeated with deterministic Bridge exit 4.
- Truncated Image rejection repeated with deterministic Bridge exit 4.
- Source/pre-existing destination preservation PASS; numbered no-overwrite publication PASS; zero partial residue PASS.
- Managed tests 254/254 PASS, 0 skipped.
- Static tests 95/95 PASS.
- Contract vectors 5/5 PASS.
- Pre-authority deterministic workspace A/B: SHA-256 `50324f542c263cb7b23f43a6e9c87b68ec773f08c1a7b828dc41da7e369cdda2`, 459142 bytes, 369 entries. The subsequent integrity assertion fails only because tracked generated authority is intentionally still dev.17 at the behavior stage.

## Finality
Behavior-green is not frozen authority. Final dev.18 requires version-aligned generated authority from exact CI output, guarded sync, zero-diff branch qualification, unchanged-main non-force promotion and a fresh exact-main three-job green run with deterministic verified delivery.
