# Dev.19 Image mixed-batch isolation — TDD evidence

## Scope
Dev.19 closes Image multi-file failure isolation before Video expansion. It verifies one same-family Explorer/Bridge selection can contain valid and invalid Images without an invalid member suppressing later valid members.

## Original RED and implementation
The dev.19 static contract was added before the packaged acceptance smoke existed. The implementation then added:
- `build/image-batch-isolation-smoke.ps1`;
- `tests/static/test_dev19_image_batch_isolation.py`;
- `tests/Converty.Core.Tests/Execution/ImageBatchIsolationTests.cs`;
- managed CI wiring after the existing 24-case Image single-file gate.

Historical implementation anchors retained in machine-readable evidence:
- static contract `090065ecb70ec0a27557938a2c0f80b08e0dd950`;
- Image smoke `3a494427a9ef794766fd326273f22ac715610107`;
- core test `405514a5eb29b53765cfc14d42705e749b43ad91`;
- CI wiring `051d0cde725ef33350e150d03a932bbe60420f61`;
- static-contract correction `3cd15f4dfad4742b25fa91ece36d651291dd7f5c`.

Earlier implementation also corrected PowerShell acceptance-harness defects around safe `${attempt}` interpolation and argument-array construction. Those guards remain required.

## Contract
The real Windows gate must:
- invoke one packaged `Converty.Bridge.exe` with `--preset image.png` and multiple literal `ArgumentList` source paths;
- place valid Images before and after malformed/truncated members;
- continue after ordinary per-file conversion failures;
- report aggregate Bridge exit code `4` only after the full selection is attempted;
- publish later valid outputs with numbered/no-overwrite semantics;
- publish no output for malformed/truncated members;
- preserve every source and every pre-existing destination byte-for-byte;
- leave no `.converty-*.partial.*` files;
- leave no converter-worker/FFmpeg processes whose command line belongs to the test package;
- repeat the complete batch twice;
- preserve all Audio and single-file Image regression gates.

## Qualification defect 1 — same-extension smoke source alias
Run `33593266879` exposed `Source preserved invariant failed for valid-before`. Root cause was in the acceptance harness, not the product: for the valid `.png` member, `ChangeExtension(source, '.png')` resolves to the source path itself, and collision setup wrote sentinel bytes to that path after hashing the source.

TDD correction sequence on isolated branch `fix/dev19-image-source-destination-alias`:
- `ce54e0f08b5dfd9ba1b4d6cba6132c64f7ce006f` — initial reproducer;
- `a988204b058ada86f8909cf94e1d9f2b6e69cf39` — corrected RED invariant after comparison with the qualified single-Image gate; run `33595474461`, static `1 failed, 98 passed`;
- `633fc39b5df8062496914cc641b7001adea805ee` — GREEN harness fix: seed a pre-existing target only when the resolved target does not equal the selected source.

Same-extension Image conversion remains valid. The fix mirrors the existing qualified single-file Image acceptance behavior and changes no production conversion code.

## Qualification defect 2 — core-test fixture deleted its source
The next managed run reached the core suite and failed because the test fixture itself set its artificial `firstExisting` collision path equal to `first.png` and deleted it before calling `RunAsync`. `ConversionBatchRunner.ValidateInputPath` correctly raised `FileNotFoundException`.

The reference Audio batch test and `OutputPathResolver` establish the correct same-extension behavior: `first.png` itself occupies the base target path, so successful publication naturally resolves to `first (1).png`.

GREEN correction:
- `8a64d0a12ccf47df5df364a1c6c545f876d57d29` — preserves `first.png`, removes the invalid artificial delete, and retains all source/existing-destination/no-overwrite assertions.

## Behavior GREEN anchor
Run `33596229372` on exact SHA `8a64d0a12ccf47df5df364a1c6c545f876d57d29` established:
- dependency audit: 18 projects / 18 frameworks / 0 vulnerable-result packages;
- Release build: 0 warnings / 0 errors;
- native Explorer/package/COM/Bridge→FFmpeg gates: PASS;
- Audio source/action: 36 successful conversions plus repeated malformed/truncated rejection: PASS;
- Audio mixed-batch failure isolation: PASS twice;
- Image source/action: 24 successful conversions plus repeated malformed/truncated rejection: PASS;
- Image mixed-batch failure isolation: PASS twice;
- managed tests: 255/255 PASS;
- static tests: 99/99 PASS;
- raw contract vectors: 5/5 PASS.

The deterministic workspace was built twice byte-identically at SHA-256 `1c8d197941a616a25bcc4bab59550037a221309f51bc937a1ba7daa9b34bf97d`, 475353 bytes, 378 entries. The subsequent semantic archive assertion correctly stopped because tracked `machine-readable/package_manifest.json` still represented dev.18. Delivery therefore was not produced on this pre-authority run. Final generated-authority synchronization and zero-diff qualification remain required.

## Architecture and fixtures
The existing dev.17 `ConversionBatchRunner` per-file isolation behavior is reused. Dev.19 does not create a second batch subsystem or widen the executable/argument surface. Disposable media and logs remain below excluded `artifacts/`. Development FFmpeg generates/validates fixtures only; conversion under test remains packaged Bridge → Strict Worker/provider → fixed app-local FFmpeg.

## Regression boundary
The existing 24-case Image source/action matrix and Audio source/action, malformed/truncated, and mixed-batch gates must remain green. Video work must not start until dev.19 has synchronized generated authority, passed branch zero-diff, and passed exact-main qualification.

## Known environment limitations
Headed Windows 11 modern Explorer UI acceptance, exact-build screenshots, headed crash/hang/failure testing, production-signed package identity, production FFmpeg redistribution approval, signed MSIX lifecycle, and final security/fuzz/chaos/end-user acceptance require later dedicated evidence and are not implied by this automated acceptance.