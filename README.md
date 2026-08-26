# Converty
<img width="1536" height="1024" alt="Converty Architecture Blueprint" src="https://github.com/user-attachments/assets/985f38c5-5c04-4b45-b69f-5deb0cdcc374" />

Windows 11 modern-context-menu file conversion platform. Converty is being built as a modular right-click converter for Audio, Images, Video, and future file families while keeping the final shipping architecture explicit about Explorer, coordinator, worker, and media-engine trust boundaries.

## Workspace version
**0.1.0-dev.9** — first automated functional Explorer → Bridge → FFmpeg product qualification, 2026-08-26.

## Current evidence-backed state
The dev.9 behavior head `b71aa06fb024afe85f64707b05d996e86c37d8c8` was exercised by permanent GitHub Actions run `33001019450` on Windows Server 2025 / .NET SDK `10.0.400`:
- 15/15 managed projects restored from committed lock files.
- Restored-graph NuGet audit: PASS, zero vulnerable-result packages.
- Release build: PASS, zero warnings and zero errors.
- Native C++20/MSVC Explorer DLL and registration smoke executable: PASS.
- Pinned development FFmpeg/ffprobe 9.0.1 archive hash verification and executable probes: PASS.
- MakeAppx validation of the unsigned development package: PASS.
- Direct staged `Converty.ShellExtension.dll` class-factory + `IExplorerCommand::Invoke`: PASS.
- Loose package registration plus packaged COM activation + `IExplorerCommand::Invoke`: PASS.
- Product Bridge → FFmpeg smoke: PASS with Unicode/metacharacter filename, source preservation, pre-existing destination preservation, numbered-copy publication, no leftover partial output, and ffprobe verification of MP3 at 320000 bit/s.
- Microsoft Testing Platform/xUnit: **176/176 PASS**, 0 failed, 0 skipped.
- Python/static repository gates: **54/54 PASS** at the behavior head after in-job authority regeneration.

Dev.9 also implements transactional development output publication: FFmpeg writes a Converty-owned same-directory partial file, successful non-empty output is published with no-overwrite semantics to a freshly resolved numbered destination, and failure cleanup only removes the owned partial path.

## What dev.9 does not claim
Dev.9 is a development product qualification, not a shipping release. These gates remain open:
- real headed Windows 11 Explorer modern-menu acceptance with screenshots/evidence;
- Explorer crash/hang/failure acceptance matrix;
- B2 connected-server anti-squatting/final wire/session hardening;
- B4 worker containment, Job Object/resource limits, no-network and outside-scope write canaries;
- migration of the development FFmpeg execution spike into the final restricted worker/provider architecture;
- production FFmpeg redistribution/license/notices/signature decision;
- signed production MSIX, clean-VM install/update/uninstall, release signing and final release audit.

The unsigned development MSIX and the Gyan FFmpeg 9.0.1 payload are qualification inputs only. They are not production redistribution/signing approval.

## Product-first architecture decision
`docs/adr/ADR-013-dev9-functional-product-spike.md` records the approved exception that allowed dev.9 to qualify the minimum functional product before all earlier infrastructure gates were closed. It does **not** waive the shipping gates. Do not revert the working Explorer → Bridge/Core → fixed app-local FFmpeg path merely to restore the older infrastructure-first ordering.

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
