# Converty Rebrand Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make `Converty` the authoritative product, solution, assembly, namespace, module, package, documentation, and workspace-delivery identity without changing conversion behavior or weakening any dev.5 qualification gate.

**Architecture:** This is an identity-only migration layered on the already-green B0/B1 foundation. A static stale-brand gate is added first, then active source/project paths and namespaces are migrated atomically enough to keep the solution coherent, dependency locks are regenerated under .NET SDK 10.0.400, and current documentation/machine-readable authorities are synchronized. Historical source material may retain its original wording only when explicitly classified as historical evidence.

**Tech Stack:** .NET 10.0.400, C# 14/net10.0, xUnit v3 + Microsoft Testing Platform, Python 3.13/pytest, CMake, GitHub Actions.

**Spec:** User directive in PR #1 development session: product/project identity is `Converty`.

## Global Constraints

- Product/display name is exactly `Converty`.
- Managed assemblies and namespaces use the `Converty.*` prefix.
- Active managed project/test directories and `.csproj` filenames use the `Converty.*` prefix.
- Root solution is `Converty.slnx`; active build scripts must not depend on the legacy solution name.
- Native/deferred/provider/package module paths use `Converty.*` names.
- Workspace ZIP naming becomes `Converty_<VERSION>_full_workspace.zip`.
- Current authority docs and machine-readable metadata use `Converty`; explicitly historical source material can preserve original wording.
- Wire schema field names, schema versions, format/provider IDs, behavior, security policy, warnings-as-errors, analyzers, and isolation rules do not change as part of the rebrand.
- All seven NuGet lock files must be regenerated/reviewed after project identity migration and must immediately survive locked restore.
- Final dev.5 evidence requires static gates, immutable Action-pin verification, locked restore, vulnerability audit, zero-warning Release build, all managed tests, native smoke, release-input verification, and release SBOM.

---

### Task 1: Add the stale-brand RED gate

**Files:**
- Create: `tests/static/test_dev5_converty_rebrand.py`

**Interfaces:**
- Consumes: repository filesystem only.
- Produces: a pytest gate that fails while active paths/content still use the legacy identity.

- [ ] **Step 1: Write tests requiring `Converty.slnx`, Converty project paths, Converty namespaces, Converty package naming, and absence of the legacy identity from active source/current authority files.**
- [ ] **Step 2: Run the static suite on the pre-rebrand branch and record the expected RED failures.**
- [ ] **Step 3: Do not weaken/exclude active files to make the test green; exclusions are limited to explicitly historical source/history documents.**

### Task 2: Rename the managed solution and active projects

**Files:**
- Replace: `FileConvert.slnx` -> `Converty.slnx`
- Replace active `src/FileConvert.*` project/module directories with `src/Converty.*`.
- Replace active `tests/FileConvert.*.Tests` directories with `tests/Converty.*.Tests`.
- Modify all active `.csproj`, `.cs`, `.slnx`, and build-script project references.

**Interfaces:**
- Produces assemblies/namespaces `Converty.Contracts`, `Converty.Core`, `Converty.Serialization`, `Converty.FakeProviders` and matching test assemblies.

- [ ] **Step 1: Rename paths and project references without changing domain behavior.**
- [ ] **Step 2: Replace namespace/import prefix with `Converty` in active C# source/tests.**
- [ ] **Step 3: Run static stale-brand gate; it must progress toward GREEN without adding suppressions.**
- [ ] **Step 4: Regenerate all seven lock files using the temporary lock workflow under SDK 10.0.400 and immediately verify locked restore.**

### Task 3: Rename native/deferred/provider/package identities

**Files:**
- Rename: `native/FileConvert.ShellExtension` -> `native/Converty.ShellExtension`
- Rename: `packaging/FileConvert.Package` -> `packaging/Converty.Package`
- Rename: `providers/FileConvert.Provider.FFmpeg` -> `providers/Converty.Provider.FFmpeg`
- Rename: `providers/FileConvert.Provider.Wic` -> `providers/Converty.Provider.Wic`
- Rename deferred `src/FileConvert.{Bridge,Host,Ipc,ProbeWorker,EngineWorker,Security,Settings}` module paths to `src/Converty.*`.
- Modify: `native/CMakeLists.txt`, module docs, repository verifier.

- [ ] **Step 1: Rename module paths and current module documentation.**
- [ ] **Step 2: Update CMake/repository-verifier path authorities.**
- [ ] **Step 3: Run CMake configure/build topology smoke.**

### Task 4: Rebrand current docs, package tooling, and machine-readable authority

**Files:**
- Rename: `docs/FileConvert_Master_Build_Plan.md` -> `docs/Converty_Master_Build_Plan.md`
- Modify current README/security/architecture/development/supply-chain/ADR/spec/handover docs.
- Modify package/SBOM/manifest scripts and current machine-readable authority.
- Regenerate: source/release SBOM and hash/package evidence as applicable.

- [ ] **Step 1: Replace active product references with `Converty` while preserving explicitly historical evidence.**
- [ ] **Step 2: Change workspace archive naming to `Converty_<VERSION>_full_workspace.zip`.**
- [ ] **Step 3: Make stale-brand static gate GREEN.**
- [ ] **Step 4: Run repository verifier and deterministic SBOM checks.**

### Task 5: Requalify dev.5 after identity migration

**Files:**
- Update: `VERSION`, `CHANGELOG.md`, current handover/status/backlog/evidence, PR description.
- Remove after locks are committed: `.github/workflows/dev5-generate-locks.yml` and the corresponding temporary upload-artifact pin.

- [ ] **Step 1: Run complete exact-head CI: Action pins, source SBOM, vectors, static suite, SDK 10.0.400, locked restore, vulnerability audit, Release build, all MTP/xUnit tests, native smoke.**
- [ ] **Step 2: Run release-input verification and release SBOM from real lock data.**
- [ ] **Step 3: Remove temporary lock-generation workflow/pin together and rerun exact-head CI.**
- [ ] **Step 4: Perform PR diff/security review, synchronize dev.5 evidence, make PR ready, merge only after final green evidence, and verify `main`.**
