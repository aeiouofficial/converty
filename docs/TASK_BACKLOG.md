# Converty Implementation Backlog

## Dev.16 closure / dev.17 next
- [x] Preserve frozen dev.15 exact-main authority and normal Explorer→Bridge→Strict Worker→FFmpeg routing.
- [x] Freeze dev.15 on exact `main` `dc46bd4dd25fe672f1695a0895cdb06152a743a7` with exact-main run `33339019327` and verified deterministic delivery.
- [x] Add dedicated Audio source-format acceptance for WAV/FLAC/MP3/M4A/Ogg/Opus against all six fixed Audio actions.
- [x] Prove 36/36 product-path conversions with Unicode/metacharacter filenames, source/destination preservation, numbered publication, no partials and ffprobe codec checks.
- [x] Add repeated malformed/truncated negative acceptance.
- [x] Diagnose and fix the noninteractive Bridge modal-blocking defect without changing Explorer's default modal error UI.
- [x] Prove malformed/truncated deterministic exit code 4, source/destination preservation, no publication and no partial residue.
- [x] Preserve RED/GREEN evidence and pre-authority deterministic workspace evidence for dev.16.
- [ ] Synchronize dev.16 version-aligned generated authority and require branch zero-diff qualification plus exact-current-main CI + deterministic verified delivery.
- [ ] Dev.17: final Audio multi-file/mixed-valid-invalid batch failure isolation and matrix closure before beginning Image/Video expansion.

## B0 Repository/bootstrap
- [x] .NET 10.0.400 / C# 14 / C++20 topology, warnings-as-errors, analyzers, locked dependencies, immutable Action pins, vulnerability audit and deterministic workspace tooling.
- [x] Disposable build/test/media/package outputs constrained to excluded `artifacts/`, `bin/`, `obj/` and cache paths; source tests remain organized under `tests/`.
- [ ] Final Debug/Release signed-production matrix and dependency/license/notices review.

## B1 Core contracts
- [x] Strict versioned contracts/schemas, capability/planner/output collision logic, typed JSON v1 adapters and adversarial validation.
- [x] Fixed typed Audio preset registry: MP3, FLAC, M4A/AAC, Opus, Ogg Vorbis, WAV.

## B2 Host/IPC — release hardening remains open
- [x] Single-instance Host runtime, current-user pipe DACL/SID validation, bounded framing/timeouts, fixed Host startup, crash-recovery queue/journal.
- [x] Development connected-server authentication and package identity qualification.
- [x] Status lookup, queued-only transactional cancellation and replay/disconnect/reconnect acceptance.
- [x] 12-case checked-in IPC adversarial corpus.
- [ ] Production signed-package B2 identity/authentication requalification.

## B3 Explorer
- [x] Real native `IExplorerCommand` DLL, package COM/context-menu registration, fixed typed product subcommands.
- [x] Native Audio submenu mirrors fixed MP3/FLAC/M4A-AAC/Opus/Ogg-Vorbis/WAV actions.
- [ ] Headed Windows 11 modern-menu acceptance with exact-build screenshots.
- [ ] Explorer crash/hang/failure headed matrix.

## B4 Containment
- [x] Private staging, strict AppContainer, Job Object termination/resource ceilings, no-network/outside-scope denial, finite output growth, explicit Strict/Compatibility without silent downgrade.

## B5–B8 Providers
- [x] Development Audio MVP and expanded fixed Audio matrix.
- [x] 36-case representative Audio source/action matrix through the packaged strict path.
- [x] Repeated malformed/truncated rejection with deterministic failure and transactional preservation.
- [ ] Multi-file/mixed-valid-invalid Audio batch isolation and final Audio matrix closure.
- [ ] Production FFmpeg/ffprobe pin/signature/hash/licensing/redistribution approval.
- [ ] After final Audio closure: Image/Video providers and malformed corpora.

## B9 UX/settings
- [ ] Defaults, mixed-selection behavior, pinned presets, output/concurrency/isolation settings and user-visible progress/result UX.

## B10 Plugin SDK
- [ ] Manifest/API/signature/hash gate and worker-only non-media sample provider.

## B11–B12 Release
- [ ] Final fuzz/chaos/security/static gates.
- [ ] Signed production MSIX and clean Windows 11 VM install/update/uninstall.
- [ ] Final SBOM/notices/hash manifest and headed end-user acceptance.

Check a box only when matching evidence exists. ADR-013 changes development ordering only; it does not waive final shipping requirements.
