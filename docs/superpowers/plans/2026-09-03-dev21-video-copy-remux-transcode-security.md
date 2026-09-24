# Dev.21 Video Copy / Remux / Transcode Security Implementation Plan

> **For agentic workers:** REQUIRED: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Implement the approved dev.21 B8 Video Copy/Remux/Transcode planner with a strict bounded ffprobe boundary, provider-owned FFmpeg token compilation, managed byte-exact Copy, post-probe target authorization, and no regression of dev.20 publication/isolation semantics.

**Architecture:** Preserve Explorer -> Bridge -> private staging -> disposable strict worker/provider -> fixed app-local engine -> validated transactional publication. Add a purpose-specific read-only `Converty.ProbeWorker` that translates fixed `ffprobe.exe` output into strict `MediaProbeResultV1`; Core uses those facts through `VideoPlanningPolicy`; `Converty.Provider.FFmpeg` alone owns `(PresetId, ConversionMode)` token vectors; Copy is managed bytes + SHA-256; Remux/Transcode require post-probe `TargetMediaContract` validation before publication.

**Tech Stack:** .NET/C# (`net10.0` repo toolchain), xUnit, Windows Job/AppContainer worker containment, `System.Text.Json`, SHA-256, fixed app-local FFmpeg/ffprobe 9.0.1 development qualification input, Python static gates, PowerShell packaged acceptance, GitHub Actions.

**Approved design authority:** `docs/superpowers/specs/2026-09-03-dev21-video-copy-remux-transcode-security-design.md` at commit `d80cc33a2e7c38738f113856e95f1451fd2df1b0`.

**Frozen predecessor authority:** `main@8a1f46603aa842728247bc11b34fcccf121858fd`, tree `4bd6f8d7acbadd60a3488870c773d2eafd67ba26`, exact-main CI `33671671714` SUCCESS. Do not rewrite or reinterpret dev.20 evidence.

---

## Execution invariants

1. TDD is mandatory: create the smallest failing RED witness before every material behavior change; preserve failed run evidence; then implement only enough GREEN to satisfy the witness.
2. Before every GitHub write, re-read live `main` and the branch head. Never force-update refs.
3. Do not hand-edit `machine-readable/package_manifest.json`, `SHA256SUMS.txt`, generated SBOM authority, or other deterministic generated authority. Synchronize only through the repo's guarded generation workflow after behavior/spec/plan stabilizes.
4. No shell command construction, PATH/CWD engine lookup, raw argument pass-through, arbitrary executable/plugin discovery, ordinary conversion network dependency, silent Strict->Compatibility fallback, or hardware acceleration.
5. Video input failures are member-local and preserve mixed-batch continuation. Infrastructure/trust failures remain fail-closed.
6. `Copy` must never launch FFmpeg. `Remux`/`Transcode` output must never publish based only on process exit 0.
7. Do not claim production ffmpeg/ffprobe redistribution approval, signed MSIX/B2 completion, headed Explorer acceptance, or customer ship-readiness from dev.21 engineering evidence.

## Task 1: RED — strict bounded probe contracts

**Files:**
- Create: `src/Converty.Contracts/Conversion/MediaProbeFactsV1.cs`
- Create: `src/Converty.Contracts/Conversion/MediaProbeResultV1.cs`
- Create: `src/Converty.Contracts/Conversion/MediaStreamFactsV1.cs`
- Create: `src/Converty.Contracts/Conversion/MediaProbeIds.cs`
- Modify: `src/Converty.Contracts/Conversion/ProbedFileDescriptor.cs`
- Create: `tests/Converty.Contracts.Tests/Conversion/MediaProbeFactsTests.cs`

**Step 1 — write failing boundary tests.** Assert immutable bounded construction for stream count exactly max/max+1, string/value bounds, negative/overflow dimensions/sample rates/channel counts, duplicate stream indexes, explicit `Unknown`, and additive `ProbedFileDescriptor` media facts while the existing four-argument constructor remains source-compatible.

**Step 2 — run RED.**

