# Dev.16 Audio source and malformed-input acceptance design

## Goal
Close the representative single-file Audio source/input-failure acceptance gap before expanding to Image or Video.

## Design
- Keep the frozen Explorer→Bridge→Strict EngineWorker/provider→fixed app-local FFmpeg→private staging→transactional publication architecture.
- Add one dedicated Windows acceptance component under `build/`; all disposable fixtures/results live below excluded `artifacts/`.
- Generate deterministic short development fixtures for WAV/FLAC/MP3/M4A/Ogg/Opus using the pinned development FFmpeg only as test-input preparation.
- Exercise every fixture through every fixed Audio preset by launching packaged Bridge with structured arguments.
- Verify codec, source preservation, pre-existing destination preservation, numbered no-overwrite publication and zero partial outputs.
- Exercise malformed WAV and truncated FLAC repeatedly; require bounded deterministic nonzero failure, no source/destination mutation, no published output and no partial residue.
- Noninteractive automation may explicitly suppress modal UI, but Explorer default behavior must remain unchanged.

## Non-goals
No raw FFmpeg surface, arbitrary converter selection, PATH lookup, network dependency, silent containment downgrade, media parsing in Host/Bridge, Image/Video expansion, production FFmpeg redistribution approval or production signing claim.
