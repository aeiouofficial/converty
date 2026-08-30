# Dev.15 Audio preset matrix implementation plan — 2026-08-30

1. Add failing managed tests for exact Audio preset IDs, order and fixed encoder arguments.
2. Add failing static gates requiring the native submenu and real product-smoke matrix to include the new actions.
3. Implement the new fixed preset definitions in `ProductPresetRegistry`.
4. Expand the product conversion smoke to MP3, M4A/AAC, Opus and Ogg Vorbis while preserving transactional path assertions.
5. Add the same stable typed IDs to the native Explorer submenu.
6. Correct only test assertions that encode superseded implementation details while preserving their safety properties.
7. Require Release/native/package/COM/product/managed/static/vector behavior GREEN before versioning.
8. Advance version/evidence atomically, regenerate generated authority on CI, synchronize only the exact runner-generated authority files through a guarded self-deleting workflow, and qualify zero-diff.
9. Fast-forward `main` only after branch qualification, then require an ordinary exact-main run with all three jobs successful and deterministic verified delivery before declaring dev.15 frozen.
