# Converty
<img width="1536" height="1024" alt="Converty Architecture Blueprint" src="https://github.com/user-attachments/assets/985f38c5-5c04-4b45-b69f-5deb0cdcc374" />

Windows 11 modern-context-menu file conversion platform. The long-term product is a generic, modular right-click converter for Audio, Images, Video, and future file families while keeping Explorer and the coordinator outside the untrusted media-parser boundary.

## Workspace version
**0.1.0-dev.5** — qualified B0/B1 foundation and product rebrand closure, 2026-08-25.

## Current evidence-backed state
The engine-independent foundation is implemented and qualified on Windows Server 2025 with .NET SDK `10.0.400`:
- `Converty.Contracts` — versioned IDs/contracts with schema-aligned bounds; no parser/process/network logic.
- `Converty.Core` — format registry, capability graph, deterministic planner, safe output-name resolution.
- `Converty.Serialization` — strict `System.Text.Json` v1 adapters with version dispatch, unknown-member rejection, recursive duplicate-key rejection, and no transport/execution dependencies.
- `Converty.FakeProviders` — non-executing Audio/Image/Video capability fixtures.
- `schemas/v1` — six strict JSON Schemas plus raw adversarial request vectors.
- Seven managed projects carry committed `packages.lock.json` files and pass locked restore.
- Restored-graph NuGet audit passes with zero vulnerable-result packages.
- Release build passes with zero warnings and zero errors.
- Microsoft Testing Platform/xUnit executes 63 tests with 63 successes.
- Static/provenance/contract-vector gates and native topology smoke pass.
- Deterministic source/release SPDX tooling, release preflight, SHA-256 workspace manifest tooling, immutable Action pins, and secret/private-key exclusions remain fail closed.

B2 Host/Bridge authenticated IPC is the next implementation tranche. Real Explorer integration, worker containment, media engines/providers, transactional final commit, settings UI, installer, and release signing remain unimplemented. Do not treat this foundation as a functioning converter yet.

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