```powershell
dotnet test tests/Converty.Contracts.Tests/Converty.Contracts.Tests.csproj --no-restore --filter "FullyQualifiedName~MediaProbeFactsTests"
```

Expected: FAIL because the V1 probe contract types do not yet exist.

**Step 3 — commit RED evidence.** Commit tests only (or tests plus compile-only references required to expose the intended API), keeping production behavior unchanged.

**Step 4 — implement minimal GREEN contract types.** Use closed enums/IDs plus `Unknown`; no raw backend dictionaries/text; hard maximum constants live with the contract type; collections become read-only snapshots. Add a five-argument `ProbedFileDescriptor(..., MediaProbeFactsV1? mediaFacts)` overload and keep the existing constructor delegating with null facts.

**Step 5 — run GREEN plus existing contract tests.**

```powershell
dotnet test tests/Converty.Contracts.Tests/Converty.Contracts.Tests.csproj --no-restore
```

Expected: all contract tests PASS.

**Step 6 — commit GREEN.**

## Task 2: RED/GREEN — strict `MediaProbeResultV1` JSON codec

**Files:**
- Modify: `src/Converty.Serialization/ContractJson.cs`
- Modify: `src/Converty.Serialization/V1/WireModels.cs`
- Modify: `src/Converty.Serialization/V1/WireEnumText.cs`
- Create: `tests/Converty.Serialization.Tests/MediaProbeResultJsonTests.cs`
- Extend if needed: `tests/Converty.Serialization.Tests/AdversarialJsonTests.cs`

**Step 1 — RED tests.** Cover valid minimal success/failure, future schema, missing required member, duplicate property, unknown/extra property, trailing JSON, malformed/truncated JSON, max/max+1 string/stream boundaries, numeric overflow/extreme dimensions, contradictory HDR/color state, unknown-vs-missing distinction, and no raw ffprobe tags/metadata payload surface.

**Step 2 — run RED.**

```powershell
dotnet test tests/Converty.Serialization.Tests/Converty.Serialization.Tests.csproj --no-restore --filter "FullyQualifiedName~MediaProbeResultJsonTests"
```

Expected: FAIL because probe-result serialization is absent.

**Step 3 — GREEN implementation.** Add explicit V1 wire models and strict parsing with duplicate/unknown/trailing rejection. Do not enable permissive global JSON options that weaken existing contracts.

**Step 4 — run full serialization suite.**

```powershell
dotnet test tests/Converty.Serialization.Tests/Converty.Serialization.Tests.csproj --no-restore
```

Expected: PASS.

**Step 5 — commit.**

## Task 3: RED/GREEN — bounded worker stdout and purpose-specific filesystem scope

**Files:**
- Modify: `src/Converty.Security/Workers/WorkerProcessLaunchRequest.cs`
- Modify: `src/Converty.Security/Workers/WorkerProcessResult.cs`
- Modify: `src/Converty.Security/Workers/WindowsWorkerProcessLauncher.cs`
- Modify: `src/Converty.Security/Workers/WorkerFileSystemScope.cs`
- Modify as needed: `src/Converty.Security/Workers/WindowsAclGrant.cs`
- Create/extend: `tests/Converty.Security.Tests/Workers/WorkerStandardOutputLimitTests.cs`
- Extend: `tests/Converty.Security.Tests/Workers/WorkerFileSystemScopeTests.cs`
- Extend: `tests/Converty.Security.Tests/Workers/WindowsWorkerStrictIsolationCanaryTests.cs`
- Modify if required for deterministic canaries: `tests/Converty.WorkerCanary/Program.cs`

**Step 1 — RED tests.** Add independent stdout/stderr budgets. Exactly maximum stdout bytes succeeds; max+1 terminates the Job and throws/fails closed. Timeout/cancel/output overflow kills the complete Job and leaves no descendant. Add a read-only-input scope mode proving probe input readable but not writable, profile/Documents unreadable, and no write outside authorized paths.

**Step 2 — run RED.**

