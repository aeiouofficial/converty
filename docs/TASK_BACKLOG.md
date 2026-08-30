# Converty Implementation Backlog

## Dev.14 closure / next tranche
- [x] Dev.13 exact-main generated-authority and deterministic delivery freeze.
- [x] Add RED acceptance for ambiguous disconnect then admission replay on a fresh authenticated pipe connection.
- [x] Make admission replay idempotent by `requestId` without adding persistent-session architecture.
- [x] Qualify fresh-connection admission/status/cancel/status behavior.
- [x] GREEN behavior: 250/250 managed, 78/78 static, 5/5 vectors and product/package/COM gates PASS before expected authority freshness failure.
- [ ] Synchronize dev.14 generated authority from exact CI output and require exact-main zero-diff CI + deterministic verified delivery.
- [ ] Next unblocked product-first tranche: expand the fixed typed Audio conversion preset/action matrix through the existing Strict Worker/provider path; do not weaken containment or introduce raw FFmpeg passthrough.

## B0 Repository/bootstrap
- [x] .NET 10.0.400 / C# 14 / C++20 topology, warnings-as-errors, analyzers, locked dependencies, immutable Action pins, vulnerability audit and deterministic workspace tooling.
- [ ] Final Debug/Release signed-production matrix and dependency/license/notices review.

## B1 Core contracts
- [x] Strict versioned contracts/schemas, capability/planner/output collision logic, typed JSON v1 adapters and adversarial validation.
- [x] Dev.13 job-control contracts and strict serialization.

## B2 Host/IPC — release hardening remains open
- [x] Single-instance Host runtime, current-user pipe DACL/SID validation, bounded framing/timeouts, fixed Host startup, crash-recovery queue/journal.
- [x] Development connected-server authentication and package identity qualification.
- [x] Dev.13 status lookup and queued-only transactional cancellation wire.
- [x] Dev.14 replay/disconnect/reconnect one-shot acceptance.
- [x] 12-case checked-in IPC adversarial corpus.
- [ ] Production signed-package B2 identity/authentication requalification.

## B3 Explorer
- [x] Real native `IExplorerCommand` DLL, package COM/context-menu registration, fixed typed product subcommands.
- [ ] Headed Windows 11 modern-menu acceptance with exact-build screenshots.
- [ ] Explorer crash/hang/failure headed matrix.

## B4 Containment
- [x] Private staging, strict AppContainer, Job Object termination/resource ceilings, no-network/outside-scope denial, finite output growth, explicit Strict/Compatibility without silent downgrade.

## B5–B8 Providers
- [x] Development Audio MVP: WAV/supported audio → MP3 320k through Bridge→EngineWorker→FFmpeg, ffprobe-verified.
- [ ] Expand fixed typed Audio conversion preset/action matrix through existing worker/provider boundaries.
- [ ] Production FFmpeg/ffprobe pin/signature/hash/licensing/redistribution approval.
- [ ] Final Audio matrix plus Image/Video providers and malformed corpora.

## B9 UX/settings
- [ ] Defaults, mixed-selection behavior, pinned presets, output/concurrency/isolation settings and user-visible progress/result UX.

## B10 Plugin SDK
- [ ] Manifest/API/signature/hash gate and worker-only non-media sample provider.

## B11–B12 Release
- [ ] Final fuzz/chaos/security/static gates.
- [ ] Signed production MSIX and clean Windows 11 VM install/update/uninstall.
- [ ] Final SBOM/notices/hash manifest and headed end-user acceptance.

Check a box only when matching evidence exists. ADR-013 changes development ordering only; it does not waive final shipping requirements.
