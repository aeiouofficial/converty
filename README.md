# Converty
<img width="1536" height="1024" alt="FileConvert Architecture Blueprint" src="https://github.com/user-attachments/assets/985f38c5-5c04-4b45-b69f-5deb0cdcc374" />

Windows 11 modern-context-menu file conversion platform. The long-term product is a generic, modular right-click converter for Audio, Images, Video, and future file families while keeping Explorer and the coordinator outside the untrusted media-parser boundary.

## Workspace version
**0.1.0-dev.4** — fourth implementation tranche, 2026-08-24.

## Current evidence-backed state
Implemented source scaffolding exists for the deterministic engine-independent foundation:
- `Converty.Contracts` — versioned IDs/contracts with schema-aligned bounds; no parser/process/network logic.
- `Converty.Core` — format registry, capability graph, deterministic planner, safe output-name resolution.
- `Converty.Serialization` — strict `System.Text.Json` v1 adapters with explicit enum text, version dispatch, unknown-member rejection, recursive duplicate-key rejection, and no transport/execution dependencies.
- `Converty.FakeProviders` — non-executing Audio/Image/Video capability fixtures.
- `schemas/v1` — six strict JSON Schemas with `additionalProperties: false` and NUL-rejecting path constraints.
- `tests` — xUnit v3/Microsoft Testing Platform source including seeded property/adversarial suites plus executable Python schema/security/toolchain/static verification.
- `scripts/generate_sbom.py` — deterministic SPDX 2.3 source inventory; release mode fails closed until every managed lock file exists.
- `machine-readable/release_policy.json` + `docs/supply-chain` — release hashing/signing/key-custody, immutable CI Action, and dependency-audit policy.
- `machine-readable/ci_action_pins.json` + `scripts/verify_ci_actions.py` — reviewed full-SHA GitHub Action authority; mutable workflow refs fail the static gate.
- CI containment — checkout credentials are not persisted, workflow permissions are read-only, and both jobs have finite timeouts.
- `build/dependency-audit.ps1` + `scripts/verify_dependency_audit.py` — `.NET 10` machine-readable transitive vulnerability audit path with fail-closed report parsing and adversarial fixtures.
- `tests/vectors/v1` — raw request vectors for valid, duplicate-member, unknown-member/version, and command-injection-field cases.

Not implemented yet: real Explorer integration, Bridge/Host IPC, probe/engine workers, sandbox/Job Object enforcement, FFmpeg/WIC providers, transactional commit, UI, installer, or signing. Do not treat this source tranche as a functioning converter.

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
12. `docs/superpowers/plans/2026-08-24-foundation-dev4-implementation.md`

## First build on a Windows/.NET-capable machine
```powershell
./build/bootstrap.ps1 -GenerateLockFiles
./build/dependency-audit.ps1
./build/build.ps1 -Configuration Release
./build/test.ps1 -Configuration Release
./build/native-smoke.ps1
```

`bootstrap.ps1 -GenerateLockFiles` regenerates the lock graph and immediately verifies it again in `--locked-mode`. Review every generated lock file before commit.

After reviewed lock files are committed:
```powershell
./build/verify.ps1
./build/native-smoke.ps1
```

## Verification available in a Python/CMake environment
```bash
python scripts/verify_ci_actions.py
python scripts/verify_repository.py
python scripts/verify_contract_vectors.py
python -m pytest -q tests/static
cmake --preset native-smoke
cmake --build --preset native-smoke
```

## Architecture authority
The source-controlled architecture authority is under `docs/`, `source/`, and `reference-images/*.dot`. The complete versioned workspace ZIP additionally carries presentation/render artifacts such as the Word build-plan rendering and PNG/SVG diagram renders; those generated/binary presentation copies are intentionally not required for building or continuing the Git repository.

## Versioned workspace delivery
See `docs/development/VERSIONING.md`. Assistant tranches ship complete `Converty_<VERSION>_full_workspace.zip` snapshots; build caches, `.git`, package caches, Python bytecode, `.env`, and common private-key file forms are excluded.
