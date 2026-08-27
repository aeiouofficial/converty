# Implementation status — 0.1.0-dev.10

## Tranche result
`0.1.0-dev.10` behavior-qualifies B4 disposable-worker containment while preserving the functional Explorer product introduced by ADR-013:

`IExplorerCommand → fixed Bridge → Strict EngineWorker → typed preset/provider → fixed app-local FFmpeg → private staging → validated numbered publication`

The B4 behavior head is frozen separately from this authority synchronization. dev.10 is not a frozen delivery until generated SBOM/package/hash authority and exact-head package verification are green.

## Behavior qualification
Immutable dev.10 B4 behavior head: `f221563c790057344a94b4e60c309d4512a77c38`.
GitHub Actions run: `33028554361`; managed job `98375493893`; static job `98375494099`.
Runner: Windows Server 2025 (`windows-2025-vs2026`, image version `20260824.214.3`) with .NET SDK exactly `10.0.400`.

Executed results:
- 18/18 managed projects locked restore PASS.
- NuGet vulnerability audit PASS; 18 projects / 18 frameworks / 0 vulnerable-result packages.
- Release managed build PASS; 0 warnings, 0 errors.
- Native C++20/MSVC Explorer Release build PASS.
- Development FFmpeg/ffprobe 9.0.1 archive SHA-256 verification PASS; both executables execute successfully.
- MakeAppx unsigned development package schema/layout validation PASS.
- Direct staged shell DLL class factory + `IExplorerCommand::Invoke` PASS.
- Loose package registration + packaged COM activation + `IExplorerCommand::Invoke` PASS.
- Real strict Bridge→EngineWorker→FFmpeg product smoke PASS with Unicode/metacharacter path, source preservation, pre-existing base destination preservation, numbered output, and ffprobe MP3 exactly 320000 bit/s.
- Microsoft Testing Platform/xUnit: 190 total, 190 succeeded, 0 failed, 0 skipped.
- Contract vectors 5/5 PASS; repository verifier PASS; Python/static tests 66/66 PASS.
- Workspace ZIP double build produced identical bytes: SHA-256 `a0edd6e15a63d71cc2ef493ef33f6bb6e3f0b16ee0d8f484ebc981b800f749de`, 369035 bytes, 328 files.
- Final ZIP embedded-manifest verification failed only because tracked generated authority was intentionally stale before this dev.10 synchronization (`build/stage-dev-package.ps1` package-manifest assertion). Delivery upload therefore correctly did not occur in the behavior run.

## B4 containment implemented and qualified
- Each source item is copied into a unique Converty-owned private job directory; the worker receives staged paths only and Core publishes the validated output with race-safe `File.Move(..., overwrite: false)` numbered semantics.
- Core owns only `IConversionWorkerClient`; FFmpeg path trust and process execution are isolated in the FFmpeg provider behind the disposable EngineWorker.
- EngineWorker accepts a fixed typed CLI surface (`--preset`, `--input`, `--output`) and reconstructs FFmpeg arguments from `ProductPresetRegistry`; no raw FFmpeg vector is forwarded from Explorer/user/IPC.
- Windows worker creation is suspended, uses an explicit inherited-handle list, is assigned to a Job Object before resume, and runs without shell execution.
- Job Object/resource policy includes kill-on-close, finite process count, process/job memory, CPU hard cap, wall-clock timeout/cancellation, bounded stderr and finite output growth.
- Strict isolation creates a unique zero-capability AppContainer, grants application read/execute plus private-staging read/write, rejects reparse-point scope substitution, and cleans the ACL/profile authority afterward.
- Direct strict canaries prove staging write allowed, outside/sibling write denied, loopback connection denied, and descendants are killed with the job.
- Product Bridge explicitly requests `WorkerIsolationLevel.Strict`; no Compatibility fallback exists on strict failure.
- Output growth is sampled every 25 ms relative to a pre-resume baseline. Positive per-path growth is charged, shrinking/deleting staged input gives no refund, new files count in full, a final post-exit check prevents fast-exit bypass, and reparse points fail closed.
- `ConversionDefault.MaximumOutputBytes` is 8 GiB with a 16 GiB hard configuration maximum. The executable Windows canary uses a 64 KiB budget and proves the worker is terminated with `WorkerOutputLimitExceededException` after exceeding it.

## Remaining shipping gates
1. Real headed Windows 11 Explorer acceptance and current-build screenshots, plus Explorer crash/hang/failure headed matrix.
2. Remaining B2 connected-server anti-squatting/authentication, final status/cancel wire decision, and replay/disconnect/session acceptance.
3. Production FFmpeg redistribution/license/notices/signature/hash approval; the current Gyan 9.0.1 payload is development qualification only.
4. Signed production MSIX and clean Windows 11 VM install/update/uninstall acceptance.
5. Final security/fuzz/chaos/release audit and end-user shipping acceptance.

## Boundary status
Explorer remains trigger-only. Host remains media/process neutral. Core coordinates typed presets, staging and publication but no longer launches FFmpeg. Production media parsing/conversion occurs in a disposable strict worker/provider process. Strict local conversion has no network capability. Original inputs and externally created destinations are never overwritten.
