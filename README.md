# Converty
<img width="1536" height="1024" alt="Converty Architecture Blueprint" src="https://github.com/user-attachments/assets/985f38c5-5c04-4b45-b69f-5deb0cdcc374" />

Windows 11 modern-context-menu file conversion platform. Converty is a modular right-click converter for Audio, Images, Video, and future file families while keeping Explorer, coordinator, worker, media-engine, staging, and publication trust boundaries explicit.

## Workspace version
**0.1.0-dev.19** — Image multi-file/mixed-valid-invalid failure isolation is under evidence-backed qualification; Audio and single-file Image acceptance remain recursively green.

## Current evidence-backed state
The product path remains:

`IExplorerCommand → fixed Converty.Bridge.exe → Strict Converty.EngineWorker.exe → typed preset/provider → fixed app-local ffmpeg.exe → private staging → validated no-overwrite numbered publication`

Dev.19 adds a real Windows packaged mixed-Image batch acceptance gate without introducing a parallel image subsystem or widening the executable/argument surface. One same-family Bridge invocation receives valid Image files before and after malformed/truncated members. Ordinary per-file conversion failures must not suppress later valid selections; the batch reports aggregate failure only after all members have been attempted. The gate also checks source and pre-existing destination preservation, numbered publication, partial cleanup and absence of converter-worker or FFmpeg processes left behind.

The dev.18 exact-main qualification anchor remains commit `ef079f7e7923e399624067c4d54b9ce7577bf090`, tree `0af729f150897d170eac9f9aebfd5bc7d5d4083a`, run `33390111824`. All three jobs succeeded with the complete Audio and Image single-file acceptance gates.

## Dev.19 qualification target
- Image mixed selection: valid → malformed → valid → truncated → valid;
- one Bridge process per batch attempt;
- aggregate Bridge exit code `4` after processing the full selection;
- later valid Images publish numbered PNG outputs;
- failing members publish no output;
- source and pre-existing destination bytes remain unchanged;
- no `.converty-*.partial.*` residue;
- no orphan converter-worker/FFmpeg processes;
- two repeated attempts;
- complete Audio regression and 24-case Image single-file matrix remain green.

## Still open before customer launch
- freeze dev.19 after generated-authority synchronization and exact-main qualification;
- additional Image malformed corpus if evidence warrants it;
- Video action/source/malformed/batch qualification;
- headed Windows 11 modern Explorer acceptance, exact-build screenshots and crash/hang/failure matrix;
- production signed-package B2 identity/authentication requalification;
- production FFmpeg/ffprobe redistribution/license/notices/signature/hash approval;
- signed production MSIX and clean Windows 11 install/update/uninstall acceptance;
- final fuzz/chaos/security/release audit and headed end-user acceptance;
- UX/settings defaults, mixed-selection UX, progress/result presentation;
- plugin SDK manifest/API/signature/hash gate.

## Start here
1. `docs/HANDOVER_NEXT_AGENT.md`
2. `docs/HANDOVER_PROMPT.txt`
3. `machine-readable/handover_state.json`
4. `machine-readable/build_evidence.json`
5. `docs/development/IMPLEMENTATION_STATUS.md`
6. `docs/TASK_BACKLOG.md`
7. `docs/Converty_Master_Build_Plan.md`
8. `docs/development/DEV19_IMAGE_BATCH_ISOLATION_TDD_EVIDENCE.md`
9. `docs/superpowers/specs/2026-09-02-dev19-image-batch-isolation-design.md`
10. `docs/superpowers/plans/2026-09-02-dev19-image-batch-isolation.md`
11. `docs/SECURITY_THREAT_MODEL.md`
12. `docs/TEST_AND_RELEASE_GATES.md`

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
./build/audio-batch-isolation-smoke.ps1
./build/image-input-acceptance-smoke.ps1
./build/image-batch-isolation-smoke.ps1
./build/test.ps1 -Configuration Release
```

Disposable build, test, media, package and log output stays below excluded `artifacts/`, `bin/`, `obj/`, and cache directories. Source-controlled tests remain under `tests/`. Gyan FFmpeg remains development qualification input only and is not production redistribution approval.
