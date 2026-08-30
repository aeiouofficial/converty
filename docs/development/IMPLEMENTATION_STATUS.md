# Implementation status — 0.1.0-dev.13

## Dev.13 authenticated status/cancel wire — 2026-08-30
- Added strict typed `status` and `cancel` control contracts on the existing Host pipe; no second pipe or persistent session protocol was introduced.
- Existing conversion admission and the normal `IExplorerCommand → Bridge → Strict EngineWorker → provider → FFmpeg → staging → numbered publication` path remain unchanged.
- Bridge uses a fresh connection for every submission/control call and verifies the connected Host before writing the first application frame.
- Host validates the expected peer before reading application frames and rejects malformed/hybrid control payloads with the existing generic `invalidRequest` response.
- Status reuses `JobStatusSnapshot`. Cancellation delegates to existing `HostJobQueue.TryCancel`, is queued-only, persists before in-memory publication, and reports `jobNotFound`, `notCancellable`, or `persistenceFailure` as appropriate.
- IPC fuzz corpus is 12 cases. Dedicated dev.13 static gates lock single-pipe reuse, auth ordering, strict serialization, queued-only cancellation, and Host/Bridge media neutrality.

## TDD evidence
- Task 1 contracts/serialization RED: `beabdc7fe0fec0e9d2e0f9f6add4fefa9eaa593b`, run `33285343578`, managed `99187464826`.
- Task 2 Host dispatch RED: `01ac60213d91ed14b721fbe954fbb0c5143e5de3`, run `33285816631`, managed `99188726185`.
- Task 3 Bridge client RED: `7954def408d79e5bf31b783b472df2d02669d2d4`, run `33286056932`, managed `99189341816`.
- Task 3 analyzer-only correction behavior: `630b622dc7d2d1c8df9a9ce7b44c37efc28c9401`, run `33286255211`, managed behavior green before stale-authority package verification.
- Task 4 strengthened behavior/static head: `84f1b2502c912633c8fb019da3d6860e6891cf9c`, run `33318858033`, managed `99277143724`, static `99277143808`, continuity `99277143780`.

## Observed pre-version qualification at 84f1b250
- Windows Server 2025 / windows-2025-vs2026; .NET SDK 10.0.400.
- 18/18 locked restore; dependency audit PASS across 18 projects/18 frameworks with 0 vulnerable-result packages.
- Release build PASS with 0 warnings / 0 errors.
- Native Explorer, unsigned development package/MakeAppx, direct class-factory Invoke and loose-package COM activation/Invoke PASS.
- Real Bridge→Strict Worker→FFmpeg conversion PASS; Unicode/metacharacter path, source/existing-destination preservation, numbered `(1)` publication and MP3 exactly 320000 bit/s PASS.
- Managed tests 248/248 PASS, 0 skipped; Python static tests 78/78 PASS; contract vectors 5/5 PASS; repository verifier PASS after in-job generated authority.
- Deterministic workspace A/B byte identity PASS at pre-version bytes; embedded manifest verification correctly rejected stale tracked authority at `src/Converty.Bridge/Ipc/BridgeClient.cs`.

## Current authority state
Dev.13 source/version metadata is being synchronized. Final runner-generated SBOM/package/hash authority, exact-current-main ordinary CI, deterministic dev.13 workspace ZIP and verified-delivery artifacts are still pending.

## Remaining shipping gates
Headed Win11 UI/screenshots and Explorer failure matrix; production signed-package B2 requalification; replay/disconnect/reconnect/session acceptance; FFmpeg redistribution approval; signed MSIX/clean-VM lifecycle; final security/fuzz/chaos/release/end-user acceptance.
