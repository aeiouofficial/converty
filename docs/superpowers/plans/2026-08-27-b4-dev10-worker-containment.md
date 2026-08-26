# B4 dev.10 Worker Containment Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Move the dev.9 FFmpeg execution spike behind a disposable worker/provider boundary with private staging and enforceable Windows process containment while preserving the existing Explorer UX and numbered no-overwrite publication behavior.

**Architecture:** `Converty.Bridge` remains a bounded trigger/coordinator. `Converty.Core` owns typed presets, input/output policy, private staging orchestration and publication, but no FFmpeg process implementation. `Converty.EngineWorker` is a disposable worker that resolves a checked-in preset ID and invokes `providers/Converty.Provider.FFmpeg`; Bridge launches the worker through an explicit Windows isolation profile and the worker never receives the final destination path. Strict isolation fails closed and never silently falls back to compatibility.

**Tech Stack:** .NET 10.0.400 / C# 14, Windows 11 x64 Win32 process/job APIs, xUnit v3 + Microsoft Testing Platform, PowerShell CI/product smokes, Python static repository gates.

**Spec:** `docs/Converty_Master_Build_Plan.md` sections 9–12, `docs/SECURITY_THREAT_MODEL.md`, `docs/adr/ADR-013-dev9-functional-product-spike.md`.

## Global Constraints

- Explorer is trigger-only and performs no media parsing, probing, codec/plugin loading, FFmpeg execution, or network work.
- Host/Bridge never parse hostile media or load codec/plugin code.
- Production probing/conversion belongs in disposable restricted workers.
- Local strict conversion has no intended network access.
- Presets/IPC/Explorer never expose raw commands or pass-through FFmpeg argument vectors.
- Process creation never uses a shell.
- Numbered copy remains the default and input/external destinations are never overwritten.
- Strict isolation never silently downgrades to compatibility.
- Worker output is private staging until orchestration validates and publishes it.
- .NET SDK is exactly `10.0.400`; every managed project has a committed `packages.lock.json`.
- Report only gates actually executed; headed Windows 11 Explorer acceptance remains separate.

---

### Task 1: Private Per-Job Staging Transaction

**Files:**
- Modify: `src/Converty.Core/Execution/ConversionBatchRunner.cs`
- Create: `src/Converty.Core/Execution/ConversionStagingDirectory.cs`
- Modify: `tests/Converty.Core.Tests/Execution/ConversionBatchRunnerTests.cs`

**Interfaces:**
- Consumes: `ProductPresetDefinition`, `OutputPathResolver`, current conversion executor abstraction.
- Produces: a unique private job directory below a Converty-owned staging root; staged input/output paths are passed to execution; only the validated staged output is published to the final numbered destination.

- [ ] **Step 1: Write the failing private-staging regression**

Update the existing multi-file runner test so it requires the execution input/output paths to be outside the source directory, requires each staged input to preserve the source bytes while execution is active, and requires all staged paths/directories to be gone after `RunAsync` completes.

- [ ] **Step 2: Run RED gate**

Run: `./build/test.ps1 -Configuration Release`
Expected on the dev.9 implementation: FAIL because the launcher receives the original source path and a `.converty-*.partial.*` sibling beside it.

- [ ] **Step 3: Implement minimal staging transaction**

Create one unique job directory per source item under a Converty-owned local staging root. Copy the source file to a fixed data filename, allocate output only in that directory, execute against staged paths, verify staged output is non-empty, then use the existing race-safe `File.Move(..., overwrite: false)` publication loop. Delete only the owned job directory in `finally`.

- [ ] **Step 4: Run GREEN gate**

Run: `./build/test.ps1 -Configuration Release`
Expected: all managed tests PASS; no source or existing destination is modified.

- [ ] **Step 5: Commit**

Commit message: `feat: add private conversion staging`

---

### Task 2: Worker/Provider Boundary

**Files:**
- Create: `src/Converty.EngineWorker/Converty.EngineWorker.csproj`
- Create: `src/Converty.EngineWorker/Program.cs`
- Update: `src/Converty.EngineWorker/MODULE.md`
- Create: `providers/Converty.Provider.FFmpeg/Converty.Provider.FFmpeg.csproj`
- Move/replace responsibility from: `src/Converty.Core/Execution/FfmpegProcessLauncher.cs`, `TrustedFfmpegPath.cs`, `FfmpegExecutionResult.cs`, `IFfmpegProcessLauncher.cs`
- Update: `providers/Converty.Provider.FFmpeg/MODULE.md`
- Create: `src/Converty.Core/Execution/IConversionWorkerClient.cs`
- Create: `src/Converty.Core/Execution/ConversionWorkerResult.cs`
- Modify: `src/Converty.Core/Execution/ConversionBatchRunner.cs`
- Modify: `src/Converty.Bridge/Converty.Bridge.csproj`
- Modify: `Converty.slnx`
- Create/modify: managed tests and `tests/static/test_dev10_b4_worker_boundary.py`

