# Converty Implementation Backlog

## Dev.11 current priority — 2026-08-29
- Development B2 Explorer→Bridge package identity: QUALIFIED.
- Development B2 Bridge→Host connected-server authentication: QUALIFIED.
- CURRENT: regenerate/freeze dev.11 generated authority after source/version synchronization.
- OPEN: headed Win11 UI, production signed-package B2 requalification, status/cancel + session acceptance, FFmpeg redistribution approval, signed MSIX/clean-VM lifecycle, final release audit.

Check a box only when the stated deliverable has matching evidence. ADR-013 changes development ordering only; it does not waive final shipping requirements.

## B0 Repository/bootstrap
- [x] Create solution/repository topology.
- [x] Pin .NET 10 SDK (`10.0.400`, `latestPatch`).
- [x] Native CMake/C++20 topology and hardening policy.
- [ ] Full qualified Windows x64 Debug/Release production matrix — Release managed/native product path is green; Debug/final signed-production matrix remains open.
- [x] Nullable, warnings-as-errors/analyzers policy.
- [x] Dependency locking — 18 current managed projects carry committed `packages.lock.json` files and locked restore passes.
- [x] Immutable CI Action provenance and read-only permanent workflow containment.
- [x] Dependency vulnerability audit — zero vulnerable-result packages at configured all/low gate.
- [ ] Final release dependency/license/notices review.
- [x] Workspace/package SHA-256 manifest tooling.
- [ ] Release signing infrastructure and signed-artifact evidence.
- [x] Architecture/security/handover authority in repository.

## B1 Core contracts
- [x] Strict versioned request/preset/provider/job/plan/format JSON schemas.
- [x] Schema/domain limit alignment and embedded-NUL rejection.
- [x] Format/family/provider/preset IDs and validation.
- [x] Capability graph and conversion planner.
- [x] Fake Audio/Image/Video providers.
- [x] Output naming/collision resolver and bounded adversarial coverage.
- [x] Typed JSON v1 serialization/migration adapters.

## B2 Host/IPC — still open for shipping hardening
- [x] Single-instance Host runtime.
- [x] Explicit current-user pipe DACL.
- [x] Connected-client SID validation before application-frame parsing.
- [x] Bounded/versioned framing, count/size/time limits and cancellation.
- [x] Trusted fixed Host startup and bounded one-launch Bridge retry.
- [x] Bounded persistent crash-recovery queue/journal.
- [x] Host status/cancellation semantics and IPC adversarial corpus.
- [ ] Final connected-server anti-squatting/authentication acceptance under the chosen package/signing authority.
- [ ] Final status/cancel wire-surface decision/qualification.
- [ ] Final replay/disconnect/reconnect/session acceptance matrix.

ADR-013 allowed the functional Explorer/FFmpeg product path before final B2 closure. The remaining B2 release gates are not waived.

## B3 Explorer
- [x] Real native `IExplorerCommand` DLL compiled under MSVC Release with hardening.
- [x] Package manifest COM/context-menu registration accepted by MakeAppx and exercised through loose package registration.
- [x] Fixed typed/pinned product subcommands with no raw FFmpeg command surface in Explorer.
- [ ] **Headed Windows 11 modern-menu acceptance** — real interactive Explorer/right-click UI with exact-build screenshots/evidence is still required.
- [ ] Explorer crash/hang/failure headed matrix.

Automated dev.10 evidence retains direct staged DLL class-factory/Invoke and loose-package COM activation/Invoke PASS on Windows Server 2025; this is not headed Windows 11 UI evidence.

## B4 Containment — behavior-qualified in dev.10
- [x] Private per-job worker staging.
- [x] Restricted worker launcher.
- [x] Job Object kill-on-close and descendant cleanup.
- [x] Memory/CPU/process/output/time ceilings.
- [x] Zero-capability AppContainer strict-launch qualification.
- [x] No-network strict canary.
- [x] Outside-scope file-write strict canary.
- [x] Explicit Strict vs Compatibility profile with no silent downgrade.
- [x] Move FFmpeg execution from the dev.9 Core/Bridge spike into fixed EngineWorker/provider architecture.

Behavior authority: `f221563c790057344a94b4e60c309d4512a77c38`, run `33028554361`. The executable Windows suite includes the 64 KiB output-growth termination canary; the earlier direct strict-canary run `33027104465` proves staging-only filesystem access, loopback-network denial, and descendant/resource containment.

## B5–B8 Providers
- [ ] Production FFmpeg/ffprobe provider pin/signature/hash/licensing/redistribution qualification — **development archive hash/execution verification is PASS only**.
- [x] Development strict-worker Audio MVP: WAV→MP3 320k through real Bridge→EngineWorker→FFmpeg path, ffprobe-verified.
- [ ] Final worker-contained Audio matrix/presets beyond the MVP.
- [ ] Image provider/matrix/security limits.
- [ ] Video provider/remux/transcode matrix.
- [ ] Provider-specific malformed/fuzz corpora.

## B9 UX/settings
- [ ] Per-family defaults.
- [ ] Mixed-selection defaults.
- [ ] User-configurable pinned presets.
- [ ] Output/collision/metadata/concurrency/isolation settings.
- [ ] User-visible progress/result/diagnostic UX appropriate for long conversions.

## B10 Plugin SDK
- [ ] Manifest contract.
- [ ] API/signature/hash gate.
- [ ] Worker-only plugin code.
- [ ] Non-media sample provider proves no core/shell fork.

## B11–B12 Release
- [ ] Fuzz + chaos matrix passes.
- [ ] Dependency/static-analysis/security release gates pass.
- [ ] Clean Windows 11 VM install/update/uninstall.
- [ ] Signed production package.
- [ ] Final SBOM + notices + release hash manifest.
- [ ] Headed end-user conversion acceptance from installed signed build.
- [ ] Final shipping handover/release evidence.
