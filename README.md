# Converty
<img width="1536" height="1024" alt="Converty Architecture Blueprint" src="https://github.com/user-attachments/assets/985f38c5-5c04-4b45-b69f-5deb0cdcc374" />

Windows 11 modern-context-menu file conversion platform. Converty is a modular right-click converter for Audio, Images, Video, and future file families while keeping Explorer, coordinator, worker, media-engine, staging, and publication trust boundaries explicit.

## Workspace version
**0.1.0-dev.17** — final Audio mixed-valid/invalid multi-file failure-isolation closure.

## Current evidence-backed state
The product path remains:

`IExplorerCommand → fixed Converty.Bridge.exe → Strict Converty.EngineWorker.exe → typed preset/provider → fixed app-local ffmpeg.exe → private staging → validated no-overwrite numbered publication`

Dev.17 closes the planned Audio batch-isolation tranche. A same-family Explorer multi-selection still launches one Bridge process, while `ConversionBatchRunner` now treats ordinary per-file `ConversionFailedException` failures as isolated media failures: staging is cleaned, later files are still attempted, successful files remain published, and the first conversion failure is reported only after the selection has been processed. Cancellation, programmer/contract errors and global infrastructure failures remain fail-fast.

The dedicated Windows mixed-batch smoke executes valid WAV → malformed WAV → valid FLAC → truncated FLAC → valid WAV in one Bridge process, twice. Each attempt returns aggregate exit code 4 only after processing the batch; all three valid items publish numbered MP3 outputs, failing items publish nothing, every source and pre-existing destination remains byte-identical, ffprobe validates successful MP3 outputs, and no `.converty-*.partial.*` residue remains.

Behavior candidate `5829c868c5d192c70f21ea0da9337250a8d9c961`, run `33347652162`, passed 18/18 locked restore, dependency audit with 0 vulnerable-result packages, Release build with 0 warnings/errors, native Explorer/package/direct+registered COM/product gates, the dev.16 36-conversion Audio matrix and repeated malformed/truncated cases, the new real mixed-batch gate, 254/254 managed tests, 91/91 static tests and 5/5 contract vectors. The deterministic pre-authority workspace built byte-identically twice at SHA-256 `4af24ae6f866c6389a3010642504aea13952ecb17d717c9974d05161fb8f6ba0`, 447903 bytes, 364 entries, then stopped only at the expected stale generated-authority manifest check.

## Still open before customer launch
- headed Windows 11 modern Explorer acceptance, exact-build screenshots and crash/hang/failure matrix;
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
7. `docs/development/DEV17_AUDIO_BATCH_ISOLATION_TDD_EVIDENCE.md`
8. `docs/superpowers/specs/2026-08-31-dev17-audio-batch-isolation-design.md`
9. `docs/superpowers/plans/2026-08-31-dev17-audio-batch-isolation.md`
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
./build/audio-batch-isolation-smoke.ps1
./build/test.ps1 -Configuration Release
```

Disposable build, test, media, package and log output stays below excluded `artifacts/`, `bin/`, `obj/`, and cache directories. Gyan FFmpeg remains development qualification input only and is not production redistribution approval.