```powershell
dotnet test tests/Converty.Security.Tests/Converty.Security.Tests.csproj --no-restore --filter "FullyQualifiedName~WorkerStandardOutputLimitTests|FullyQualifiedName~WorkerFileSystemScopeTests|FullyQualifiedName~WindowsWorkerStrictIsolationCanaryTests"
```

Expected: FAIL because the launcher currently captures stderr only and the filesystem scope is conversion-RW shaped.

**Step 3 — GREEN implementation.** Extend `WorkerProcessLaunchRequest` with a separate bounded stdout budget without breaking existing callers; extend `WorkerProcessResult` with bounded stdout. Capture stdout incrementally; never unlimited `ReadToEnd` then validate size. Introduce explicit purpose/access semantics for exact-file read-only probe scope while retaining current EngineWorker staging RW behavior.

**Step 4 — run security suite.**

```powershell
dotnet test tests/Converty.Security.Tests/Converty.Security.Tests.csproj --no-restore
```

Expected: PASS, including existing containment tests.

**Step 5 — commit.**

## Task 4: RED/GREEN — fixed app-local ProbeWorker and ffprobe adapter

**Files:**
- Create: `src/Converty.ProbeWorker/Converty.ProbeWorker.csproj`
- Create: `src/Converty.ProbeWorker/Program.cs`
- Update: `src/Converty.ProbeWorker/MODULE.md`
- Create: `providers/Converty.Provider.FFmpeg/TrustedFfprobePath.cs`
- Create: `providers/Converty.Provider.FFmpeg/FfprobeProcessLauncher.cs`
- Modify: `Converty.slnx`
- Modify project references as minimally required
- Create: `tests/Converty.Bridge.Tests/Workers/ProbeWorkerClientTests.cs`
- Create: `src/Converty.Bridge/Workers/ProbeWorkerClient.cs`
- Create: `src/Converty.Core/Execution/IMediaProbeClient.cs`

**Step 1 — RED tests.** Require fixed `Converty.ProbeWorker.exe`, fixed app-local `ffprobe.exe`, no PATH/CWD/user binary fallback, only fully-qualified staged input, read-only scope, zero network capability, bounded stdout/stderr, Unicode/metachar paths inert, malformed/oversized worker output reject, timeout/cancel/overflow fail closed.

**Step 2 — run RED.**

```powershell
dotnet test tests/Converty.Bridge.Tests/Converty.Bridge.Tests.csproj --no-restore --filter "FullyQualifiedName~ProbeWorkerClientTests"
```

Expected: FAIL because client/worker/ffprobe adapter do not exist.

**Step 3 — GREEN implementation.** ProbeWorker accepts only one fixed typed input surface; invokes fixed ffprobe directly; parses raw ffprobe JSON only inside the worker; emits only strict `MediaProbeResultV1`. Provider launcher uses a fixed allowlisted token vector and local-file protocol posture. No backend/user text is re-emitted unbounded.

**Step 4 — run affected suites.**

```powershell
dotnet test tests/Converty.Bridge.Tests/Converty.Bridge.Tests.csproj --no-restore
dotnet test tests/Converty.Serialization.Tests/Converty.Serialization.Tests.csproj --no-restore
dotnet test tests/Converty.Security.Tests/Converty.Security.Tests.csproj --no-restore
```

Expected: PASS.

**Step 5 — commit.**

## Task 5: RED/GREEN — move all FFmpeg token ownership behind provider

**Files:**
- Modify: `src/Converty.Core/Presets/ProductPresetDefinition.cs`
- Modify: `src/Converty.Core/Presets/ProductPresetRegistry.cs`
- Create: `providers/Converty.Provider.FFmpeg/FfmpegPresetCompiler.cs`
- Modify: `providers/Converty.Provider.FFmpeg/FfmpegProcessLauncher.cs`
- Modify: `tests/Converty.Core.Tests/Presets/ProductPresetRegistryTests.cs`
- Modify/create provider assertions under the existing test project that references `Converty.Provider.FFmpeg`
- Create: `tests/static/test_dev21_video_planner_security.py`