**Interfaces:**
- Produces: `IConversionWorkerClient.ExecuteAsync(PresetId presetId, string stagedInputPath, string stagedOutputPath, TimeSpan timeout, CancellationToken cancellationToken)` returning exit/result data.
- `EngineWorker` accepts only a checked-in preset ID plus fully-qualified staged input/output paths supplied by the trusted launcher. It reconstructs FFmpeg arguments from `ProductPresetRegistry` inside the worker/provider and accepts no arbitrary argument vector.

- [ ] **Step 1: Write failing architecture tests**

Require EngineWorker/provider managed projects to exist, require Bridge/Core source to contain no `FfmpegProcessLauncher` instantiation or direct `ProcessStartInfo` for FFmpeg, and require the provider to be the only first-party module containing FFmpeg process execution.

- [ ] **Step 2: Run RED gates**

Run: `python -m pytest -q tests/static/test_dev10_b4_worker_boundary.py` and `./build/test.ps1 -Configuration Release`.
Expected: FAIL because dev.9 executes FFmpeg in Core and no worker/provider project exists.

- [ ] **Step 3: Implement worker/provider split**

Move trusted FFmpeg path verification and structured `ProcessStartInfo.ArgumentList` construction into the FFmpeg provider. Implement the EngineWorker argument parser as a fixed typed surface (`--preset`, `--input`, `--output`) with no extra argument forwarding. Core invokes only `IConversionWorkerClient`.

- [ ] **Step 4: Run GREEN gates**

Run both the static architecture test and full managed suite. Expected: PASS.

- [ ] **Step 5: Commit**

Commit message: `feat: move ffmpeg execution into worker provider`

---

### Task 3: Windows Disposable Worker Containment

**Files:**
- Create: `src/Converty.Security/Workers/WorkerIsolationLevel.cs`
- Create: `src/Converty.Security/Workers/WorkerResourceLimits.cs`
- Create: `src/Converty.Security/Workers/WindowsJobObject.cs`
- Create: `src/Converty.Security/Workers/WindowsRestrictedWorkerLauncher.cs`
- Create: `src/Converty.Bridge/Workers/EngineWorkerClient.cs`
- Create/modify: `tests/Converty.Security.Tests/Workers/*`
- Create/modify: `tests/Converty.Bridge.Tests/Workers/*`
- Create: `build/worker-containment-smoke.ps1`
- Modify: `.github/workflows/ci.yml`

**Interfaces:**
- `WorkerIsolationLevel.Strict` and `WorkerIsolationLevel.Compatibility` are explicit; strict launch failure throws and never retries in compatibility mode.
- `WorkerResourceLimits` carries finite wall-clock, memory, process-count, CPU and output ceilings.
- The Windows launcher creates the worker suspended, applies the selected restricted security context, assigns a Job Object before resume, sets kill-on-close plus process/memory/CPU limits, disables handle inheritance by default, and waits with a finite timeout.

- [ ] **Step 1: Write failing containment tests**

Test no-fallback profile selection, finite/validated ceilings, kill-on-close/process-count policy construction, and timeout/cancellation result mapping. Add an executable smoke canary that starts a purpose-built worker mode and proves descendants die when the job handle closes.

- [ ] **Step 2: Run RED gates**

Run: `./build/test.ps1 -Configuration Release` and `./build/worker-containment-smoke.ps1`.
Expected: FAIL because no restricted launcher/Job Object exists.

- [ ] **Step 3: Implement Job Object/resource containment**

Use Windows Job Objects with `JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE`, finite active-process and memory limits, CPU control where supported, explicit timeout/cancellation kill, bounded stdout/stderr, no shell execution, and explicit environment/working-directory policy. The compatibility profile remains an explicit policy choice and is never an automatic fallback.

- [ ] **Step 4: Implement/qualify strict no-network/filesystem profile**

