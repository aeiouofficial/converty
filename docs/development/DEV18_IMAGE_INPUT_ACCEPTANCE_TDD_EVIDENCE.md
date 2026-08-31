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
- Pre-authority deterministic workspace A/B: SHA-256 `50324f542c263cb7b23f43a6e9c87b68ec773f08c1a7b828dc41da7e369cdda2`, 459142 bytes, 369 entries. The subsequent integrity assertion failed only because tracked generated authority was intentionally still dev.17 at the behavior stage.

## Frozen authority qualification
Dev.18 generated authority was synchronized through the guarded artifact path and qualified on exact `main`.

Qualification anchor: commit `ef079f7e7923e399624067c4d54b9ce7577bf090`, tree `0af729f150897d170eac9f9aebfd5bc7d5d4083a`, exact-main run `33390111824`.

- Managed job `99481546832`: SUCCESS.
- Supply-chain-static job `99481546572`: SUCCESS, including tracked generated-authority zero-diff.
- Main-authority-continuity job `99481546655`: SUCCESS.
- Exact-main deterministic workspace SHA-256 `4be8d5a2f503a8a885347b647bbd0aa0b61ce6d56b3ac39d9af7b11fb801628a`, 463162 bytes, 372 entries, 370 package-manifest entries, 371 SHA-manifest entries; CRC/deterministic double-build/exclusion policy PASS.
- Generated-authority artifact `9757102986`, digest `sha256:a8eb7d4a9d70044d2d6125404de7de686de41a97fd7dada1b2cf814d6f2c50d5`.
- Verified-delivery artifact `9757169067`, digest `sha256:97931ae9cbee196fb3cc6c3b0c2d792055ad04365d6dc6609e659301edaa0f92`.

Dev.18 is therefore frozen as an evidence-backed product tranche. Metadata-only curation after this anchor must itself re-run ordinary CI and generated-authority zero-diff before promotion, but it does not reopen or alter the proven Image behavior.