**Step 1 — RED assertions.** Core preset definitions expose only product/menu/extensions/output semantics. Static test rejects FFmpeg syntax/tokens in `src/Converty.Core/Presets`. Provider compiler accepts only exact supported `(PresetId, ConversionMode)` tuples and produces immutable known tokens. Unsupported tuple rejects before process start. No caller-provided arbitrary token surface.

**Step 2 — run RED.**

```powershell
python -m pytest -q tests/static/test_dev21_video_planner_security.py
dotnet test tests/Converty.Core.Tests/Converty.Core.Tests.csproj --no-restore --filter "FullyQualifiedName~ProductPresetRegistryTests|FullyQualifiedName~Ffmpeg"
```

Expected: FAIL because Core currently owns `FfmpegArgumentsAfterInput` and `BuildFfmpegArguments`.

**Step 3 — GREEN migration.** Remove `_ffmpegArgumentsAfterInput`, `FfmpegArgumentsAfterInput`, and `BuildFfmpegArguments` from `ProductPresetDefinition`; retain menu/product semantics. Implement closed provider compiler. Include explicit stream mapping, `file` protocol policy, metadata/chapter stripping, fixed codec/pixel/audio profiles, and no hardware-acceleration tokens.

**Step 4 — run affected tests/static gates.** Expect PASS without changing Audio/Image advertised behavior.

**Step 5 — commit.**

## Task 6: RED/GREEN — deterministic `VideoPlanningPolicy`

**Files:**
- Create: `src/Converty.Core/Planning/VideoExecutionDecision.cs`
- Create: `src/Converty.Core/Planning/VideoPlanningReasonCode.cs`
- Create: `src/Converty.Core/Planning/VideoPlanningPolicy.cs`
- Modify minimally: `src/Converty.Core/Planning/PlanningRequest.cs`
- Modify minimally: `src/Converty.Core/Planning/ConversionPlanner.cs`
- Create: `tests/Converty.Core.Tests/Planning/VideoPlanningPolicyTests.cs`
- Extend: `tests/Converty.Core.Tests/Planning/ConversionPlannerTests.cs`

**Step 1 — RED table tests.** Implement explicit witnesses for MP4 Copy/Remux/Transcode, WebM Copy/Remux/Transcode, MP3 extract Remux/Transcode, plus reject cases: unsupported/unknown codec/container/required fact, multiple primary A/V, subtitle/data/attachment for Video target, HDR/high-bit-depth, missing audio extraction, unqualified codecs. Reason codes must be deterministic and bounded.

**Step 2 — run RED.**

```powershell
dotnet test tests/Converty.Core.Tests/Converty.Core.Tests.csproj --no-restore --filter "FullyQualifiedName~VideoPlanningPolicyTests|FullyQualifiedName~ConversionPlannerTests"
```

Expected: FAIL because Video stream-aware policy is absent.

**Step 3 — GREEN.** Reuse existing `ConversionMode`, `ConversionPlan.Mode`, `CapabilityGraph`, and generic planner. Do not create a second planner hierarchy. Video policy consumes only typed probe facts + target/preset intent.

**Step 4 — run full Core tests.** Expected PASS.

**Step 5 — commit.**

## Task 7: RED/GREEN — mode-aware EngineWorker and managed Copy

**Files:**
- Modify: `src/Converty.Core/Execution/IConversionWorkerClient.cs`
- Modify: `src/Converty.Bridge/Workers/EngineWorkerClient.cs`
- Modify: `src/Converty.EngineWorker/Program.cs`
- Modify: provider compiler/launcher files from Task 5
- Add/extend worker/provider tests in `tests/Converty.Core.Tests/Execution` and `tests/Converty.Bridge.Tests/Workers`

**Step 1 — RED tests.** EngineWorker surface is exactly `--preset --mode --input --output`. Copy launches no FFmpeg and performs managed byte copy. Require staged-input/output SHA-256 equality. Remux/Transcode call only provider-compiled tokens. Unsupported mode/preset tuple rejects before engine start.

