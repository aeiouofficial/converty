# Changelog

## 0.1.0-dev.13 — 2026-08-30
- Added typed one-shot `status` and `cancel` requests/responses on the existing authenticated Host named pipe while preserving the legacy conversion-admission JSON and normal Explorer→Bridge→Strict Worker→FFmpeg product path.
- Added a separate `IBridgeJobControlClient` surface implemented by the existing `BridgeClient`; every operation opens a fresh connection and verifies connected-server identity before the first application-frame write.
- Added strict canonical-D job IDs, strict unknown/duplicate/case/schema rejection, response operation/job correlation, and typed `jobNotFound`, `notCancellable`, and `persistenceFailure` outcomes.
- Reused existing `HostJobQueue.TryGet`/`TryCancel`; cancellation remains queued-only and transactional, and persistence failure leaves queued state unchanged.
- Expanded the checked-in IPC adversarial corpus from 7 to 12 cases and added dev.13 static architecture gates for single-pipe reuse, auth ordering, status-model reuse and Host/Bridge media neutrality.
- Preserved TDD RED evidence: Task 1 `beabdc7fe0fec0e9d2e0f9f6add4fefa9eaa593b` / run `33285343578`; Task 2 `01ac60213d91ed14b721fbe954fbb0c5143e5de3` / run `33285816631`; Task 3 `7954def408d79e5bf31b783b472df2d02669d2d4` / run `33286056932`.
- Pre-version behavior head `84f1b2502c912633c8fb019da3d6860e6891cf9c` / run `33318858033` passed 248/248 managed, 78/78 static, 5/5 vectors and all native/package/COM/product conversion gates; only generated-authority/workspace-integrity freshness remained intentionally open.

## 0.1.0-dev.12 — 2026-08-29
- Eliminated the historical `StrictWorkerIsTerminatedWhenStagingGrowthExceedsOutputBudget` scheduler-dependent test assumption without changing production output limits or containment.
- Replaced the arbitrary 512 KiB post-kill ceiling with a canary that writes exactly 64 KiB + 4 KiB and then holds until the existing launcher detects the breach and terminates the Job Object.
- Preserved RED evidence at `ad223384400be1c5749e0b09e301f7ddd5565eda` / run `33271012596` and failed intermediate experiments rather than rewriting history.
- Behavior head `f4c241b0895d06d2e44d72f31e07f141cdc74577` / run `33271379504` passed 192/192 managed tests, 72/72 static tests, 5/5 vectors, Release 0 warnings/errors, native/package/COM/product conversion gates; only generated-authority freshness remained intentionally open.
- Production `WindowsWorkerProcessLauncher`, resource limits, AppContainer/Job Object isolation, output polling and normal Bridge→Worker→FFmpeg path are unchanged.

## 0.1.0-dev.11 — 2026-08-29
- Closed development B2 connected-server identity/authentication without rerouting normal conversion through Host.
- Development package staging includes exact sibling `Converty.Host.exe` and packaged Bridge authenticates connected Host PID/path/PFN/stable PID before first application frame.
- Real registered Explorer COM→Bridge PFN and package-identified parent→Host PFN preservation were qualified under development package identity.

## 0.1.0-dev.10 — 2026-08-27
- Moved conversion execution into fixed app-local `Converty.EngineWorker` and `Converty.Provider.FFmpeg` with private per-job staging, strict AppContainer/Job Object containment, finite resource/output limits, and no silent Strict→Compatibility fallback.
- Behavior-qualified at `f221563c790057344a94b4e60c309d4512a77c38`, run `33028554361`.

## 0.1.0-dev.9 — 2026-08-26
- Delivered the first automated functional Windows product path: packaged native Explorer command → fixed Bridge → typed preset → fixed app-local FFmpeg → same-folder numbered output.
- Behavior-qualified at `b71aa06fb024afe85f64707b05d996e86c37d8c8`, run `33001019450`.

Earlier foundation history remains available in repository history and prior handovers.
