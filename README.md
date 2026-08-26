# Converty
<img width="1536" height="1024" alt="Converty Architecture Blueprint" src="https://github.com/user-attachments/assets/985f38c5-5c04-4b45-b69f-5deb0cdcc374" />

Windows 11 modern-context-menu file conversion platform. The long-term product is a generic, modular right-click converter for Audio, Images, Video, and future file families while keeping Explorer and the coordinator outside the untrusted media-parser boundary.

## Workspace version
**0.1.0-dev.8** — B2 trusted Bridge Host-startup/retry tranche, 2026-08-25.

## Current evidence-backed state
B0/B1 remain qualified and B2 now includes a bounded Host lifetime plus a tightly constrained Bridge startup path:
- `Converty.Ipc` owns fixed 12-byte versioned framing, checked lengths, a 1 MiB payload ceiling, and fail-closed malformed/truncated handling.
- `Converty.Security` owns the protected current-user pipe DACL, SID-qualified endpoint naming, and connected-client SID validation before application-frame reads.
- `Converty.Host` is a no-console WinExe with a tested per-user single-instance server loop, strict admission, ACL-backed sessions, bounded queue/status/cancellation, and a persistent bounded crash-recovery journal.
- Host journal state is schema/member strict, capped at 4,096 entries / 8 MiB, disk-flushed before atomic publication, and interrupted in-flight work becomes `Failed` rather than being silently resumed.
- `Converty.Bridge` remains a strict one-session bounded IPC client. Connect-stage unavailability is distinguished from protocol/application failure.
- `TrustedHostPath` derives only `Converty.Host.exe` from an absolute existing non-reparse installation directory; no caller chooses an executable filename or command line.
- `InstalledHostProcessLauncher` uses `UseShellExecute=false`, an empty argument string, a hidden/no-console start, and the trusted install directory as working directory.
- `BridgeSubmissionCoordinator` first tries the existing Host, performs at most one trusted Host launch only for connect-stage unavailability, then retries inside a maximum 30-second startup deadline with retry delays capped at one second. Rejections/protocol failures do not launch Host and caller cancellation stops the retry path.
- Static/repository gates confine `Process.Start` to the single approved Bridge startup launcher; Host and all other Bridge code continue to forbid process execution, and both modules still forbid media-engine/network execution.
- Fifteen managed projects carry committed `packages.lock.json` files and pass locked restore.
- Restored-graph NuGet audit passes with zero vulnerable-result packages.
- Release build passes with zero warnings and zero errors.
- Microsoft Testing Platform/xUnit executes **129 tests with 129 successes** on the immutable dev.8 behavior run.
- Native topology smoke passes; exact run IDs are recorded in `machine-readable/build_evidence.json`.

B2 is **not fully closed yet**. Fixed-path startup and finite retry do not authenticate the server behind a connected same-user pipe. Connected-server identity / anti-squatting acceptance must be matched to the actual installer/signing/package authority before B2 can close. The final status/cancel wire decision and final replay/disconnect/session acceptance matrix also remain open. B3 Explorer work therefore remains blocked.

Explorer integration, worker containment, media engines/providers, transactional final output commit, settings UI, installer, and release signing remain unimplemented. Do not treat this workspace as a functioning media converter yet.

## Start here
1. `docs/HANDOVER_NEXT_AGENT.md`
2. `machine-readable/handover_state.json`
3. `machine-readable/build_evidence.json`
4. `docs/development/IMPLEMENTATION_STATUS.md`
5. `docs/TASK_BACKLOG.md`
6. `docs/Converty_Master_Build_Plan.md`
7. `docs/SECURITY_THREAT_MODEL.md`
8. `docs/TEST_AND_RELEASE_GATES.md`
9. `docs/security/B2_SERVER_AUTH_GATE.md`
10. `docs/supply-chain/CI_PROVENANCE_POLICY.md`
11. `docs/supply-chain/SBOM_POLICY.md`
12. `docs/supply-chain/RELEASE_SIGNING_POLICY.md`
13. `docs/superpowers/plans/2026-08-25-b2-dev8-bridge-startup.md`

## Verification
On Windows with .NET SDK `10.0.400`:
```powershell
./build/bootstrap.ps1
./build/dependency-audit.ps1
./build/build.ps1 -Configuration Release
./build/test.ps1 -Configuration Release
./build/native-smoke.ps1
```

Supply-chain/static verification:
```bash
python scripts/verify_ci_actions.py
python scripts/verify_release_inputs.py
python scripts/generate_sbom.py --mode source
python scripts/generate_sbom.py --mode release
python scripts/generate_hash_manifest.py
python scripts/verify_repository.py
python scripts/verify_contract_vectors.py
python -m pytest -q tests/static
```

## Architecture authority
Source-controlled architecture authority is under `docs/`, `source/`, and `reference-images/*.dot`. The durable boundary remains: Explorer only triggers work, the coordinator does not parse hostile media, and substantive probing/conversion belongs to disposable restricted workers.

## Versioned workspace delivery
See `docs/development/VERSIONING.md`. Complete snapshots use `Converty_<VERSION>_full_workspace.zip`; build caches, `.git`, package caches, Python bytecode, `.env`, and common private-key forms are excluded.
