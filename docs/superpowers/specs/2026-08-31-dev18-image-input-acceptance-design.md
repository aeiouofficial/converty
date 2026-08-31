# Dev.18 Image input acceptance design

## Goal
Evidence-back the existing fixed Image conversion surface before adding more Image behavior or beginning Video expansion.

## Product boundary
Every conversion under qualification must enter through packaged `Converty.Bridge.exe`, resolve a fixed typed Image preset, execute in the Strict disposable EngineWorker/provider boundary, use only the fixed app-local FFmpeg executable, stage privately and publish transactionally with numbered no-overwrite semantics.

Fixture generation and ffprobe verification may use the pinned development FFmpeg/ffprobe binaries as test tooling; they must not replace the product path being tested.

## Matrix
Representative source extensions are the complete currently advertised Image set: PNG, JPG, JPEG, WebP, BMP, GIF, TIF and TIFF. Fixed actions are PNG, JPEG and WebP. This yields 24 product conversions.

Every success must prove expected codec, dimensions, Unicode/metacharacter path handling, source preservation, pre-existing destination preservation, numbered publication and zero partial residue.

Negative acceptance must include malformed and physically truncated Image input, repeated to prove deterministic bounded failure. Failure must preserve sources/destinations, publish nothing and leave no partial output.

## Non-goals
- No new Image engine or image-specific Bridge/Host logic.
- No arbitrary encoder arguments or executable discovery.
- No production redistribution approval of the development Gyan FFmpeg payload.
- No claim of headed Explorer or signed production-package acceptance.

## Decision
If the existing Image path passes the contract, dev.18 remains an acceptance/evidence tranche with no unnecessary production-code churn. If it fails, fix the root cause test-first at the narrowest existing architectural boundary.
