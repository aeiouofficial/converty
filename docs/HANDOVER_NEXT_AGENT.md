# CONVERTY — CONTINUATION HANDOVER
# PRODUCT-FIRST ROADMAP — 0.1.0-dev.14 SESSION ACCEPTANCE

Continue development directly in `https://github.com/aeiouofficial/converty`. Default branch: `main`.

Current workspace version: `0.1.0-dev.14`. Next workspace version: `0.1.0-dev.15`. Repository `main` is the only durable completed authority. Re-fetch live `main` before every write and completion claim.

## PRIOR FROZEN AUTHORITY
Dev.13 main `19482bc21460f84096e350f730065988239fbd3c`, tree `53f8638cb7be0bec1e0175569a8b22c009d3d771`, exact-main run `33319810581` all three jobs successful. Deterministic workspace SHA-256 `c14ac057f11fb9d47eac7687ec73e59b0aa1f3658cf9b361e83bc325b051743a`. Verified delivery artifact `9734576999`.

## DEV.14 BEHAVIOR
Existing one-shot authenticated IPC is preserved. Admission replay is now idempotent by `requestId`: if the queue reports a duplicate request, Host resolves the already-known request-to-job mapping and returns the existing `jobId` without enqueueing another job. Fresh connections recover after ambiguous disconnect and support status/cancel/status. No persistent-session protocol, second pipe, media parsing in Host/Bridge, or normal conversion reroute was added.

## TDD EVIDENCE
- RED `34b0401ed139efe55f76037b55d8e749e30afc0b`, run `33327339694`, managed `99299712127`: 250 total, 249 passed, exactly the new replay acceptance assertion failed.
- Queue lookup `7766cb1f7831356de08e2288ac2da51bcfee743d`.
- GREEN behavior `46da899ec7dad5ebe2acc934dbaf7c009abc0c26`, run `33327473492`, managed `99300068738`: 250/250 managed, 78/78 static, 5/5 vectors, Release 0 warnings/errors, native/package/direct+registered COM/product conversion PASS. Deterministic A/B source ZIP SHA `d6b3b56b3cf3f84c10a86c652e634f75f052f749b2cd31420169aba7f9dab73a`, 420896 bytes, 349 files, then expected stale generated-authority manifest failure.

## FINALITY RULE
Do not call dev.14 frozen until version-aligned generated authority is synchronized from one exact CI artifact, branch qualification reaches generated-authority zero-diff, `main` is fast-forwarded, and ordinary CI on the exact current `main` has continuity + managed + supply-chain-static all SUCCESS with deterministic workspace verification and verified delivery upload.

## HARD INVARIANTS
Preserve Explorer → native `IExplorerCommand` → fixed Bridge → strict disposable EngineWorker/provider → fixed app-local FFmpeg → private staging → transactional numbered no-overwrite publication. Preserve Unicode/metacharacter filenames, source/existing-destination preservation, no shell/raw FFmpeg/user converter/PATH/network/silent fallback, media-neutral Host/Bridge, worker/provider-only media parsing, and no signing private keys.

Gyan FFmpeg remains development qualification input only, not production redistribution approval.

## HEADED WINDOWS 11 LIMITATION
No real headed Windows 11 Explorer environment is available here. Do not claim modern submenu visual acceptance, mouse-driven acceptance, exact-build screenshots, headed Explorer failure matrix or end-user UI acceptance.

## ONE PRECISE NEXT TASK AFTER DEV.14 FREEZE
Implement `0.1.0-dev.15` by expanding the fixed typed Audio conversion preset/action matrix through the existing Strict Worker/provider path, test-first, without raw FFmpeg passthrough, PATH lookup, network dependency or containment weakening.

## RECURSIVE HANDOVER RULE
Every completed tranche must end with a full copy-paste handover containing repo/default branch/live SHA/tree, prior authority, all commits, RED/GREEN history, run/job/artifact IDs, exact changes/reasons, executed tests/build/security outcomes, workspace hashes/counts, blockers, explicitly unverified claims, ONE precise next task, invariants, headed limitation, production signing/FFmpeg limitation, and this same recursive rule.
