# Converty Implementation Backlog

Check a box only when the stated deliverable has matching evidence.

## B0 Repository/bootstrap
- [x] Create solution/repository topology.
- [x] Pin .NET 10 SDK (`10.0.400`, `latestPatch`).
- [x] Native CMake/C++20 topology and hardening-policy scaffold.
- [ ] Full qualified Windows x64 Debug/Release production matrix — Release managed foundation is green; Debug and production native targets remain pending.
- [x] Nullable, warnings-as-errors/analyzers policy.
- [x] Dependency locking — all 15 current managed projects carry committed `packages.lock.json` files and locked restore passes.
- [x] CI Action provenance — all external workflow Actions full-SHA pinned with machine-readable authority.
- [x] CI execution containment — non-persistent checkout credentials, read-only permanent workflow permissions, finite job timeouts.
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

**B1 qualification:** preserved as regression authority inside the 108-test dev.6 managed suite.

## B2 Host/IPC — in progress
- [ ] Single-instance Host — per-user named-mutex lease primitive is implemented and tested; complete Host executable lifetime wiring remains open.
- [x] Explicit pipe DACL — protected current-user DACL and ACL-backed server creation are tested.
- [x] Peer validation — connected-client SID validation occurs before application-frame parsing and fails closed.
- [x] Framing + size/count/time limits — v1 fixed framing, 1 MiB frame ceiling, strict request-domain limits, cancellation, and finite Bridge connect timeout are tested; further end-to-end session-deadline policy can be tightened with final Host lifetime wiring.
- [ ] Bridge — bounded same-user client and strict acknowledgement validation are implemented/tested; Host startup/retry process behavior remains open.
- [ ] Bounded queue/journal — bounded in-memory queue is implemented/tested; persistent crash-safe atomic journal remains open.
- [x] Cancellation/status — queued job lookup/cancellation semantics are implemented/tested in Host.
- [x] IPC fuzz harness — seven checked-in adversarial cases execute against the real codec/request-admission path.

**B2 tranche evidence:** 15-project locked restore PASS; vulnerability audit PASS with 0 vulnerable-result packages; Release build PASS with 0 warnings/errors; 108/108 managed tests PASS; native topology smoke PASS. B2 is **not fully closed** because the three partial items above remain.

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
