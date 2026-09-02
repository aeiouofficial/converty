# Converty Implementation Backlog

## Dev.19 Image mixed-batch closure
- [x] Preserve frozen dev.18 Image single-file authority and recursive Audio evidence.
- [x] Define dev.19 test-first contract for one same-family Image selection containing valid and malformed/truncated members.
- [x] Add focused core-runner Image batch isolation coverage.
- [x] Add real Windows packaged Image mixed-batch smoke and CI wiring.
- [ ] Complete final dev.19 qualification, generated-authority synchronization and exact-main freeze.
- [ ] Do not begin Video expansion until dev.19 is evidence-backed and frozen.

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
- [ ] Image mixed-valid/invalid multi-file closure; dev.19 acceptance is implemented and awaiting final authority qualification.
- [ ] Additional Image malformed corpus if evidence warrants it.
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

## Execution rule
Check a box only when matching evidence exists. Product-first development ordering does not waive final shipping requirements. Preserve all existing granular evidence keys when extending machine-readable authority; never hand-edit generated SBOM/package/hash artifacts.
