# Converty Implementation Backlog

Check a box only when the stated deliverable has matching evidence.

## B0 Repository/bootstrap
- [x] Create solution/repository topology.
- [x] Pin .NET 10 SDK (`10.0.400`, `latestPatch`).
- [x] Native CMake/C++20 topology and hardening-policy scaffold.
- [ ] Full qualified Windows x64 Debug/Release production matrix — Release managed foundation is green; Debug and production native targets remain pending.
- [x] Nullable, warnings-as-errors/analyzers policy.
- [x] Dependency locking — seven `packages.lock.json` files committed and locked restore passes.
- [x] CI Action provenance — all external workflow Actions full-SHA pinned with machine-readable authority.
- [x] CI execution containment — non-persistent checkout credentials, read-only workflow permissions, finite job timeouts.
- [x] Dependency vulnerability audit — real restored graph passes at `all`/`low`, zero vulnerable-result packages.
- [ ] Final release dependency/license/notices review — deterministic source/release SPDX tooling exists; final human/license release approval remains open.
- [x] Workspace/package SHA-256 manifest tooling.
- [ ] Release signing infrastructure and signed-artifact evidence.
- [x] Architecture/security/handover authority in repository.
- [x] Clean-checkout Windows managed restore/build/test verified in GitHub Actions.

## B1 Core contracts
- [x] Strict versioned request/preset/provider/job/plan/format JSON schemas.
- [x] Schema/domain limit alignment and embedded-NUL rejection.
- [x] Format/family/provider/preset IDs and validation — managed tests pass.
- [x] Capability graph — managed and seeded ordering tests pass.
- [x] Conversion planner — managed tests pass.
- [x] Fake Audio/Image/Video providers — managed tests pass.
- [x] Output naming/collision resolver — managed and seeded Unicode/collision tests pass.
- [x] Bounded property/adversarial path/name/collision suites — managed execution passes.
- [x] Typed JSON v1 serialization/migration adapters — managed/adversarial execution passes.

**B1 qualification:** 63/63 managed tests PASS on the qualified dev.5 head.

## B2 Host/IPC — next
- [ ] Single-instance Host.
- [ ] Explicit pipe DACL.
- [ ] Peer validation.
- [ ] Framing + size/count/time limits.
- [ ] Bridge.
- [ ] Bounded queue/journal.
- [ ] Cancellation/status.
- [ ] IPC fuzz harness.

**B2 start gate: PASS.** Begin in `0.1.0-dev.6`. No media parsing or engine execution belongs in B2.

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
- [ ] Dependency/static-analysis/security release gates pass.
- [ ] Clean VM install/update/uninstall.
- [ ] Signed package.
- [ ] Final SBOM + notices + release hash manifest.
- [ ] Final handover with exact evidence.
