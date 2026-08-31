# Converty
<img width="1536" height="1024" alt="Converty Architecture Blueprint" src="https://github.com/user-attachments/assets/985f38c5-5c04-4b45-b69f-5deb0cdcc374" />

Windows 11 modern-context-menu file conversion platform. Converty is a modular right-click converter for Audio, Images, Video, and future file families while keeping Explorer, coordinator, worker, media-engine, staging, and publication trust boundaries explicit.

## Workspace version
**0.1.0-dev.18** — frozen fixed Image source/action and malformed/truncated-input acceptance through the existing Strict Worker/provider path.

## Current evidence-backed state
The product path remains:

`IExplorerCommand → fixed Converty.Bridge.exe → Strict Converty.EngineWorker.exe → typed preset/provider → fixed app-local ffmpeg.exe → private staging → validated no-overwrite numbered publication`

Dev.18 qualifies the already-existing fixed Image actions without introducing a parallel image subsystem or widening the executable/argument surface. Eight advertised Image source extensions (`png`, `jpg`, `jpeg`, `webp`, `bmp`, `gif`, `tif`, `tiff`) are exercised against all three fixed Image actions (`image.png`, `image.jpeg`, `image.webp`) for 24 real packaged Bridge→Strict Worker/provider→FFmpeg conversions. Every success is ffprobe-verified for the expected codec and 64×48 dimensions, preserves source and a pre-existing destination byte-for-byte, publishes only a numbered output, and leaves no partial residue.

Malformed and physically truncated Image inputs are each repeated and reject deterministically with Bridge exit code 4, publish nothing, preserve existing files and leave no `.converty-*.partial.*` files. Existing Audio acceptance remains recursively green, including the 36-case Audio matrix and mixed-valid/invalid batch isolation.

The dev.18 exact-main qualification anchor is commit `ef079f7e7923e399624067c4d54b9ce7577bf090`, tree `0af729f150897d170eac9f9aebfd5bc7d5d4083a`, run `33390111824`. All three jobs succeeded: managed `99481546832`, supply-chain-static `99481546572`, continuity `99481546655`. The run passed 18/18 locked restore, dependency audit with 0 vulnerable-result packages, Release build with 0 warnings/errors, native Explorer/package/direct+registered COM/product gates, all Audio regression gates, 24/24 Image conversions plus repeated malformed/truncated rejection, 254/254 managed tests, 95/95 static tests, 5/5 vectors and tracked generated-authority zero-diff.

Deterministic exact-main workspace: `Converty_0.1.0-dev.18_full_workspace.zip`, SHA-256 `4be8d5a2f503a8a885347b647bbd0aa0b61ce6d56b3ac39d9af7b11fb801628a`, 463162 bytes, 372 entries. Generated-authority artifact `9757102986`; verified-delivery artifact `9757169067`. Re-fetch live `main` before development because metadata-only curation may advance the commit without changing product behavior.

## Next product tranche
`0.1.0-dev.19`: Image multi-file/mixed-valid-invalid failure isolation and Image matrix closure. Do not begin Video expansion until that Image closure tranche is evidence-backed and frozen.

## Still open before customer launch
- headed Windows 11 modern Explorer acceptance, exact-build screenshots and crash/hang/failure matrix;
- Image multi-file/mixed-valid-invalid isolation and Image matrix closure before Video expansion;
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
7. `docs/development/DEV18_IMAGE_INPUT_ACCEPTANCE_TDD_EVIDENCE.md`
8. `docs/superpowers/specs/2026-08-31-dev18-image-input-acceptance-design.md`
9. `docs/superpowers/plans/2026-08-31-dev18-image-input-acceptance.md`
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
./build/image-input-acceptance-smoke.ps1
./build/test.ps1 -Configuration Release
```

Disposable build, test, media, package and log output stays below excluded `artifacts/`, `bin/`, `obj/`, and cache directories. Source-controlled tests remain under `tests/`. Gyan FFmpeg remains development qualification input only and is not production redistribution approval.
