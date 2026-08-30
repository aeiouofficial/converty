# Changelog

## 0.1.0-dev.14 — 2026-08-30
- Added test-first replay/disconnect/reconnect acceptance for the existing authenticated one-shot Host IPC; no second pipe or persistent-session protocol was introduced.
- Added `HostJobQueue.TryGetByRequestId` and made authenticated admission replay idempotent: an already-known `requestId` returns its existing `jobId` without a second enqueue.
- Qualified recovery from an ambiguous post-send client disconnect by replaying the same admission on a fresh connection, then resolving status on another fresh connection.
- Qualified fresh-connection admission → status → cancel → status behavior, preserving queued-only transactional cancellation and one queue entry.
- Preserved RED evidence at `34b0401ed139efe55f76037b55d8e749e30afc0b` / run `33327339694`: 250 total, 249 passed, exactly the new replay assertion failed.
- GREEN behavior head `46da899ec7dad5ebe2acc934dbaf7c009abc0c26` / run `33327473492` passed 250/250 managed tests, 78/78 static tests, 5/5 vectors, Release 0 warnings/errors and all native/package/COM/product conversion gates before the expected generated-authority freshness boundary.

## 0.1.0-dev.13 — 2026-08-30
- Added typed one-shot `status` and `cancel` requests/responses on the existing authenticated Host named pipe while preserving the legacy conversion-admission JSON and normal Explorer→Bridge→Strict Worker→FFmpeg product path.
- Added a separate `IBridgeJobControlClient` surface implemented by the existing `BridgeClient`; every operation opens a fresh connection and verifies connected-server identity before the first application-frame write.
- Added strict canonical-D job IDs, strict unknown/duplicate/case/schema rejection, response operation/job correlation, and typed `jobNotFound`, `notCancellable`, and `persistenceFailure` outcomes.
- Reused existing `HostJobQueue.TryGet`/`TryCancel`; cancellation remains queued-only and transactional, and persistence failure leaves queued state unchanged.
- Expanded the checked-in IPC adversarial corpus from 7 to 12 cases and added dev.13 static architecture gates for single-pipe reuse, auth ordering, status-model reuse and Host/Bridge media neutrality.
- Exact-main dev.13 finality was proven at `19482bc21460f84096e350f730065988239fbd3c`, run `33319810581`; deterministic workspace SHA-256 `c14ac057f11fb9d47eac7687ec73e59b0aa1f3658cf9b361e83bc325b051743a`.

## 0.1.0-dev.12 — 2026-08-29
- Eliminated the historical `StrictWorkerIsTerminatedWhenStagingGrowthExceedsOutputBudget` scheduler-dependent test assumption without changing production output limits or containment.
- Preserved RED evidence at `ad223384400be1c5749e0b09e301f7ddd5565eda` / run `33271012596`; behavior head `f4c241b0895d06d2e44d72f31e07f141cdc74577` passed 192/192 managed and 72/72 static tests before authority freshness.

## 0.1.0-dev.11 — 2026-08-29
- Closed development B2 connected-server identity/authentication without rerouting normal conversion through Host.

## 0.1.0-dev.10 — 2026-08-27
- Moved conversion execution into fixed app-local `Converty.EngineWorker` and `Converty.Provider.FFmpeg` with private per-job staging and strict containment.

## 0.1.0-dev.9 — 2026-08-26
- Delivered the first automated functional Windows product path: packaged native Explorer command → fixed Bridge → typed preset → fixed app-local FFmpeg → same-folder numbered output.

Earlier foundation history remains available in repository history and prior handovers.
