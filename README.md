# Converty
<img width="1536" height="1024" alt="Converty Architecture Blueprint" src="https://github.com/user-attachments/assets/985f38c5-5c04-4b45-b69f-5deb0cdcc374" />

Windows 11 modern-context-menu file conversion platform. Converty is being built as a modular right-click converter for Audio, Images, Video, and future file families while keeping Explorer, coordinator, worker, media-engine, staging, and publication trust boundaries explicit.

## Workspace version
**0.1.0-dev.12** — output-budget containment verification is deterministic without weakening production limits or containment.

## Current evidence-backed state
The product path remains:

`IExplorerCommand → fixed Converty.Bridge.exe → Strict Converty.EngineWorker.exe → typed preset/provider → fixed app-local ffmpeg.exe → private staging → validated no-overwrite numbered publication`

Dev.12 fixes the historical output-budget test intermittent at the harness boundary. The canary now writes exactly 64 KiB + 4 KiB and then holds, so the existing strict launcher must detect a bounded breach and terminate the worker. `WindowsWorkerProcessLauncher`, production `WorkerResourceLimits`, AppContainer/Job Object containment, poll interval, and normal conversion routing are unchanged.

Behavior head `f4c241b0895d06d2e44d72f31e07f141cdc74577` run `33271379504` passed 18/18 locked restore, zero vulnerable-result packages, Release 0 warnings/errors, native Explorer, unsigned MakeAppx, direct and registered COM Invoke, Bridge→Strict Worker→FFmpeg conversion, Unicode/metacharacter paths, source/existing-destination preservation, numbered publication, MP3 exactly 320000 bit/s, 192/192 managed, 72/72 static and 5/5 vectors. The run stopped only at tracked generated-authority/workspace-integrity freshness because the source bytes had changed; dev.12 generated authority regeneration is the current closure step.

## What dev.12 still does not claim
- headed Windows 11 modern Explorer UI acceptance, exact-build screenshots or crash/hang/failure matrix;
- production signed-package B2 requalification;
- status/cancel and replay/disconnect/reconnect/session acceptance;
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
7. `docs/adr/ADR-013-dev9-functional-product-spike.md`
8. `docs/SECURITY_THREAT_MODEL.md`
9. `docs/TEST_AND_RELEASE_GATES.md`
10. `docs/security/B2_SERVER_AUTH_GATE.md`
11. `docs/supply-chain/SBOM_POLICY.md`
12. `docs/supply-chain/RELEASE_SIGNING_POLICY.md`

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
