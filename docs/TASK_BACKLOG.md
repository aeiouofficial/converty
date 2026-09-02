# Converty Implementation Backlog

## Dev.19 Image mixed-batch closure — FROZEN
- [x] Preserve frozen dev.18 Image single-file authority and recursive Audio evidence.
- [x] Define dev.19 test-first mixed-valid/invalid Image batch contract.
- [x] Add focused core-runner Image batch isolation coverage.
- [x] Add real Windows packaged Image mixed-batch smoke and CI wiring.
- [x] Correct same-extension Image source/target alias with RED/GREEN evidence.
- [x] Correct the core-test same-extension fixture without changing production behavior.
- [x] Requalify behavior: Audio/Image product gates, 255/255 managed, 99/99 static, 5/5 vectors.
- [x] Curate and independently verify exact generated authority.
- [x] Synchronize generated authority via guarded exact-parent self-deleting workflow.
- [x] Require canonical dev.19 zero-diff plus complete deterministic-workspace/delivery qualification.
- [x] Re-read unchanged main and fast-forward non-force only.
- [x] Require fresh exact-main three-job SUCCESS.
- [x] Independently verify final exact-main workspace and delivery.
- [x] Freeze dev.19 at main `eb0ce66dab646427d5bef1548c12e5cc4765b2f1`, tree `337a4e11fb41bab6b6eeb462c3755381580f06c1`, run `33597504612`.

## Dev.20 Video foundation — NEXT
- [ ] Use Superpowers brainstorming on exact frozen main; inspect existing typed preset/provider/native Explorer patterns.
- [ ] Present Video design options and recommended design to the human; obtain explicit design approval before implementation.
- [ ] Commit approved Video design spec and invoke writing-plans.
- [ ] Create `dev/0.1.0-dev.20-video-foundation` from exact frozen main `eb0ce66dab646427d5bef1548c12e5cc4765b2f1` after approval.
- [ ] Establish RED static and managed Video contracts before production implementation.
- [ ] Define only fixed typed Video actions/output contracts; no raw FFmpeg surface or arbitrary executable path.
- [ ] Qualify representative Video source extensions × every fixed action through packaged Bridge → Strict Worker/provider → app-local FFmpeg.
- [ ] Preserve Unicode/metacharacter filenames, sources, pre-existing destinations and collision numbering.
- [ ] Add repeated malformed/truncated deterministic rejection.
- [ ] Prove mixed-valid/invalid Video batch failure isolation and later-valid continuation.
- [ ] Prove bounded waits, staging cleanup, partial cleanup and worker/FFmpeg process cleanup.
- [ ] Keep all Audio 36-case + negatives + mixed-batch and Image 24-case + negatives + mixed-batch gates green.
- [ ] Close generated authority, branch zero-diff and exact-main qualification before freezing dev.20.

## B0 Repository/bootstrap
- [x] .NET 10.0.400 / C# 14 / C++20 topology, warnings-as-errors, analyzers, locked dependencies, immutable Action pins, vulnerability audit and deterministic workspace tooling.
- [x] Disposable build/test/media/package outputs constrained to excluded `artifacts/`, `bin/`, `obj` and cache paths.
- [ ] Final Debug/Release signed-production matrix and dependency/license/notices review.

## B1 Core contracts
- [x] Strict versioned contracts/schemas, capability/planner/output collision logic, typed JSON v1 adapters and adversarial validation.
- [x] Fixed typed Audio preset registry.
- [x] Fixed typed Image actions (PNG/JPEG/WebP) qualified through single-file and mixed-batch product gates.
- [ ] Fixed typed Video actions after approved dev.20 design.

## B2 Host/IPC — release hardening remains open
- [x] Single-instance Host runtime, current-user pipe DACL/SID validation, bounded framing/timeouts, fixed Host startup, crash-recovery queue/journal.
- [x] Development connected-server authentication and package identity qualification.
- [x] Status lookup, queued-only transactional cancellation and replay/disconnect/reconnect acceptance.
- [x] 12-case checked-in IPC adversarial corpus.
- [ ] Production signed-package B2 identity/authentication requalification.

## B3 Explorer
- [x] Real native `IExplorerCommand` DLL, package COM/context-menu registration, fixed typed product subcommands.
- [x] Native Audio and Image submenu actions map only to fixed typed preset IDs.
- [ ] Video submenu integration after approved fixed Video contracts.
- [ ] Headed Windows 11 modern-menu acceptance with exact-build screenshots.
- [ ] Explorer crash/hang/failure headed matrix.

## B4 Containment
- [x] Private staging, strict AppContainer, Job Object termination/resource ceilings, no-network/outside-scope denial, finite output growth, explicit Strict/Compatibility without silent downgrade.

## B5–B8 Providers
- [x] Audio action/source/malformed/batch closure through strict product path.
- [x] Image 8-source × 3-action acceptance plus malformed/truncated and mixed-batch closure.
- [ ] Production FFmpeg/ffprobe pin/signature/hash/licensing/redistribution approval.
- [ ] Video action/source/malformed/batch qualification.

## B9 UX/settings
- [ ] Defaults, mixed-selection behavior, pinned presets, output/concurrency/isolation settings and user-visible progress/result UX.

## B10 Plugin SDK
- [ ] Manifest/API/signature/hash gate and worker-only non-media sample provider.

## B11–B12 Release
- [ ] Final fuzz/chaos/security/static gates.
- [ ] Signed production MSIX and clean Windows 11 VM install/update/uninstall.
- [ ] Final SBOM/notices/hash manifest and headed end-user acceptance.

## Execution rule
Check a box only when matching evidence exists. Product-first development ordering does not waive final shipping requirements. Never hand-edit generated SBOM/package/hash authority.