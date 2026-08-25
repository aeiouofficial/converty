# Converty
<img width="1536" height="1024" alt="Converty Architecture Blueprint" src="https://github.com/user-attachments/assets/985f38c5-5c04-4b45-b69f-5deb0cdcc374" />

Windows 11 modern-context-menu file conversion platform. The long-term product is a generic, modular right-click converter for Audio, Images, Video, and future file families while keeping Explorer and the coordinator outside the untrusted media-parser boundary.

## Workspace version
**0.1.0-dev.7** — B2 persistent Host journal/runtime tranche, 2026-08-25.

## Current evidence-backed state
The B0/B1 foundation remains qualified and B2 now has a real bounded Host lifetime on Windows Server 2025 with .NET SDK `10.0.400`:
- `Converty.Ipc` owns fixed 12-byte versioned framing, checked lengths, a 1 MiB payload ceiling, and fail-closed malformed/truncated handling.
- `Converty.Security` owns the protected current-user pipe DACL, SID-qualified endpoint naming, and connected-client SID validation before application-frame reads.
- `Converty.Host` now builds as a no-console WinExe. It owns a tested per-user single-instance runtime loop, strict request admission, ACL-backed pipe sessions, bounded queue/status/cancellation, and a persistent bounded journal.
- The journal is schema/version/member strict, capped at 4,096 entries / 8 MiB, writes through a same-directory temporary file with disk flush before atomic publication, ignores orphan temp state, rejects duplicate IDs, and converts interrupted in-flight jobs to `Failed` instead of resuming them after restart.
- Queue enqueue/cancel mutations persist before the corresponding in-memory state is published; operational journal-write failure rejects the mutation without changing queue state, while corrupt recovery data blocks Host startup before IPC begins.
- `Converty.Bridge` submits one bounded request to the same-user Host endpoint with a finite connect timeout and strictly validates one acknowledgement before returning.
- Fifteen managed projects carry committed `packages.lock.json` files and pass locked restore.
- Restored-graph NuGet audit passes with zero vulnerable-result packages.
- Release build passes with zero warnings and zero errors.
- Microsoft Testing Platform/xUnit executes **120 tests with 120 successes** on the qualified dev.7 behavior head.
- The dev.6 seven-case IPC adversarial corpus remains executed; dev.7 adds journal/runtime unit and static boundary coverage.
- Native topology smoke passes; exact closure evidence is recorded in `machine-readable/build_evidence.json`.

B2 is **not fully closed yet**. Bridge Host startup/retry from the trusted installed Host path, server-auth/pipe-squatting acceptance, and any remaining session/signature policy required by the selected packaging model are still open. Explorer integration, worker containment, media engines/providers, transactional final output commit, settings UI, installer, and release signing also remain unimplemented. Do not treat this workspace as a functioning media converter yet.

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
13. `docs/superpowers/plans/2026-08-25-b2-dev7-journal-runtime.md`

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
