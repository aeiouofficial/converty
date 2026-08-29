# Implementation status — 0.1.0-dev.11

## Dev.12 output-budget determinism — 2026-08-29
- Behavior head `f4c241b0895d06d2e44d72f31e07f141cdc74577` / run `33271379504` passed the full product path and 192/192 managed tests.
- Historical 512 KiB post-kill ceiling was a test-only scheduler assumption; historical 610304-byte failure had already thrown the correct output-limit exception.
- Canary now writes exactly 64 KiB + 4 KiB then holds, making breach detection/termination deterministic.
- Production launcher/resource limits/AppContainer/Job Object/poll interval are unchanged.
- Generated dev.12 SBOM/package/hash authority and final exact-main delivery remain the closure step.

## Tranche result
Development B2 connected-server identity/authentication is executable-qualified while the normal product remains `IExplorerCommand → fixed Bridge → Strict EngineWorker → typed preset/provider → fixed FFmpeg → private staging → numbered publication`. Host is staged for dormant IPC/security infrastructure only.

## B2 evidence
- Host-missing package RED: run `33202365348`, job `98954716457`.
- Package-identified parent→Host PFN preserved: run `33211928010`, job `98986920905`.
- Real registered Explorer COM→Bridge PFN preserved: run `33218030168`, job `99005949641`.
- Real packaged Bridge→authenticated Host accepted: run `33218498644`, job `99007347897`; Host job `5bd48925-8c88-48d2-bbd7-a62c2ba03e3e`.
- Development PFN `Converty.Dev_yr4ybytcyx7nj`. Negative wrong/missing PFN, wrong path, PID race, unpackaged server and pre-frame-write cases remain fail-closed.

## Pre-version exact-tree qualification
Head `0d37afdba33abcd9ca31f3e59d0d6dc8a1bb7e5d`, tree `7560366cb059c1ff90c539f497903e84df1b2141`, run `33260905467`, managed `99122561963`, static `99122562067`: 18/18 locked restore, 0 vulnerable-result packages, Release 0 warnings/errors, native/package/COM/product PASS, 192/192 managed, 66/66 static, 5/5 vectors, zero-diff authority, deterministic workspace and verified delivery.

## Current authority state
Source/version authority is `0.1.0-dev.11`. Regenerate the four generated authority files and exact-head-qualify before freezing dev.11.

## Remaining shipping gates
Headed Win11 UI/screenshots and Explorer failure matrix; production signed-package B2 requalification; status/cancel + session acceptance; FFmpeg redistribution approval; signed MSIX/clean-VM lifecycle; final security/fuzz/chaos/release/end-user acceptance.

## Historical intermittent
`StrictWorkerIsTerminatedWhenStagingGrowthExceedsOutputBudget` has intermittently produced 191/192 on prior runs; unchanged-head reruns passed 192/192. Preserve this history until independently eliminated.
