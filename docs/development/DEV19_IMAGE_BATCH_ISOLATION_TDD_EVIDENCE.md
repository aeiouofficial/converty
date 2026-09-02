# Dev.19 Image mixed-batch isolation — TDD evidence

## Scope
Dev.19 closes Image multi-file failure isolation before Video expansion. It verifies one same-family Explorer/Bridge selection can contain valid and invalid Images without an invalid member suppressing later valid members.

## RED
The first dev.19 contract commit adds `tests/static/test_dev19_image_batch_isolation.py` before the acceptance smoke exists. Expected RED is limited to the new smoke/wiring assertions; prior dev.18 static coverage remains green.

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

## Implementation
The existing dev.17 `ConversionBatchRunner` per-file isolation behavior is reused. Dev.19 adds Image-specific real-product acceptance and core-runner coverage; it does not create a second batch subsystem or widen the executable/argument surface.

## Fixtures
Disposable media and logs remain below excluded `artifacts/image-batch-isolation-smoke`. FFmpeg used to generate fixtures is development qualification tooling only. Conversion under test remains packaged Bridge → Strict Worker/provider → fixed app-local FFmpeg.

## Regression boundary
The existing 24-case Image source/action matrix and Audio source/action, malformed/truncated and mixed-batch gates must remain green. Video work must not start until this tranche has synchronized generated authority and passed exact-main qualification.

## Known environment limitations
Headed Windows 11 modern Explorer UI acceptance, exact-build screenshots, headed crash/hang/failure testing, production-signed package identity, and production FFmpeg redistribution approval require later dedicated gates and are not implied by this automated acceptance.
