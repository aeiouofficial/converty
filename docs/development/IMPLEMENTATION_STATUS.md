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

## Dev.19 qualification corrections
Two independent qualification-fixture defects were found and fixed without changing production behavior:
1. The Windows mixed-batch harness seeded a `.png` collision target over the `.png` source itself. Corrected RED `a988204b058ada86f8909cf94e1d9f2b6e69cf39` / run `33595474461` proved the source/target guard requirement; GREEN `633fc39b5df8062496914cc641b7001adea805ee` skips collision seeding only when target aliases source.
2. The core Image batch test deleted `first.png` before `RunAsync`. GREEN `8a64d0a12ccf47df5df364a1c6c545f876d57d29` preserves the source and exercises the real `OutputPathResolver` same-extension numbering path.

## Current pre-authority behavior qualification
Exact behavior SHA `8a64d0a12ccf47df5df364a1c6c545f876d57d29`, run `33596229372`:
- locked restore PASS;
- dependency audit PASS: 18 projects / 18 frameworks / 0 vulnerable-result packages;
- Release build PASS: 0 warnings / 0 errors;
- native Explorer, development package, COM registration, and packaged Bridge→FFmpeg PASS;
- Audio 36-case source/action + repeated malformed/truncated + mixed-batch PASS;
- Image 24-case source/action + repeated malformed/truncated + mixed-batch PASS;
- 255/255 managed tests PASS;
- 99/99 static tests PASS;
- 5/5 contract vectors PASS.

The workspace ZIP was produced twice byte-identically: SHA-256 `1c8d197941a616a25bcc4bab59550037a221309f51bc937a1ba7daa9b34bf97d`, 475353 bytes, 378 entries. Semantic archive verification then correctly rejected the stale tracked dev.18 `package_manifest.json`; delivery staging/upload was skipped. This is an expected pre-authority blocker. It is not final workspace or delivery evidence.

## Required dev.19 freeze sequence
1. Commit this metadata curation onto the exact behavior lineage.
2. Run ordinary CI and obtain the exact generated-authority artifact for the curated workspace.
3. Independently verify artifact SHA-256, ZIP CRC, exact four-member set, and `0.1.0-dev.19` version alignment.
4. Perform guarded exact-parent, self-deleting generated-authority synchronization.
5. Require canonical dev.19 generated-authority zero-diff and complete Windows managed qualification including deterministic workspace and delivery.
6. Re-read live `main`; fast-forward non-force only if its expected base is unchanged.
7. Require fresh exact-main continuity + supply-chain/static + managed SUCCESS.
8. Independently verify final deterministic workspace and delivery artifacts before declaring dev.19 frozen.

## Remaining implementation/release work
After dev.19 freeze: Video foundation and fixed typed action/source/malformed/batch matrix, then UX/settings, plugin SDK, production FFmpeg approval, production signed-package B2 requalification, signed MSIX lifecycle, headed Windows 11 Explorer acceptance, and final fuzz/chaos/security/release/end-user acceptance.

Automated CI does not close headed UI, production signing, production FFmpeg redistribution, signed MSIX lifecycle, or final end-user/security gates.