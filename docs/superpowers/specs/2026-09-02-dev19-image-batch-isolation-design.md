# Dev.19 Image mixed-batch isolation design

## Goal
Prove that a same-family Image multi-selection remains failure-isolated: ordinary per-file conversion failure does not suppress later valid selections, while aggregate failure is still reported after the batch.

## Existing architecture
Keep `IExplorerCommand → Bridge → Strict EngineWorker/provider → app-local FFmpeg → private staging → transactional numbered publication`. Do not introduce a new IPC contract, media parser in Host/Bridge, raw FFmpeg arguments, PATH lookup, or compatibility fallback.

## Behavior contract
Use one Bridge process per batch. Process selected files sequentially. For each ordinary conversion failure, clean that file's staging and continue. Successful files publish transactionally. After all selected files have been attempted, report aggregate failure when any member failed. Cancellation and non-per-file contract/infrastructure failures remain fail-fast according to existing behavior.

## Acceptance matrix
Selection: valid PNG, malformed JPG, valid WebP, truncated BMP, valid JPEG. Fixed action: `image.png`. Repeat twice. Verify exit `4`, later valid outputs, no invalid outputs, source hashes, pre-existing destination hashes, numbered publication, no partials, and no test-package converter processes remaining.

## Test-first
Add static contract before the smoke. Add a focused core-runner test using a sequenced worker. Add the real Windows packaged smoke and CI wiring only after RED evidence is recorded.

## Completion
Do not mark dev.19 frozen until versioned metadata, generated authority, branch zero-diff qualification, non-force main promotion, and fresh exact-main three-job qualification all succeed.
