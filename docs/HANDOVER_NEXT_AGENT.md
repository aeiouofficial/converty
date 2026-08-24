# Converty 0.1.0-dev.4 — Next-Agent Handover

## First action
Read, in order:
1. `machine-readable/handover_state.json`
2. `machine-readable/build_evidence.json`
3. `docs/development/IMPLEMENTATION_STATUS.md`
4. `docs/TASK_BACKLOG.md`
5. `docs/Converty_Master_Build_Plan.md`
6. `docs/SECURITY_THREAT_MODEL.md`
7. `docs/TEST_AND_RELEASE_GATES.md`
8. `docs/supply-chain/CI_PROVENANCE_POLICY.md`
9. `docs/supply-chain/SBOM_POLICY.md`
10. `docs/supply-chain/RELEASE_SIGNING_POLICY.md`
11. `docs/superpowers/specs/2026-08-24-foundation-design.md`
12. `docs/superpowers/plans/2026-08-24-foundation-dev4-implementation.md`

## Version and evidence authority
- Current delivered workspace: `0.1.0-dev.4`.
- Source snapshot commit: see `machine-readable/build_evidence.json`.
- Required next workspace: `0.1.0-dev.5`.
- `VERSION`, machine-readable evidence/state, `eng/toolchain.json`, README, CHANGELOG, implementation status, backlog, and handover must move together.
- Never reuse dev.4 source tree with a dev.5 archive name.

## What exists
The repository now contains the complete B0 topology and an authored B1 engine-independent foundation:
- Generic Contracts/Core/Serialization/FakeProviders source remains present.
- Strict v1 JSON schemas and schema-aligned domain limits remain present.
- Strict JSON adapters reject unknown versions, unknown members, duplicate keys, invalid enum text, trailing commas/comments, over-limit paths/options, and embedded NUL path content.
- Property/adversarial managed test source remains present.
- Python static/schema/toolchain/package/source tests are executable without .NET.
- Deterministic source SPDX generation and fail-closed release-SBOM mode exist.
- Release preflight rejects missing lock files and secret/private-key-like workspace material.
- Release policy requires SHA-256+, external signing-key custody, signed Windows deliverables, timestamping, and real dependency evidence.
- Every external GitHub Action is pinned to a reviewed full commit SHA; mutable workflow Action refs fail `scripts/verify_ci_actions.py`.
- CI does not persist checkout credentials, workflow permissions remain read-only, and static/managed jobs have finite 15/30 minute timeouts.
- NuGet auditing is explicitly configured for `all` dependencies at `low` severity with a vulnerability-only audit source.
- `build/dependency-audit.ps1` + `scripts/verify_dependency_audit.py` provide a fail-closed machine-readable transitive vulnerability report path; fixtures cover clean, vulnerable, wrong-version, and missing-project cases.
- Native CMake topology/hardening policy exists, but there is deliberately no fake Explorer DLL target.
- B2 Host/Bridge/IPC remains unstarted.

## Exact dev.4 verification status
Use `machine-readable/build_evidence.json` for the machine-readable record. Do not replace that record with guesses.

The dev.4 environment still has no usable .NET SDK. The official SDK 10.0.400 binary could not be fetched through the sandbox download path, so:
- C# compile: NOT RUN.
- xUnit/Microsoft Testing Platform: NOT RUN.
- NuGet lock generation/review: NOT RUN.
- real NuGet dependency vulnerability audit: NOT RUN.
- release dependency SBOM: BLOCKED because lock graph is absent.
- B2 start gate: BLOCKED.

Executable dev.4 evidence does include:
- full Python static/schema/security/toolchain/package/source suite;
- immutable GitHub Action pin verification;
- dependency-audit verifier exercised against clean/vulnerable/malformed fixtures;
- strict raw contract-vector verifier;
- deterministic source SPDX generation;
- repository architecture verifier;
- CMake configure/build topology smoke;
- JSON/XML/Python syntax sweeps;
- deterministic ZIP integrity/package-manifest/SHA-256/CRC checks.

## Non-negotiable boundaries
1. Explorer DLL remains tiny and trigger-only; no media parsing, network, conversion, settings database, FFmpeg/WIC/plugin load, or unbounded work.
2. Host/coordinator never parses untrusted media and never dynamically loads codec/plugin code.
3. Probe and conversion occur in disposable restricted workers.
4. Ordinary local conversion has no network requirement; strict worker profile denies network.
5. IPC uses explicit same-user ACL + peer validation + bounded/versioned framing; default pipe ACL is forbidden.
6. Presets/IPC never carry raw executable command strings or raw engine argument vectors.
7. Provider options are typed/whitelisted before argument token construction.
8. Workers write only Converty-owned private staging. Host validates then atomically commits final output.
9. Strict isolation never silently falls back to compatibility mode.
10. Arbitrary writable DLL/plugin auto-loading remains forbidden.
11. Safe collision default remains numbered-copy.
12. Unknown schema versions/members and duplicate JSON members fail closed.
13. Release dependency versions must come from reviewed lock files; never infer/fabricate them in release SBOM.
14. Signing private keys never enter repository/workspace/package; secret-like/private-key path forms are excluded and preflighted.
15. External GitHub Actions stay full-SHA pinned and reviewed; mutable tags/branches are not accepted release authority.
16. CI checkout credentials remain non-persistent and jobs retain explicit finite timeouts.

## Immediate next work — required order
### 1. Run the real .NET-capable B0/B1 gate
Use SDK exactly `10.0.400`.

Run:
```powershell
./build/bootstrap.ps1 -GenerateLockFiles
```
Then inspect every generated `packages.lock.json`. The generation path intentionally performs an immediate second `--locked-mode` restore; it must pass before lock files are accepted.

Run:
```powershell
./build/dependency-audit.ps1
./build/build.ps1 -Configuration Release
./build/test.ps1 -Configuration Release
./build/native-smoke.ps1
```

Fix compile/analyzer/test defects without weakening tests or security/contracts.

### 2. Complete release dependency evidence
After reviewed locks exist:
```bash
python scripts/verify_release_inputs.py
python scripts/generate_sbom.py --mode release
```
Review the actual dependency graph, license data, and the retained machine-readable NuGet vulnerability report. Do not call the source-only SBOM release evidence.

### 3. Close B1 honestly
Run all managed unit/property/adversarial suites. If behavior defects appear, add/fix tests first. Update backlog only after actual passes.

### 4. Begin B2 only after the gate is green
Write a focused B2 design/plan first, then implement Host/Bridge/authenticated named-pipe IPC from tests/contracts. No FFmpeg/WIC execution yet.

### 5. Package `0.1.0-dev.5`
Update all authority/evidence files, generate source/release SBOM as evidence permits, regenerate `package_manifest.json` and `SHA256SUMS.txt`, package with `scripts/package_workspace.py`, reopen/verify the archive, and provide the next handover prompt.

## If .NET is still unavailable
Do **not** start B2. Advance only useful non-runtime B0 supply-chain/release tooling that does not depend on speculative runtime behavior, or package an evidence-only dev.5 if no honest implementation progress can be made. Never convert missing managed evidence into a pass.
