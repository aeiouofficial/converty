# Implementation status — 0.1.0-dev.19

## Frozen baseline: dev.18 Image acceptance
- `0.1.0-dev.18` exact-main authority: `ef079f7e7923e399624067c4d54b9ce7577bf090`, tree `0af729f150897d170eac9f9aebfd5bc7d5d4083a`, run `33390111824`.
- Managed `99481546832`, supply-chain-static `99481546572`, continuity `99481546655`: SUCCESS.
- Workspace SHA-256 `4be8d5a2f503a8a885347b647bbd0aa0b61ce6d56b3ac39d9af7b11fb801628a`, 463162 bytes, 372 entries; generated-authority artifact `9757102986`; verified delivery `9757169067`.
- Image single-file acceptance: 8 source extensions × 3 fixed actions = 24/24 real packaged conversions; repeated malformed/truncated rejection; source/destination preservation; numbered publication; zero partial residue.
- Recursive baseline: 18/18 locked restore, 0 vulnerable-result packages, Release 0 warnings/errors, native/package/COM/product gates PASS, Audio 36-case + malformed/truncated + mixed-batch PASS, 254/254 managed, 95/95 static, 5/5 vectors.

## Dev.19 Image mixed-batch closure
- Added `build/image-batch-isolation-smoke.ps1`, a real Windows packaged product gate.
- Added `tests/static/test_dev19_image_batch_isolation.py` and `tests/Converty.Core.Tests/Execution/ImageBatchIsolationTests.cs`.
- Added CI step `Image mixed-batch failure isolation` after the existing Image single-file gate.
- Contract: one Bridge process receives valid PNG → malformed JPG → valid WebP → truncated BMP → valid JPEG; ordinary per-file failures do not suppress later valid members; aggregate failure is returned after the full batch.
- Transactional requirements: valid members publish numbered PNG outputs; invalid members publish nothing; source/pre-existing destination hashes remain unchanged; no `.converty-*.partial.*`; no test-package converter-worker/FFmpeg orphan processes.
- Batch is repeated twice and all Audio + Image single-file regression gates remain mandatory.
- No new production media subsystem or widened executable/argument surface is introduced.

## Dev.19 TDD evidence
- Static RED was established before the smoke existed; subsequent harness corrections were independently guarded.
- PowerShell harness corrections preserve safe `${attempt}` interpolation and named argument arrays instead of relying on fragile inline concatenation.
- Current branch qualification must still be completed after the latest metadata/CI commits. Do not label dev.19 frozen until generated-authority zero-diff and exact-main qualification succeed.

## Remaining implementation/release work
1. Finish dev.19 authority qualification and exact-main freeze.
2. Video foundation and fixed typed action/source/malformed/batch matrix, test-first, reusing the strict worker/provider architecture.
3. UX/settings: defaults, mixed-selection UX, progress/results, output/concurrency/isolation settings.
4. Plugin SDK manifest/API/signature/hash gate with worker-only sample provider.
5. Production FFmpeg/ffprobe exact-binary provenance, signature/hash/license/notices and redistribution approval.
6. Production signed-package B2 identity/authentication requalification.
7. Signed production MSIX clean Windows 11 install/update/uninstall.
8. Headed Windows 11 modern Explorer exact-build mouse-driven acceptance, screenshots and crash/hang/failure matrix.
9. Final fuzz/chaos/security/release audit and end-user acceptance.

Automated CI does not close headed UI, production signing, production FFmpeg redistribution or final end-user/security gates.
