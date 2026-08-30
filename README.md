# Converty
<img width="1536" height="1024" alt="Converty Architecture Blueprint" src="https://github.com/user-attachments/assets/985f38c5-5c04-4b45-b69f-5deb0cdcc374" />

Windows 11 modern-context-menu file conversion platform. Converty is a modular right-click converter for Audio, Images, Video, and future file families while keeping Explorer, coordinator, worker, media-engine, staging, and publication trust boundaries explicit.

## Workspace version
**0.1.0-dev.15** — expanded fixed typed Audio conversion actions through the existing Strict Worker/provider path.

## Current evidence-backed state
The product path remains:

`IExplorerCommand → fixed Converty.Bridge.exe → Strict Converty.EngineWorker.exe → typed preset/provider → fixed app-local ffmpeg.exe → private staging → validated no-overwrite numbered publication`

Dev.15 adds fixed Audio actions for M4A/AAC 256k, Opus 192k VBR and Ogg Vorbis q6 alongside the existing MP3 320k, FLAC and WAV presets. The native Explorer submenu mirrors these stable preset IDs; Bridge accepts the typed preset ID only, and FFmpeg arguments remain fixed inside the product registry/provider boundary.

Behavior head `335754a7d99c99f918fa7f2bc29a89f691f0fd2a`, run `33331186761`, passed 18/18 locked restore, zero-vulnerability audit, Release build with 0 warnings/errors, native Explorer, unsigned MakeAppx, direct and registered COM Invoke, 253/253 managed tests, 81/81 static tests and 5/5 vectors. The real product smoke converted the same Unicode/metacharacter WAV through packaged Bridge→Strict Worker→FFmpeg to MP3, M4A/AAC, Opus and Ogg Vorbis while preserving the source and each pre-existing destination and publishing numbered outputs. The run stopped only at the expected generated-authority/workspace-integrity freshness boundary.

## What dev.15 still does not claim
- headed Windows 11 modern Explorer UI acceptance, exact-build screenshots or crash/hang/failure matrix;
- broad Audio source-format/malformed-input acceptance across the expanded matrix;
- production signed-package B2 requalification;
- production FFmpeg redistribution/license/notices/signature/hash approval;
- signed production MSIX and clean Windows 11 VM lifecycle;
- final security/fuzz/chaos/release audit or end-user acceptance.

## Start here
1. `docs/HANDOVER_NEXT_AGENT.md`
2. `machine-readable/handover_state.json`
3. `machine-readable/build_evidence.json`
4. `docs/development/IMPLEMENTATION_STATUS.md`
5. `docs/TASK_BACKLOG.md`
6. `docs/Converty_Master_Build_Plan.md`
7. `docs/superpowers/specs/2026-08-30-dev15-audio-preset-matrix-design.md`
8. `docs/superpowers/plans/2026-08-30-dev15-audio-preset-matrix.md`
9. `docs/SECURITY_THREAT_MODEL.md`
10. `docs/TEST_AND_RELEASE_GATES.md`

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
./build/test.ps1 -Configuration Release
```

Supply-chain/static verification:
```bash
python scripts/verify_ci_actions.py
python scripts/verify_release_inputs.py
python scripts/generate_sbom.py --mode source
python scripts/generate_sbom.py --mode release
python scripts/generate_package_manifest.py
python scripts/generate_hash_manifest.py
python scripts/verify_repository.py
python scripts/verify_contract_vectors.py
python -m pytest -q tests/static
```

Complete snapshots use `Converty_<VERSION>_full_workspace.zip`; build caches, `.git`, package caches, Python bytecode, `.env`, and common private-key formats are excluded.
