# Converty Implementation Backlog

## Dev.17 closure / dev.18 next
- [x] Preserve frozen dev.16 exact-main authority and normal Explorer→Bridge→Strict Worker→FFmpeg routing.
- [x] Freeze dev.16 on exact `main` `dca3cbcba326a35801bc442ec93f16d84f58a692` with exact-main run `33346588907` and verified deterministic delivery.
- [x] Add managed RED coverage proving a failed middle Audio file must not suppress later selected files.
- [x] Change only the existing batch runner so ordinary per-file conversion failures are isolated while staging cleanup remains unconditional.
- [x] Add a dedicated real packaged five-file mixed-valid/invalid Audio batch smoke using one Bridge process per attempt.
- [x] Prove two repeated attempts: aggregate exit 4, later valid outputs survive, `(1)`→`(2)` collision numbering, preserved sources/pre-existing destinations, ffprobe success validation and zero partial residue.
- [x] Preserve 254/254 managed, 91/91 static, 5/5 vectors and every dev.16 product gate at behavior head.
- [ ] Synchronize dev.17 version-aligned generated authority and require branch zero-diff qualification plus exact-current-main CI + deterministic verified delivery.
- [ ] Dev.18: first fixed typed Image conversion action matrix through the existing Strict Worker/provider boundary.

## B0 Repository/bootstrap
- [x] .NET 10.0.400 / C# 14 / C++20 topology, warnings-as-errors, analyzers, locked dependencies, immutable Action pins, vulnerability audit and deterministic workspace tooling.
- [x] Disposable build/test/media/package outputs constrained to excluded paths; source tests organized under `tests/`.
- [ ] Final Debug/Release signed-production matrix and dependency/license/notices review.

## B1 Core contracts
- [x] Strict versioned contracts/schemas, capability/planner/output collision logic, typed JSON v1 adapters and adversarial validation.
- [x] Fixed typed Audio preset registry: MP3, FLAC, M4A/AAC, Opus, Ogg Vorbis, WAV.
- [ ] Re-audit and qualify the first fixed typed Image action matrix in dev.18.

## B2 Host/IPC — release hardening remains open
- [x] Single-instance Host runtime, current-user pipe DACL/SID validation, bounded framing/timeouts, fixed Host startup, crash-recovery queue/journal.
- [x] Development connected-server authentication and package identity qualification.
- [x] Status lookup, queued-only transactional cancellation and replay/disconnect/reconnect acceptance.
- [x] 12-case checked-in IPC adversarial corpus.
- [ ] Production signed-package B2 identity/authentication requalification.

## B3 Explorer
- [x] Real native `IExplorerCommand` DLL, package COM/context-menu registration, fixed typed product subcommands.
- [x] Native Audio submenu mirrors fixed Audio actions.
- [ ] Headed Windows 11 modern-menu acceptance with exact-build screenshots.
- [ ] Explorer crash/hang/failure headed matrix.

## B4 Containment
- [x] Private staging, strict AppContainer, Job Object termination/resource ceilings, no-network/outside-scope denial, finite output growth, explicit Strict/Compatibility without silent downgrade.

## B5–B8 Providers
- [x] Development Audio MVP and expanded fixed Audio matrix.
- [x] 36-case representative Audio source/action matrix plus repeated malformed/truncated rejection.
- [x] Multi-file/mixed-valid-invalid Audio batch isolation and final planned Audio matrix closure at behavior level.
- [ ] Production FFmpeg/ffprobe pin/signature/hash/licensing/redistribution approval.
- [ ] Dev.18+: Image provider/action qualification and malformed corpora.
- [ ] Subsequent Video provider/action qualification and malformed corpora.

## B9 UX/settings
- [ ] Defaults, broader mixed-selection UX, pinned presets, output/concurrency/isolation settings and user-visible progress/result UX.

## B10 Plugin SDK
- [ ] Manifest/API/signature/hash gate and worker-only non-media sample provider.

## B11–B12 Release
- [ ] Final fuzz/chaos/security/static gates.
- [ ] Signed production MSIX and clean Windows 11 VM install/update/uninstall.
- [ ] Final SBOM/notices/hash manifest and headed end-user acceptance.

Check a box only when matching evidence exists. ADR-013 changes development ordering only; it does not waive final shipping requirements.