**Step 2 — run RED.** Expected failure on missing mode/copy semantics.

**Step 3 — GREEN implementation.** Add bounded `ConversionMode` input; managed Copy with SHA-256 verification; provider execution only for Remux/Transcode.

**Step 4 — run affected Bridge/Core tests.** Expected PASS.

**Step 5 — commit.**

## Task 8: RED/GREEN — stage -> probe -> plan -> execute -> post-validate -> publish

**Files:**
- Modify: `src/Converty.Core/Execution/ConversionBatchRunner.cs`
- Create: `src/Converty.Core/Execution/TargetMediaContract.cs`
- Create: `src/Converty.Core/Execution/TargetMediaContractValidator.cs`
- Extend: `tests/Converty.Core.Tests/Execution/ConversionBatchRunnerTests.cs`
- Extend: `tests/Converty.Core.Tests/Execution/ConversionBatchIsolationTests.cs`

**Step 1 — RED tests.** Video member flow probes staged input, plans, executes, post-probes staged output, validates exact target contract, then publishes. Engine exit 0 + wrong container/codec/topology/pixfmt/audio/HDR => no publication. Post-probe failure/timeout/corrupt output => no publication. Copy hash mismatch => no publication. Later valid members still run after member-local malformed/unsupported members.

**Step 2 — run RED.** Expected FAIL on missing probe/planning/post-validation path.

**Step 3 — GREEN integration.** Inject `IMediaProbeClient` and Video planner/target validation with additive constructor compatibility where practical. Preserve Audio/Image current execution path byte-for-byte/behaviorally except for provider-token ownership plumbing already covered by regression tests.

**Step 4 — run full Core/Bridge managed suites.** Expected PASS.

**Step 5 — commit.**

## Task 9: RED/GREEN — package ProbeWorker + ffprobe and qualify strict descendants

**Files:**
- Modify: `build/prepare-dev-ffmpeg.ps1`
- Modify: `build/stage-dev-package.ps1`
- Modify: `build/validate-dev-package.ps1`
- Modify: `.github/workflows/ci.yml` only as required by the new qualification gates
- Extend: `tests/Converty.Security.Tests/Workers/WindowsWorkerStrictIsolationCanaryTests.cs`
- Extend static dev.21 gate
- Add a dedicated packaged dev.21 Video smoke script if keeping dev.20 scripts immutable is clearer than expanding them

**Step 1 — RED package/static/canary gates.** Require fixed package locations for `Converty.ProbeWorker.exe` and `ffprobe.exe`, reparse rejection, actual ffprobe/ffmpeg descendants inside strict Job/AppContainer, DNS/TCP denied, prohibited filesystem reads/writes denied, timeout/cancel/failure no orphans, no PATH fallback. Add exact-engine protocol/demuxer qualification without guessing demuxer names.

**Step 2 — run RED on Windows CI.** Preserve exact failing job/run evidence.

**Step 3 — GREEN packaging/containment.** Stage pinned development ffprobe beside ffmpeg from the same declared development authority. Close only the exact qualified `file` protocol + demuxer set. Do not claim production redistribution approval.

**Step 4 — rerun exact affected Windows/static gates.** Expected PASS before proceeding.

**Step 5 — commit.**

## Task 10: Packaged B8 qualification and complete regressions

**Files:**
- Extend/add packaged Video acceptance scripts and fixtures
- Extend `tests/static/test_dev21_video_planner_security.py`
- Update test docs only after executed evidence exists

**Step 1 — real packaged witnesses.** Prove at least one real Copy, Remux, and Transcode path for relevant actions; verify outputs with fixed ffprobe against `TargetMediaContract`.

**Step 2 — negative/mixed evidence.** Repeated malformed/truncated; twice-run mixed valid/invalid batch; unknown/unqualified/HDR/ambiguous streams; source preservation; existing destination preservation; Unicode/metachar paths; numbered no-overwrite; zero partial/orphan processes.

**Step 3 — full regressions.** Run the complete managed suite, all static tests, all raw vectors, dev.20 Video 27/27 matrix, Audio 36-case, Image 24-case, native/package registration/product bridge smokes.

