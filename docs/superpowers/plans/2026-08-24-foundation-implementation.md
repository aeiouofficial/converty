# FileConvert Foundation Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Produce the first versioned FileConvert implementation workspace with reproducible B0 bootstrap and the deterministic, engine-independent B1 core.

**Architecture:** Keep all media parsing/execution outside the core. `FileConvert.Contracts` owns immutable versioned data contracts; `FileConvert.Core` owns validation, capability matching, deterministic planning, and output naming; `FileConvert.FakeProviders` supplies non-executing test capabilities. Windows Explorer, IPC, sandbox, and real engines remain separate future processes/modules.

**Tech Stack:** C# 14 / .NET 10 (`net10.0`), xUnit, native C++/CMake placeholder topology for future Explorer work, PowerShell/Python verification scripts, Git.

**Spec:** `docs/superpowers/specs/2026-08-24-foundation-design.md`

## Global Constraints
- Target SDK: .NET SDK `10.0.400` with `rollForward=latestPatch`.
- Target framework: `net10.0`.
- Nullable enabled; implicit usings enabled; warnings as errors enabled.
- No conversion engine process invocation in this tranche.
- No media parsing in Contracts/Core/FakeProviders.
- No executable command strings in presets or IPC-shaped contracts.
- All externally persisted/wire-facing contracts carry explicit schema versions.
- Output resolver defaults to numbered-copy behavior and never overwrites silently.

---

### Task 1: Repository bootstrap and build policy
**Files:** `global.json`, `Directory.Build.props`, `Directory.Packages.props`, `.editorconfig`, `.gitignore`, `FileConvert.slnx`, `build/*`, `eng/*`, `.github/workflows/ci.yml`.

- [ ] Add static bootstrap verifier first and run it to observe missing-file failures.
- [ ] Add pinned SDK/build/analyzer policy and project topology.
- [ ] Re-run static verifier and make bootstrap checks pass.
- [ ] Record toolchain limitations of the current execution environment.

### Task 2: Contracts and identifiers
**Files:** `src/FileConvert.Contracts/*`, `tests/FileConvert.Contracts.Tests/*`.

- [ ] Define tests for family/format ID validation, schema version constraints, request invariants, and job-state values.
- [ ] Run tests and confirm RED when a .NET SDK is available; otherwise preserve tests and record environment blocker.
- [ ] Implement minimal immutable contracts and validators.
- [ ] Run the complete test project when possible.

### Task 3: Capability graph
**Files:** `src/FileConvert.Core/Capabilities/*`, `tests/FileConvert.Core.Tests/Capabilities/*`.

- [ ] Define tests for provider registration, source→target lookup, duplicate capability rejection, and deterministic ordering.
- [ ] Implement `CapabilityGraph` without engine execution logic.
- [ ] Verify tests.

### Task 4: Conversion planner
**Files:** `src/FileConvert.Core/Planning/*`, `tests/FileConvert.Core.Tests/Planning/*`.

- [ ] Define tests for valid conversion, unsupported conversion, source=target policy, preferred-provider selection, and ambiguity rejection.
- [ ] Implement deterministic `ConversionPlanner` returning declarative `ConversionPlan` only.
- [ ] Verify tests.

### Task 5: Output path resolver
**Files:** `src/FileConvert.Core/Output/*`, `tests/FileConvert.Core.Tests/Output/*`.

- [ ] Define tests for extension replacement, Unicode names, existing destination numbering, and bounded collision search.
- [ ] Implement resolver with an injectable existence predicate.
- [ ] Verify tests.

### Task 6: Fake providers and cross-family fixtures
**Files:** `src/FileConvert.FakeProviders/*`, test fixtures.

- [ ] Define tests showing Audio/Image/Video capability registration and mixed family independence.
- [ ] Implement data-only fake providers.
- [ ] Verify tests.

### Task 7: Handover/version/package evidence
**Files:** `VERSION`, `CHANGELOG.md`, `docs/HANDOVER_NEXT_AGENT.md`, `machine-readable/handover_state.json`, `machine-readable/package_manifest.json`, `SHA256SUMS.txt`.

- [ ] Update implementation state with evidence-backed claims only.
- [ ] Run static repository verifier and any available unit tests.
- [ ] Generate SHA-256 manifest.
- [ ] Produce full versioned ZIP and record its hash.
