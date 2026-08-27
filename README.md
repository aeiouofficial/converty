# Converty
<img width="1536" height="1024" alt="Converty Architecture Blueprint" src="https://github.com/user-attachments/assets/985f38c5-5c04-4b45-b69f-5deb0cdcc374" />

Windows 11 modern-context-menu file conversion platform. Converty is being built as a modular right-click converter for Audio, Images, Video, and future file families while keeping Explorer, coordinator, worker, media-engine, staging, and publication trust boundaries explicit.

## Workspace version
**0.1.0-dev.10** — B4 disposable-worker containment and worker/provider migration behavior-qualified on 2026-08-27; generated/release authority closure is the next repository step.

## Current evidence-backed state
The dev.10 B4 behavior head is `f221563c790057344a94b4e60c309d4512a77c38`. Permanent GitHub Actions run `33028554361` on Windows Server 2025 / `windows-2025-vs2026` / .NET SDK `10.0.400` exercised the contained product path:

`IExplorerCommand → fixed Converty.Bridge.exe → Strict Converty.EngineWorker.exe → typed preset/provider → fixed app-local ffmpeg.exe → private staging → validated no-overwrite numbered publication`

Observed behavior qualification:
- 18/18 managed projects restored from committed lock files.
- NuGet vulnerability audit: PASS, zero vulnerable-result packages.
- Release build: PASS, 0 warnings, 0 errors.
- Native C++20/MSVC Explorer DLL: PASS.
- Pinned development FFmpeg/ffprobe 9.0.1 hash/execution: PASS.
- MakeAppx unsigned development package: PASS.
- Direct staged shell DLL class-factory + `IExplorerCommand::Invoke`: PASS.
- Loose package registration + packaged COM activation + `IExplorerCommand::Invoke`: PASS.
- Strict product Bridge → EngineWorker → FFmpeg conversion: PASS with Unicode/metacharacter paths, source preservation, pre-existing destination preservation, numbered publication, and ffprobe verification of MP3 at exactly 320000 bit/s.
- Microsoft Testing Platform/xUnit: **190/190 PASS**, 0 failed, 0 skipped.
- Repository/static gates: **66/66 PASS**; contract vectors **5/5 PASS**.

B4 now includes private per-job staging; FFmpeg execution isolated in `Converty.EngineWorker`/`Converty.Provider.FFmpeg`; suspended native worker creation; explicit inherited-handle list; Job Object kill-on-close, active-process, process/job-memory, CPU, wall-clock and output-growth ceilings; unique zero-capability AppContainer strict launches; application read/execute and staging read/write ACLs; reparse-point rejection; outside-scope-write and loopback-network denial canaries; explicit Strict/Compatibility profiles with no silent downgrade. `WorkerResourceLimits.ConversionDefault` has an 8 GiB output ceiling and a 16 GiB hard configuration maximum. The executable output-limit canary proves a strict worker is terminated after crossing a 64 KiB staging-growth budget.

The behavior run built the workspace ZIP twice with identical bytes (`a0edd6e15a63d71cc2ef493ef33f6bb6e3f0b16ee0d8f484ebc981b800f749de`, 369035 bytes, 328 files) and then failed exactly at embedded package-manifest verification because tracked generated authority was still dev.9/stale. That expected authority-sync failure is why this branch is not yet a frozen dev.10 delivery.

## What dev.10 still does not claim
Dev.10 B4 behavior is not a shipping release. These gates remain open:
- real headed Windows 11 modern Explorer context-menu visibility/usability and exact-build screenshots;
- Explorer crash/hang/failure headed matrix;
- remaining B2 connected-server anti-squatting, final status/cancel wire decision, and replay/disconnect/session acceptance;
- production FFmpeg redistribution/license/notices/signature/hash approval; the Gyan payload is development qualification input only;
- signed production MSIX and clean Windows 11 VM install/update/uninstall;
- final security/fuzz/chaos/release audit and end-user shipping acceptance.

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
