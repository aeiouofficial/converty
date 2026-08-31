# Dev.17 Audio batch failure-isolation design

## Goal
Close the remaining planned Audio multi-file risk without introducing another batch, IPC or media subsystem.

## Existing topology retained
Explorer's native `IExplorerCommand` enumerates the same-family selection and launches one fixed app-local Bridge process. Bridge resolves one fixed typed preset and delegates each file through the existing strict disposable EngineWorker/provider path. FFmpeg remains app-local and worker/provider-only.

## Failure semantics
`ConversionBatchRunner` processes sources sequentially. A normal `ConversionFailedException` is a per-file media failure: remember the first one, clean that file's staging in `finally`, continue later files, and after iteration rethrow the remembered failure so the Bridge still returns a failed batch result. Successful publications are not rolled back.

Do not broaden the catch. Cancellation, invalid contracts/programmer errors, launch/containment infrastructure defects and other unexpected exceptions remain fail-fast.

## Acceptance matrix
One real packaged Bridge process receives five literal source paths under one `audio.mp3` preset:
1. valid WAV;
2. malformed WAV payload;
3. valid FLAC;
4. physically truncated FLAC;
5. valid WAV.

Run the exact selection twice. Pre-create base `.mp3` destinations. Require Bridge exit 4 after processing, successful items at `(1)` then `(2)`, no output for bad inputs, byte-identical sources and base destinations, ffprobe `mp3` on successful outputs, bounded 30-second process wait and zero `.converty-*.partial.*` residue.

## Security/non-regression constraints
No shell construction, raw FFmpeg passthrough, PATH lookup, arbitrary executable, network dependency or silent Strict→Compatibility fallback. Host/Bridge remain codec/process neutral. The smoke may use pinned development FFmpeg only to create/probe deterministic fixtures; conversion under test must enter packaged Bridge. Gyan development binaries are not production redistribution approval.
