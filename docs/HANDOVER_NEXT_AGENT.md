# CONVERTY — CONTINUATION HANDOVER
# PRODUCT-FIRST ROADMAP — 0.1.0-dev.13 STATUS/CANCEL WIRE

Continue development directly in `https://github.com/aeiouofficial/converty`. Default branch: `main`.

Current workspace version: `0.1.0-dev.13`. Next workspace version: `0.1.0-dev.14`.

Repository `main` is the only durable completed authority. Re-fetch live `main` before every write and completion claim. Dev.13 must be treated as frozen only if ordinary CI on the exact current `main` HEAD has continuity, managed, and supply-chain-static all successful, tracked generated authority zero-diff, deterministic workspace verification PASS, and verified delivery uploaded.

## DEV.13 BEHAVIOR
Existing conversion admission is unchanged. Typed one-shot `status`/`cancel` uses the existing authenticated Host pipe and existing `JobStatusSnapshot`; cancellation is queued-only and transactional. Bridge uses a fresh connection and verifies connected Host identity before first application-frame write. Host expected-user validation precedes application-frame parsing. No second pipe, persistent session protocol, polling service, UI, or Host reroute of normal conversion was added.

## EVIDENCE
- Task 1 RED `beabdc7fe0fec0e9d2e0f9f6add4fefa9eaa593b` / run `33285343578`.
- Task 2 RED `01ac60213d91ed14b721fbe954fbb0c5143e5de3` / run `33285816631`.
- Task 3 RED `7954def408d79e5bf31b783b472df2d02669d2d4` / run `33286056932`.
- Strengthened pre-version behavior `84f1b2502c912633c8fb019da3d6860e6891cf9c` / run `33318858033`.
- Versioned behavior evidence `d3396ece12d7bd9a1d2d86ad03285e206b94456a` / run `33319252101`, managed `99278206624`, static `99278206672`.
- Observed behavior: Release 0 warnings/errors; native/package/direct+registered COM/product conversion PASS; 248/248 managed; 78/78 static; 5/5 vectors; Unicode/metacharacter paths and no-overwrite numbered publication; MP3 320000 bit/s.

## HARD INVARIANTS
Preserve Explorer → native `IExplorerCommand` → fixed app-local Bridge → strict disposable EngineWorker/provider → fixed app-local FFmpeg → private staging → transactional numbered no-overwrite publication. Preserve Unicode/metachar filenames, source/existing-destination preservation, no shell/raw FFmpeg/user converter/PATH/network/silent fallback, media-neutral Host/Bridge, worker/provider-only media parsing, and no signing private keys.

Gyan FFmpeg is development-only and not production redistribution approval. Do not redesign direct shell Bridge launch or fixed Host launcher without new evidence.

## HEADED WINDOWS 11 LIMITATION
No real headed Windows 11 Explorer environment is available here. Do not claim modern submenu visual acceptance, mouse-driven acceptance, exact-build screenshots, headed Explorer failure matrix or end-user UI acceptance.

## OPEN SHIPPING BLOCKERS
1. headed Windows 11 modern Explorer acceptance and exact-build screenshots
2. Explorer crash/hang/failure headed matrix
3. production signed-package B2 identity/authentication requalification
4. replay/disconnect/reconnect/session acceptance
5. production FFmpeg redistribution/license/notices/signature/hash approval
6. signed production MSIX
7. clean Windows 11 VM install/update/uninstall
8. final security/fuzz/chaos/release audit
9. end-user acceptance

## ONE PRECISE NEXT TASK
If exact-current-main dev.13 CI is fully green under the finality rule above, begin `0.1.0-dev.14` by test-first qualifying replay, disconnect, reconnect, and one-shot session behavior across conversion admission/status/cancel without adding an unnecessary persistent-session protocol. If the gate is not green, repair only the failing dev.13 authority/delivery gate first.

## RECURSIVE HANDOVER RULE
Every completed tranche must end with a new full copy-paste handover containing repo/default branch/live SHA/tree, prior authority, all commits, RED/GREEN history, run/job/artifact IDs, exact changes/reasons, executed tests/build/security outcomes, workspace hashes/counts, blockers, explicitly unverified claims, ONE precise next task, invariants, headed limitation, production signing/FFmpeg limitation, and this same recursive rule.
