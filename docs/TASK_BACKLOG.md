# Converty Implementation Backlog

## Dev.18 closure / dev.19 next
- [x] Freeze dev.17 Audio mixed-valid/invalid batch isolation on exact `main` `8b2756910b58b678745e6fda89866ed3bf545474` with exact-main run `33349604621` and deterministic verified delivery.
- [x] Define dev.18 Image acceptance test-first with 92 prior static tests green and exactly 3 new RED assertions.
- [x] Exercise all advertised Image source extensions against all three fixed Image actions through packaged Bridge→Strict Worker/provider→FFmpeg.
- [x] Prove 24/24 Image conversions with codec/dimension verification, collision numbering, source/destination preservation and no partials.
- [x] Add repeated malformed/truncated Image rejection with deterministic exit 4 and transactional preservation.
- [x] Preserve all Audio single-file/source-matrix/mixed-batch behavior recursively.
- [ ] Synchronize dev.18 version-aligned generated authority, require branch zero-diff qualification, then exact-current-main CI + deterministic verified delivery.
- [ ] Dev.19: Image multi-file/mixed-valid-invalid failure-isolation acceptance and Image matrix closure before beginning Video expansion.

## B0 Repository/bootstrap
- [x] .NET 10.0.400 / C# 14 / C++20 topology, warnings-as-errors, analyzers, locked dependencies, immutable Action pins, vulnerability audit and deterministic workspace tooling.
- [x] Disposable build/test/media/package outputs constrained to excluded `artifacts/`, `bin/`, `obj` and cache paths; source tests remain organized under `tests/`.
- [ ] Final Debug/Release signed-production matrix and dependency/license/notices review.

## B1 Core contracts
- [x] Strict versioned contracts/schemas, capability/planner/output collision logic, typed JSON v1 adapters and adversarial validation.
- [x] Fixed typed Audio preset registry.
- [x] Existing fixed typed Image actions (PNG/JPEG/WebP) product-path qualified by dev.18.

## B2 Host/IPC — release hardening remains open
- [x] Single-instance Host runtime, current-user pipe DACL/SID validation, bounded framing/timeouts, fixed Host startup, crash-recovery queue/journal.
- [x] Development connected-server authentication and package identity qualification.
- [x] Status lookup, queued-only transactional cancellation and replay/disconnect/reconnect acceptance.
- [x] 12-case checked-in IPC adversarial corpus.
- [ ] Production signed-package B2 identity/authentication requalification.

## B3 Explorer
- [x] Real native `IExplorerCommand` DLL, package COM/context-menu registration, fixed typed product subcommands.
- [x] Native Audio and Image submenu actions map only to fixed typed preset IDs.
- [ ] Headed Windows 11 modern-menu acceptance with exact-build screenshots.
- [ ] Explorer crash/hang/failure headed matrix.

## B4 Containment
- [x] Private staging, strict AppContainer, Job Object termination/resource ceilings, no-network/outside-scope denial, finite output growth, explicit Strict/Compatibility without silent downgrade.

## B5–B8 Providers
- [x] Audio action/source/malformed/batch closure through strict product path.
- [x] Image 8-source × 3-action acceptance plus malformed/truncated rejection.
- [ ] Image multi-file/mixed-valid-invalid closure and additional malformed corpus if warranted by evidence.
- [ ] Production FFmpeg/ffprobe pin/signature/hash/licensing/redistribution approval.
- [ ] Video action/source/malformed/batch qualification after Image closure.

## B9 UX/settings
- [ ] Defaults, mixed-selection behavior, pinned presets, output/concurrency/isolation settings and user-visible progress/result UX.

## B10 Plugin SDK
- [ ] Manifest/API/signature/hash gate and worker-only non-media sample provider.

## B11–B12 Release
- [ ] Final fuzz/chaos/security/static gates.
- [ ] Signed production MSIX and clean Windows 11 VM install/update/uninstall.
- [ ] Final SBOM/notices/hash manifest and headed end-user acceptance.

Check a box only when matching evidence exists. Product-first development ordering does not waive final shipping requirements.
