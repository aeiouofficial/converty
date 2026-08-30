# Changelog

## 0.1.0-dev.15 — 2026-08-30
- Expanded the fixed typed Audio action matrix with `audio.m4a.aac` (M4A/AAC 256k + faststart), `audio.opus` (libopus 192k VBR, application=audio), and `audio.ogg.vorbis` (libvorbis q6), preserving MP3 320k, FLAC and WAV.
- Mirrored all fixed Audio preset IDs in the native Explorer submenu with stable canonical GUIDs; no raw FFmpeg argument surface or arbitrary executable/path discovery was added.
- Extended the real packaged product smoke from MP3-only to MP3 + M4A/AAC + Opus + Ogg Vorbis. Each target uses the same Unicode/metacharacter source, preserves the source and a pre-existing destination, publishes numbered output, leaves no partial output, and is ffprobe codec-verified.
- Preserved RED evidence at `f729f821c98bc8841e585bad7764d8f7446d1c65` / run `33330926712`: managed 253 total, 249 passed, exactly 4 new managed expectations failed; 78 existing static tests passed and exactly 3 new dev.15 static assertions failed.
- GREEN behavior head `335754a7d99c99f918fa7f2bc29a89f691f0fd2a` / run `33331186761` passed 253/253 managed tests, 81/81 static tests, 5/5 vectors, Release 0 warnings/errors and all native/package/COM/four-target product conversion gates before the expected generated-authority freshness boundary.

## 0.1.0-dev.14 — 2026-08-30
- Added test-first replay/disconnect/reconnect acceptance for the existing authenticated one-shot Host IPC; no second pipe or persistent-session protocol was introduced.
- Added `HostJobQueue.TryGetByRequestId` and made authenticated admission replay idempotent: an already-known `requestId` returns its existing `jobId` without a second enqueue.
- Qualified recovery from an ambiguous post-send client disconnect by replaying the same admission on a fresh connection, then resolving status on another fresh connection.
- Qualified fresh-connection admission → status → cancel → status behavior, preserving queued-only transactional cancellation and one queue entry.
- Frozen exact-main authority: `e0d9a00c3cb832e8109bf7ba7320215302da2177`, run `33328195635`, workspace SHA-256 `532d1916d33bff2f440e96ab9a3cabc0b0f1898ea5ad2b35bfccb1a9eb63ca44`.

## 0.1.0-dev.13 — 2026-08-30
- Added typed one-shot `status` and `cancel` requests/responses on the existing authenticated Host named pipe while preserving the legacy conversion-admission JSON and normal Explorer→Bridge→Strict Worker→FFmpeg product path.

## 0.1.0-dev.12 — 2026-08-29
- Eliminated the historical scheduler-dependent containment test assumption without changing production output limits or containment.

## 0.1.0-dev.11 — 2026-08-29
- Closed development B2 connected-server identity/authentication without rerouting normal conversion through Host.

## 0.1.0-dev.10 — 2026-08-27
- Moved conversion execution into fixed app-local `Converty.EngineWorker` and `Converty.Provider.FFmpeg` with private per-job staging and strict containment.

## 0.1.0-dev.9 — 2026-08-26
- Delivered the first automated functional Windows product path: packaged native Explorer command → fixed Bridge → typed preset → fixed app-local FFmpeg → same-folder numbered output.

Earlier foundation history remains available in repository history and prior handovers.
