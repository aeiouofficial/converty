# Implementation status — 0.1.0-dev.14

## Dev.14 one-shot replay/disconnect/reconnect acceptance — 2026-08-30
- Preserved the existing single authenticated Host pipe and fresh-connection-per-operation model; no persistent-session subsystem was added.
- Added queue lookup by `requestId` so an authenticated replay can recover the original `jobId` after an ambiguous disconnect without enqueuing a duplicate job.
- Admission replay is idempotent by `requestId`: duplicate detection still occurs in `HostJobQueue.TryEnqueue`; the request handler resolves the already-known job only for that duplicate outcome.
- Added real named-pipe acceptance coverage for send/disconnect/replay/status across fresh connections and admission/status/cancel/status across fresh connections.
- Existing dev.13 typed status/cancel behavior, expected-user authorization ordering, queue/journal transactional cancellation, and normal Explorer→Bridge→Strict Worker→FFmpeg conversion routing are unchanged.

## TDD evidence
- RED `34b0401ed139efe55f76037b55d8e749e30afc0b`, run `33327339694`, managed job `99299712127`: Release/native/package/COM/product smokes passed; managed test total 250, succeeded 249, failed exactly 1 at the new replay `Assert.True(replay.Accepted)` assertion.
- Queue lookup implementation `7766cb1f7831356de08e2288ac2da51bcfee743d`.
- GREEN behavior head `46da899ec7dad5ebe2acc934dbaf7c009abc0c26`, run `33327473492`, managed job `99300068738`: managed tests 250/250 PASS, static tests 78/78 PASS, contract vectors 5/5 PASS; build/product gates PASS. Workspace integrity then failed only because tracked generated authority still described pre-dev.14 bytes.

## Prior exact-main dev.13 authority
- Main `19482bc21460f84096e350f730065988239fbd3c`, tree `53f8638cb7be0bec1e0175569a8b22c009d3d771`.
- Run `33319810581`: managed `99279692688`, static `99279692787`, continuity `99279692791`, all SUCCESS including generated-authority zero-diff and verified delivery.
- Deterministic dev.13 workspace SHA-256 `c14ac057f11fb9d47eac7687ec73e59b0aa1f3658cf9b361e83bc325b051743a`, 418746 bytes, 348 files.

## Authority rule
Do not infer dev.14 finality from this document. Dev.14 is frozen only after version-aligned generated authority is synchronized from one exact CI artifact and an ordinary CI run on the exact current `main` HEAD has continuity, managed and supply-chain-static all successful, generated-authority zero-diff, deterministic workspace verification PASS and verified delivery uploaded.

## Remaining shipping gates
Headed Windows 11 UI/screenshots and Explorer failure matrix; production signed-package B2 identity/authentication requalification; production FFmpeg redistribution approval; signed production MSIX/clean-VM lifecycle; final security/fuzz/chaos/release/end-user acceptance.