Use the production-supported restricted/AppContainer mechanism selected by the repository threat model. Give the strict worker access only to required executable inputs and its private staging scope, with no network capabilities. Add canary modes that attempt network egress and an outside-scope write; the strict smoke must prove both are denied. If this mechanism cannot be qualified on the current Windows runner, keep strict fail-closed and leave the two canary release gates explicitly open rather than claiming success.

- [ ] **Step 5: Run GREEN gates**

Run managed tests, worker containment smoke, and product conversion smoke. Record separately which strict canaries actually executed and passed.

- [ ] **Step 6: Commit**

Commit message: `feat: contain disposable conversion worker`

---

### Task 4: Development Package and Product Regression

**Files:**
- Modify: `build/stage-dev-package.ps1`
- Modify: `build/product-conversion-smoke.ps1`
- Modify: `build/explorer-registration-smoke.ps1` only if required for worker staging
- Modify: `.github/workflows/ci.yml`
- Modify: project lock files generated by `.NET SDK 10.0.400`

**Interfaces:**
- The staged package contains Bridge, EngineWorker, provider dependencies and the pinned development-only `tools/ffmpeg/ffmpeg.exe` at fixed app-local paths.
- Product smoke invokes the same Bridge command used by Explorer and preserves Unicode/metacharacter filenames, source/base destination, numbered output and MP3 320000 bit/s verification.

- [ ] **Step 1: Write failing package/static assertions**

Require staged package scripts to include EngineWorker/provider artifacts and require Core/Bridge not to contain FFmpeg executable launch code.

- [ ] **Step 2: Run RED package/static gate**

Run relevant static test plus `./build/stage-dev-package.ps1` on Windows.

- [ ] **Step 3: Update staging/build scripts and regenerate lock files**

Run `./build/bootstrap.ps1 -GenerateLockFiles` with SDK exactly 10.0.400, review new lock files, then run locked restore.

- [ ] **Step 4: Run full product regression**

Run dependency audit, Release build, native Explorer smoke, pinned development FFmpeg prep, MakeAppx validation, direct/package COM invoke smokes, worker containment smoke, Bridge→worker→FFmpeg product smoke, and full managed/static tests.

- [ ] **Step 5: Commit**

Commit message: `test: qualify dev10 worker-contained product path`

---

### Task 5: dev.10 Authority Closure and Recursive Handover

**Files:**
- Modify: `VERSION`
- Modify: `README.md`
- Modify: `CHANGELOG.md`
- Modify: `docs/TASK_BACKLOG.md`
- Modify: `docs/development/IMPLEMENTATION_STATUS.md`
- Modify: `docs/SECURITY_THREAT_MODEL.md`
- Modify: `docs/HANDOVER_NEXT_AGENT.md`
- Modify: `docs/HANDOVER_PROMPT.txt`
- Modify: `machine-readable/handover_state.json`
- Modify: `machine-readable/build_evidence.json`
- Regenerate: `machine-readable/source_sbom.spdx.json`, `machine-readable/release_sbom.spdx.json`, `machine-readable/package_manifest.json`, `SHA256SUMS.txt`

**Interfaces:**
- Authority distinguishes the immutable dev.9 qualification from the new dev.10 behavior qualification and later generated-authority closure SHA/run IDs.
- Unexecuted headed Windows 11 Explorer, signing, production FFmpeg licensing/provenance, clean-VM and any unqualified strict canary remain explicitly open.

- [ ] **Step 1: Synchronize version/docs/evidence only from executed gates**

Set workspace version to `0.1.0-dev.10`; record exact behavior SHA and GitHub Actions run/job IDs only after they exist.

- [ ] **Step 2: Regenerate authority and require zero diff**

Run source/release SBOM generation, package manifest generation and SHA-256 manifest generation; require `git diff --exit-code` for generated authority on the frozen candidate.

- [ ] **Step 3: Build workspace ZIP twice and verify**

Use `scripts/package_workspace.py` twice, require byte-identical archives, reopen the candidate, run CRC, verify every package-manifest and SHA manifest entry, and enforce exclusions.

- [ ] **Step 4: Final exact-head CI qualification**

Run the permanent GitHub Actions workflow on the final candidate and retain managed/static job IDs plus verified-delivery artifact metadata.

- [ ] **Step 5: Update main and prepare recursive handover**

Fast-forward/merge only after the evidence-supported candidate is green. The handover must include exact repository/branch/main HEAD, immutable qualification SHA/run/job IDs, changes, executed results, remaining blockers/unverified claims, single highest-priority next task, all non-negotiables, and this same recursive handover rule.
