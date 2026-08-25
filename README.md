# Converty
<img width="1536" height="1024" alt="Converty Architecture Blueprint" src="https://github.com/user-attachments/assets/985f38c5-5c04-4b45-b69f-5deb0cdcc374" />

Windows 11 modern-context-menu file conversion platform. The long-term product is a generic, modular right-click converter for Audio, Images, Video, and future file families while keeping Explorer and the coordinator outside the untrusted media-parser boundary.

## Workspace version
**0.1.0-dev.6** — first evidence-backed B2 Host/Bridge hardened-IPC tranche, 2026-08-25.

## Current evidence-backed state
The B0/B1 foundation remains qualified and the first B2 IPC tranche now runs on Windows Server 2025 with .NET SDK `10.0.400`:
- `Converty.Contracts` / `Converty.Core` / `Converty.Serialization` / `Converty.FakeProviders` remain the engine-independent B1 foundation.
- `Converty.Ipc` owns a fixed 12-byte, versioned, checked length-prefixed frame with a 1 MiB payload ceiling and fail-closed malformed/truncated handling.
- `Converty.Security` owns a protected current-user pipe DACL, SID-qualified endpoint naming, and connected-client SID validation via pipe impersonation.
- `Converty.Host` owns a bounded in-memory admission queue, duplicate/capacity rejection, queued status/cancellation, strict request admission, ACL-backed named-pipe sessions, and a tested per-user single-instance lease primitive.
- `Converty.Bridge` submits one bounded request to the same-user Host endpoint with a finite connect timeout and strictly validates one acknowledgement before returning.
- Fifteen managed projects carry committed `packages.lock.json` files and pass locked restore.
- Restored-graph NuGet audit passes with zero vulnerable-result packages.
- Release build passes with zero warnings and zero errors.
- Microsoft Testing Platform/xUnit executes **108 tests with 108 successes** on the qualified B2 behavior head.
- The checked-in IPC adversarial corpus executes seven malformed/future/oversized/truncated/request-shape cases against the real codec/admission path.
- Static/provenance/contract-vector gates and native topology smoke pass; exact closure evidence is recorded in `machine-readable/build_evidence.json`.

This is **not** a complete B2 product runtime yet. Persistent crash-safe journal semantics, a complete Host lifetime executable wired to the single-instance lease/server loop, and Bridge Host startup/retry behavior remain open. Explorer integration, worker containment, media engines/providers, transactional final output commit, settings UI, installer, and release signing also remain unimplemented. Do not treat this workspace as a functioning converter yet.

## Start here
1. `docs/HANDOVER_NEXT_AGENT.md`
2. `machine-readable/handover_state.json`
3. `machine-readable/build_evidence.json`
4. `docs/development/IMPLEMENTATION_STATUS.md`
5. `docs/TASK_BACKLOG.md`
6. `docs/Converty_Master_Build_Plan.md`
7. `docs/SECURITY_THREAT_MODEL.md`
8. `docs/TEST_AND_RELEASE_GATES.md`
9. `docs/supply-chain/CI_PROVENANCE_POLICY.md`
10. `docs/supply-chain/SBOM_POLICY.md`
11. `docs/supply-chain/RELEASE_SIGNING_POLICY.md`
12. `docs/superpowers/plans/2026-08-25-b2-host-bridge-ipc.md`

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
Source-controlled architecture authority is under `docs/`, `source/`, and `reference-images/*.dot`. The durable security boundary remains: Explorer only triggers work, the coordinator does not parse hostile media, and substantive probing/conversion belongs to disposable restricted workers.

## Versioned workspace delivery
See `docs/development/VERSIONING.md`. Complete snapshots use `Converty_<VERSION>_full_workspace.zip`; build caches, `.git`, package caches, Python bytecode, `.env`, and common private-key forms are excluded.
