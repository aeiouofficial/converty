# Converty Implementation Backlog

Use this as the execution checklist. A box is checked only when the stated deliverable has matching evidence. Items marked **source authored; managed verification pending** remain unchecked intentionally.

## B0 Repository/bootstrap
- [x] Create solution/repository topology.
- [x] Pin .NET 10 SDK (`global.json` → `10.0.400`, `latestPatch`).
- [x] Pin native CMake/C++20 topology and hardening-policy function; Linux CMake smoke passes.
- [ ] Establish qualified Windows x64 Debug/Release build evidence.
- [x] Enable nullable, warnings-as-errors/analyzers policy and native MSVC hardening flags in source configuration.
- [ ] Dependency locking — generation is force-evaluated and immediately rechecked in locked mode, but actual `packages.lock.json` generation/review requires a .NET-capable environment.
- [x] CI Action provenance — every external workflow Action is full-SHA pinned with a machine-readable reviewed pin authority and executable verifier.
- [x] CI execution containment — checkout credentials are not persisted, workflow permissions are read-only, and static/managed jobs have finite 15/30 minute ceilings.
- [ ] Dependency vulnerability audit — explicit NuGet `all`/`low` audit policy, vulnerability-only audit source, CI command and fail-closed report verifier are implemented; real restored-graph evidence requires .NET 10.0.400.
- [ ] SBOM generation and review — deterministic SPDX 2.3 source mode is implemented and tested; release mode intentionally refuses missing managed lock files, so release dependency/license/vulnerability review is pending.
- [x] Workspace/package SHA-256 manifest tooling.
- [ ] Release-signing policy/plumbing and signing evidence — machine-readable policy, external key-custody rule, timestamp requirement, and workspace secret exclusions are implemented; actual signing infrastructure/signature evidence is pending.
- [x] Copy architecture/security/handover authority docs and reference images into repository.
- [ ] Clean-clone Windows managed build verified.

## B1 Core contracts
- [x] Strict versioned request/preset/provider/job/plan/format JSON schemas; executable schema tests pass.
- [x] Schema/domain limit alignment for path/display-name/extension/option bounds and embedded-NUL rejection; executable static/schema gates pass.
- [ ] Format/family/provider/preset IDs and validation — **source + xUnit/property tests authored; managed verification pending**.
- [ ] Capability graph — **source + xUnit + seeded ordering property test authored; managed verification pending**.
- [ ] Conversion planner — **source + xUnit authored; managed verification pending**.
- [ ] Fake providers for Audio/Image/Video — **source + xUnit authored; managed verification pending**.
- [ ] Output naming/collision resolver — **source + xUnit + seeded Unicode/collision property test authored; managed verification pending**.
- [ ] Property/fuzz tests for paths/names/collisions — **bounded seeded source authored; managed execution pending**.
- [ ] Typed JSON serialization/migration adapters — **v1 source + strict static gates + xUnit/adversarial tests authored; managed execution pending**.

## B2 Host/IPC
- [ ] Single-instance Host.
- [ ] Explicit pipe DACL.
- [ ] Peer validation.
- [ ] Framing + size/count limits.
- [ ] Bridge.
- [ ] Queue/journal.
- [ ] Cancellation/status.
- [ ] IPC fuzz harness.

**B2 start gate:** do not begin until the current dev.4 managed projects restore/build/test successfully with SDK 10.0.400 and reviewed lock files.

## B3 Explorer
- [ ] `IExplorerCommand` DLL.
- [ ] Package manifest COM/context-menu registration.
- [ ] Pinned subcommands.
- [ ] Modern-menu headed acceptance.
- [ ] Explorer crash/hang/failure matrix.

## B4 Containment
- [ ] Private staging.
- [ ] Worker launcher.
- [ ] Job Object kill-on-close.
- [ ] Memory/CPU/process/output/time ceilings.
- [ ] AppContainer/Win32 isolation qualification.
- [ ] No-network canary.
- [ ] Outside-scope file-write canary.
- [ ] Strict vs compatibility profile with no silent downgrade.

## B5–B8 Providers
- [ ] FFmpeg/ffprobe pinned and verified.
- [ ] Audio MVP WAV→MP3 320k.
- [ ] Audio matrix/presets.
- [ ] Image provider/matrix/security limits.
- [ ] Video provider/remux/transcode matrix.
- [ ] Provider-specific malformed/fuzz corpora.

## B9 UX/settings
- [ ] Per-family defaults.
- [ ] Mixed-selection defaults.
- [ ] Pinned presets.
- [ ] Output/collision/metadata/concurrency/isolation settings.

## B10 Plugin SDK
- [ ] Manifest contract.
- [ ] API/signature/hash gate.
- [ ] Worker-only plugin code.
- [ ] Non-media sample provider proves no core/shell fork.

## B11–B12 Release
- [ ] Fuzz + chaos matrix passes.
- [ ] Dependency/static-analysis/security gates pass.
- [ ] Clean VM install/update/uninstall.
- [ ] Signed package.
- [ ] SBOM + notices + release hash manifest.
- [ ] Final handover with exact evidence.
