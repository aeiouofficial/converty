# Converty
<img width="1536" height="1024" alt="Converty Architecture Blueprint" src="https://github.com/user-attachments/assets/985f38c5-5c04-4b45-b69f-5deb0cdcc374" />

Windows 11 modern-context-menu file conversion platform. Converty is a modular right-click converter for Audio, Images, Video, and future file families while keeping Explorer, coordinator, worker, media-engine, staging, and publication trust boundaries explicit.

## Workspace version
**0.1.0-dev.16** — Audio source-format and malformed/truncated-input acceptance through the existing Strict Worker/provider path.

## Current evidence-backed state
The product path remains:

`IExplorerCommand → fixed Converty.Bridge.exe → Strict Converty.EngineWorker.exe → typed preset/provider → fixed app-local ffmpeg.exe → private staging → validated no-overwrite numbered publication`

Dev.16 keeps the fixed MP3/FLAC/M4A-AAC/Opus/Ogg-Vorbis/WAV action matrix from dev.15 and adds a dedicated Windows acceptance matrix covering WAV, FLAC, MP3, M4A/AAC, Ogg/Vorbis, and Opus as source formats against all six Audio actions: 36 real Bridge→Strict Worker→FFmpeg conversions. Every case uses Unicode/metacharacter filenames, preserves source and pre-existing destination bytes, publishes numbered output, leaves no partial file, and is ffprobe codec-verified.

The dev.16 matrix also exercises malformed WAV and physically truncated FLAC inputs twice each. It uncovered a real noninteractive failure-lifecycle defect: conversion failure returned correctly, but Bridge then blocked on the user-facing `MessageBoxW`. The fix adds an explicit automation-only `CONVERTY_BRIDGE_NONINTERACTIVE=1` reporter path that writes the same bounded error to stderr; Explorer does not set it and retains the modal error dialog by default. Both malformed and truncated cases now fail deterministically with exit code 4, preserve all existing files, publish nothing, and leave no partial output.

Behavior head `061ad75600fee6fd4b34e4a24bd8d571ac17ce90`, run `33340338502`, passed 18/18 locked restore, dependency audit with 0 vulnerable-result packages, Release build with 0 warnings/errors, native Explorer, unsigned development package, direct and registered COM Invoke, the existing four-target product smoke, the 36+negative Audio acceptance matrix, 253/253 managed tests, 85/85 static tests and 5/5 contract vectors. The deterministic pre-authority workspace built byte-identically twice at SHA-256 `27b6f96ea8c42afee8de2d67a2ea9d43f48607ab13a4b124cbab6acd3b55a643`, 436519 bytes, 358 entries, then stopped only at the expected stale generated-authority manifest.

## Still open before customer launch
- headed Windows 11 modern Explorer acceptance, exact-build screenshots and crash/hang/failure matrix;
- final Audio multi-file/mixed-valid-invalid batch isolation and matrix closure;
- production signed-package B2 identity/authentication requalification;
- production FFmpeg/ffprobe redistribution/license/notices/signature/hash approval;
- signed production MSIX and clean Windows 11 install/update/uninstall acceptance;
- final fuzz/chaos/security/release audit and headed end-user acceptance.

## Start here
1. `docs/HANDOVER_NEXT_AGENT.md`
2. `machine-readable/handover_state.json`
3. `machine-readable/build_evidence.json`
4. `docs/development/IMPLEMENTATION_STATUS.md`
5. `docs/TASK_BACKLOG.md`
6. `docs/Converty_Master_Build_Plan.md`
7. `docs/development/DEV16_AUDIO_INPUT_ACCEPTANCE_TDD_EVIDENCE.md`
8. `docs/superpowers/specs/2026-08-31-dev16-audio-input-acceptance-design.md`
9. `docs/superpowers/plans/2026-08-31-dev16-audio-input-acceptance.md`
10. `docs/SECURITY_THREAT_MODEL.md`
11. `docs/TEST_AND_RELEASE_GATES.md`

## Verification
On Windows with .NET SDK `10.0.400`:
```powershell
./build/bootstrap.ps1
./build/dependency-audit.ps1
./build/build.ps1 -Configuration Release
./build/native-smoke.ps1
./build/prepare-dev-ffmpeg.ps1
./build/stage-dev-package.ps1 -Configuration Release -FfmpegPath ./artifacts/dev-ffmpeg/ffmpeg.exe
./build/validate-dev-package.ps1
./build/explorer-registration-smoke.ps1
./build/product-conversion-smoke.ps1
./build/audio-input-acceptance-smoke.ps1
./build/test.ps1 -Configuration Release
```

Disposable build, test, media, package and log output stays below excluded `artifacts/`, `bin/`, `obj/`, and cache directories. Source-controlled tests remain organized under `tests/`; generated debris is not committed. Complete snapshots use `Converty_<VERSION>_full_workspace.zip`, with caches, `.git`, Python bytecode, `.env`, and common private-key formats excluded.