**Step 4 — capture exact SHA/run/job evidence.** No PASS claims before reading completed GitHub Actions jobs.

**Step 5 — commit only non-generated evidence/docs that accurately reflect executed results.**

## Task 11: Governance/supply-chain hardening required by dev.21 acceptance

**Files:**
- `.github/workflows/ci.yml`
- dependency lock/requirements mechanism used by Python static gates
- `machine-readable/ci_action_pins.json` only through its intended authority process if generated
- repository ruleset via GitHub ruleset API/tooling, not source-code fiction

**Step 1 — RED governance/static assertions.** Hash-verified Python dependency installation; full-SHA action pins remain; no broad workflow permissions. Verify desired future freeze ruleset semantics: block force push/deletion, restrict updates, require exact checks/linear history where compatible, future verified signatures without merge-generated post-qualification SHA.

**Step 2 — implement minimal compatible hardening.** Do not retroactively rewrite unsigned dev.20.

**Step 3 — verify workflow and ruleset live state.** Record exact ruleset/status evidence.

**Step 4 — commit source-side CI changes and preserve live configuration evidence in cloud docs.**

## Task 12: Stabilize generated authority, exact-candidate qualification, freeze lifecycle

**Files:** generated authority only through guarded deterministic workflow; version/changelog/evidence files as required by verified dev.21 completion.

**Step 1 — once spec/plan/code/tests are stable, run ordinary CI generation and inspect deterministic diffs.**

**Step 2 — independently verify generated package/hash/SBOM authority.**

**Step 3 — synchronize generated authority only through the existing guarded exact-parent workflow. Never hand-edit.**

**Step 4 — require branch zero-diff/deterministic qualification and exact candidate CI GREEN.**

**Step 5 — independently verify final workspace/delivery artifacts.**

**Step 6 — only if every required gate is genuinely GREEN, non-force exact-SHA promote/freeze according to the existing protocol.**

**Step 7 — fresh-read exact `main` and CI after promotion.** No freeze claim until exact-main continuity/static/managed checks are all completed SUCCESS.

## Task 13: Documentation reconciliation and handover rotation

**Cloud docs:** Update in place: Project Authority, Roadmap, Current Plan, Open Tasks/Gates, Changelog, Release/Test Evidence, Recursive Handover. Update canonical Slack anchors/channels only; no duplicate current-state docs.

**Step 1 — reconcile GitHub/CI first.** GitHub remains code/release authority.

**Step 2 — record exact verified RED/GREEN commits, run/job/artifact IDs, tests, blockers, and explicitly unverified release claims.

**Step 3 — process `ACTIVE HANDOVER #3` only after a material verified implementation boundary and exact successor is ready.** Mark #3 PROCESSED first, then create exactly one context-free OPEN successor containing current repo/branch/SHA/version/CI/evidence, completed work, blockers, invariants, exact next task and acceptance criteria.

**Step 4 — verify there is exactly one OPEN Converty handover.**

---

## First execution slice

Begin with Tasks 1–3 only: bounded contracts, strict serialization, and bounded stdout/read-only worker security foundations. These form the minimum trust substrate required before ProbeWorker or planner behavior can safely exist. Do not jump directly to ffprobe or mode-selection behavior before these RED/GREEN foundations pass.

## Acceptance for the first slice

- RED evidence exists before corresponding GREEN implementation.
- Existing four-argument `ProbedFileDescriptor` callers remain valid.
- Strict `MediaProbeResultV1` rejects malformed/future/duplicate/extra/trailing/oversized/contradictory data.
- Worker stdout is independently streaming-bounded with exact-max/max+1 tests.
- Probe-style filesystem scope is exact-input read-only, EngineWorker staging RW behavior remains available, and timeout/cancel/overflow kills the complete Job with no orphan.
- Full affected Contracts/Serialization/Security suites pass on the exact GREEN commit.
- No Video production behavior, release authority, or ship-ready claim is introduced by this first slice.
