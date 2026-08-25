# Changelog

## 0.1.0-dev.7 — 2026-08-25
- Added a strict persistent Host job journal with schema v1, 4096-entry and 8 MiB hard bounds, duplicate/unknown-member rejection, canonical ID validation, and deterministic ordering.
- Added crash-safe journal publication through a same-directory temporary generation using write-through plus disk flush before atomic replacement; orphan temporary files cannot override the committed generation.
- Added restart recovery: queued/terminal state is preserved while interrupted `Probing` through `Committing` states become `Failed` with an explicit Host-restart reason.
- Integrated journal recovery into `HostJobQueue` before new work is accepted and made enqueue/cancel mutations persist before in-memory publication; persistence failure leaves queue state unchanged.
- Wired the existing per-user single-instance lease into a bounded Host runtime/server loop and added a real no-console `Converty.Host` WinExe entrypoint using LocalAppData state.
- Added Host journal/runtime tests and a dev.7 static B2 boundary gate.
- Qualified the dev.7 behavior head on Windows Server 2025 / .NET SDK 10.0.400: 15/15 locked projects, dependency audit with zero vulnerable-result packages, Release build with zero warnings/errors, 120/120 managed tests, and native topology smoke PASS.
- Kept B2 explicitly open for Bridge trusted Host startup/retry/fast-failure, server-auth/anti-squatting under the selected packaging/signing model, final status/cancel wire-surface decision, and remaining restart/replay/disconnect/timeout/session acceptance coverage.

## 0.1.0-dev.6 — 2026-08-25
- Began B2 without crossing the media-parser boundary: no FFmpeg/WIC/provider/process execution was added to Host or Bridge.
- Added `Converty.Ipc` with fixed 12-byte versioned framing, checked exact reads, a 1 MiB payload ceiling, cancellation, and fail-closed bad-magic/future-version/negative-length/oversized/truncated handling.
- Added `Converty.Security` with protected current-user pipe DACL construction, SID-hashed endpoint naming, injectable peer-validation tests, and real Windows connected-client SID reading through pipe impersonation.
- Added a bounded Host admission queue with duplicate/capacity rejection, status lookup, queued cancellation, strict request admission, and rejection paths that do not mutate queue state.
- Added ACL-backed `HostPipeServer` and bounded `BridgeClient` one-request/one-acknowledgement sessions with authorization before application-frame parsing and a maximum 30-second Bridge connect timeout.
- Added a tested per-user Host single-instance lease primitive keyed by the existing SID-derived identity.
- Expanded the managed graph to 15 locked projects; locked restore and restored-graph vulnerability audit pass with zero vulnerable-result packages.
- Qualified the B2 behavior head with a zero-warning/zero-error Release build, 108/108 managed tests, and native topology smoke.
- Added and executed a seven-case checked-in IPC adversarial corpus covering bad magic, future version, negative/oversized/truncated lengths, malformed request JSON, and unknown executable-command-shaped request members.
- Kept incomplete B2 work explicit: persistent crash-safe journal, complete Host executable lifetime wiring, and Bridge Host startup/retry remain open for the next tranche.

## 0.1.0-dev.5 — 2026-08-25
- Completed the product-name migration across active solution/project/module paths while preserving historical records.
- Committed and locked all seven managed NuGet graphs; clean Windows locked restore passes with .NET SDK 10.0.400.
- Qualified the restored dependency graph: zero vulnerable-result packages at the configured `all`/`low` threshold.
- Qualified the Release build with zero warnings and zero errors.
- Qualified Microsoft Testing Platform/xUnit: 63/63 managed tests pass.
- Qualified static/provenance/contract-vector gates and native topology smoke.
- Promoted permanent CI to run release preflight, generate source/release SPDX evidence, regenerate the workspace SHA-256 manifest, and publish supply-chain evidence.
- B2 Host/Bridge authenticated IPC is now unblocked for `0.1.0-dev.6`.

## 0.1.0-dev.4 — 2026-08-24
- Kept B2 blocked because .NET SDK 10.0.400 still cannot execute in this sandbox.
- Pinned all external GitHub Actions to reviewed full commit SHAs with machine-readable provenance and static drift checks.
- Added explicit NuGet `all`/`low` vulnerability auditing with `data.nuget.org` audit source.
- Added machine-readable dependency vulnerability report verification, adversarial fixtures, and CI/build integration.
- Disabled checkout credential persistence, added finite CI job timeouts, and kept workflow permissions read-only.
- Preserved deterministic SBOM, release preflight, key-custody, secret-exclusion, and B1 strict-contract controls.

