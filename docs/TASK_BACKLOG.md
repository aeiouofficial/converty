# Converty Implementation Backlog

## Dev.15 closure / dev.16 next
- [x] Preserve dev.14 authenticated one-shot IPC and normal Explorer→Bridge→Strict Worker→FFmpeg routing.
- [x] Add fixed typed `audio.m4a.aac`, `audio.opus`, and `audio.ogg.vorbis` product presets without raw FFmpeg passthrough.
- [x] Mirror the expanded Audio action matrix in the native Explorer submenu with stable preset IDs and canonical GUIDs.
- [x] Expand real product smoke to MP3 + M4A/AAC + Opus + Ogg Vorbis with Unicode/metacharacter paths, source/existing-destination preservation, numbered publication, no partial outputs, and ffprobe codec checks.
- [x] Preserve RED evidence at `f729f821c98bc8841e585bad7764d8f7446d1c65` / run `33330926712`.
- [x] Pre-authority GREEN behavior at `335754a7d99c99f918fa7f2bc29a89f691f0fd2a` / run `33331186761`: 253/253 managed, 81/81 static, 5/5 vectors, Release/native/package/COM/four-target product matrix PASS before expected generated-authority freshness failure.
- [ ] Synchronize dev.15 generated authority and require branch zero-diff qualification plus exact-current-main CI + deterministic verified delivery.
- [ ] Dev.16: Audio source-format and malformed-input acceptance across the fixed Audio action matrix before beginning Image/Video expansion.

## B0 Repository/bootstrap
- [x] .NET 10.0.400 / C# 14 / C++20 topology, warnings-as-errors, analyzers, locked dependencies, immutable Action pins, vulnerability audit and deterministic workspace tooling.
- [ ] Final Debug/Release signed-production matrix and dependency/license/notices review.

## B1 Core contracts
- [x] Strict versioned contracts/schemas, capability/planner/output collision logic, typed JSON v1 adapters and adversarial validation.
- [x] Typed preset registry with fixed product actions; dev.15 expanded Audio presets remain closed over reviewed arguments.

## B2 Host/IPC — release hardening remains open
- [x] Single-instance Host runtime, current-user pipe DACL/SID validation, bounded framing/timeouts, fixed Host startup, crash-recovery queue/journal.
- [x] Development connected-server authentication and package identity qualification.
- [x] Status lookup and queued-only transactional cancellation wire.
- [x] Replay/disconnect/reconnect acceptance without persistent-session architecture.
- [x] 12-case checked-in IPC adversarial corpus.
- [ ] Production signed-package B2 identity/authentication requalification.

## B3 Explorer
- [x] Real native `IExplorerCommand` DLL, package COM/context-menu registration, fixed typed product subcommands.
- [x] Dev.15 native submenu contains fixed MP3/FLAC/M4A-AAC/Opus/Ogg-Vorbis/WAV Audio actions plus existing Video/Image actions.
- [ ] Headed Windows 11 modern-menu acceptance with exact-build screenshots.
- [ ] Explorer crash/hang/failure headed matrix.

## B4 Containment
- [x] Private staging, strict AppContainer, Job Object termination/resource ceilings, no-network/outside-scope denial, finite output growth, explicit Strict/Compatibility without silent downgrade.

## B5–B8 Providers
- [x] Development Audio MVP: WAV/supported audio → MP3 320k through Bridge→EngineWorker→FFmpeg, ffprobe-verified.
- [x] Expand fixed typed Audio conversion preset/action matrix: MP3, FLAC, M4A/AAC, Opus, Ogg Vorbis, WAV.
- [x] Dev.15 real product smoke qualifies MP3 + M4A/AAC + Opus + Ogg Vorbis through the packaged strict path.
- [ ] Audio source-format and malformed-input acceptance matrix across the expanded actions.
- [ ] Production FFmpeg/ffprobe pin/signature/hash/licensing/redistribution approval.
- [ ] Final Audio matrix closure, then Image/Video providers and malformed corpora.

## B9 UX/settings
- [ ] Defaults, mixed-selection behavior, pinned presets, output/concurrency/isolation settings and user-visible progress/result UX.

## B10 Plugin SDK
- [ ] Manifest/API/signature/hash gate and worker-only non-media sample provider.

## B11–B12 Release
- [ ] Final fuzz/chaos/security/static gates.
- [ ] Signed production MSIX and clean Windows 11 VM install/update/uninstall.
- [ ] Final SBOM/notices/hash manifest and headed end-user acceptance.

Check a box only when matching evidence exists. ADR-013 changes development ordering only; it does not waive final shipping requirements.