## 0.1.0-dev.3 — 2026-08-24

Evidence-safe B0 supply-chain tranche. B2 remains intentionally blocked because the required .NET 10.0.400 managed gate still cannot execute in this sandbox.

### Added
- Deterministic SPDX 2.3 source-SBOM generator with a separate release mode that refuses missing NuGet lock files instead of fabricating dependency state.
- Fail-closed release-input preflight for managed lock completeness, release policy validity, and private-key/secret-like workspace material.
- Machine-readable release policy plus SBOM and release-signing policy documentation.
- Workspace packaging exclusions for `.pfx`, `.p12`, `.key`, `.pem`, and `.env`.
- Checksum-pinned raw v1 conversion-request vectors covering duplicate members, unknown members/version, and executable-command injection fields.
- Independent strict contract-vector verifier using duplicate-key rejection plus JSON Schema validation.
- Dev.3 implementation plan and supply-chain static gates.

### Verified in this tranche
- Exact results are recorded in `machine-readable/build_evidence.json`; managed .NET results remain explicitly separate from Python/CMake evidence.

### Still blocked on environment
- .NET 10.0.400 managed restore/build/xUnit and generated NuGet lock-file review.
- Release dependency SBOM completion/review because release mode correctly requires those lock files.
- Therefore B2 Host/Bridge/IPC remains unstarted.

## 0.1.0-dev.2 — 2026-08-24

Second foundation tranche. B2 remains intentionally blocked pending real managed verification.

### Added
- `FileConvert.Serialization`, a Contracts-only strict JSON v1 adapter module with explicit schema-version dispatch.
- Explicit stable wire text mappings for conversion action, conversion mode, and job state.
- Unknown-member, duplicate-member, case-sensitivity, trailing-comma, comment, invalid-enum, and unknown-version rejection tests.
- Recursive duplicate JSON-property rejection before domain mapping.
- Seeded bounded property/adversarial tests for identifier grammar, capability ordering, Unicode collision-safe output naming, and JSON unknown-member mutations.
- Schema/domain alignment for 32,767-character paths, 128-character display names, 32 format extensions, 128 preset options, and 256-character option values.
- Embedded-NUL path rejection in both domain contracts and v1 JSON schemas.
- Explicit schema-version properties on root format and capability descriptors.
- Stronger NuGet lock generation: force evaluation, post-generation completeness check, and immediate locked-mode restore.
- Dev.2 implementation plan and expanded executable static boundary gates.
- Central workspace-file packaging policy that excludes `.pytest_cache`, `bin`, `obj`, `TestResults`, build artifacts, package caches, Python bytecode, and `.git`.

### Verified in the current container
- Repository static verifier.
- Python schema/security/toolchain/package/source tests.
- CMake configure/build topology smoke.

### Still blocked on environment
- .NET 10.0.400 managed restore/build/xUnit and generated NuGet lock-file review.
- Therefore no B2 Host/Bridge/IPC implementation is started in this tranche.

## 0.1.0-dev.1 — 2026-08-24

First implementation tranche based on the approved architecture pack.

### Added
- Repository/bootstrap authority pinned to .NET SDK 10.0.400 / C# 14 / `net10.0`.
- Engine-independent `FileConvert.Contracts`, `FileConvert.Core`, and `FileConvert.FakeProviders` projects.
- Canonical file-family/format/provider/preset identifiers.
- Versioned conversion request, preset, capability, plan, and format schemas.
- Deterministic capability graph and conversion planner.
- Collision-safe numbered-copy output path resolver.
- Data-only fake Audio/Image/Video providers.
- xUnit v3 Microsoft Testing Platform test projects plus executable Python schema/security/toolchain/package/static gates.
- Native CMake topology and MSVC hardening policy scaffold for the future Explorer component.
- Windows CI, local verification/build scripts, dependency policy, module-boundary documents, and handover state.
- Workspace versioning policy and copy-paste continuation handover prompt.
- Machine-readable build evidence and current full-workspace package manifest generation.

### Not yet implemented
- Bridge/Host IPC, Explorer COM registration, sandboxed workers, FFmpeg/WIC engines, transactional file commit, settings UI, plugin loading, installer/signing, and runtime conversion.
