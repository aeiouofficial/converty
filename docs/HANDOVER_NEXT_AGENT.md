# CONVERTY — CONTINUATION HANDOVER
# PRODUCT-FIRST ROADMAP — 0.1.0-dev.12 OUTPUT-BUDGET DETERMINISM

Continue development directly in:

https://github.com/aeiouofficial/converty

Default branch: `main`.

Repository `main` is the only durable authority. Never treat local files, chat state, or side-branch-only work as completed authority. Re-fetch live `main` before every write and again before every completion claim.

## CURRENT SOURCE AUTHORITY

Version: `0.1.0-dev.12`
Source behavior head before generated-authority regeneration: `f4c241b0895d06d2e44d72f31e07f141cdc74577`
Tree: `30ef2ff8ebfb7f89c8f91b3c18c08432c4fdfbd1`
Prior final main: `ac475b5a51e19c7618a424ca689657cdf75edcaa` / tree `a0d81da9a4d593c9ba7ec23dd59073eb0e501dc9`

## DEV.12 ROOT CAUSE AND FIX

Historical failures of `StrictWorkerIsTerminatedWhenStagingGrowthExceedsOutputBudget` had already received the expected `WorkerOutputLimitExceededException`; one recorded 610304 staged bytes while the test imposed an unrelated 524288-byte ceiling. The product security contract requires finite output growth, termination and no publication, not a 512 KiB scheduler-dependent overshoot ceiling.

Dev.12 changes only the test harness. The canary writes exactly 69632 bytes (64 KiB configured test budget + one 4096-byte block), flushes incrementally, then holds for two minutes. The existing strict launcher must observe 65537–69632 bytes and terminate it. Production worker limits, AppContainer/Job Object containment, poll interval and launcher code are unchanged.

TDD history:
- plan: `e2d50ca1279631a1dd1a51ab35fb170ef5357a02`
- RED: `ad223384400be1c5749e0b09e301f7ddd5565eda`, run `33271012596`, managed `99149375331` — 191/192 because the new canary mode intentionally did not exist.
- failed unbounded experiment: `8ae59ddb87411f64c647a66a0a7f941b13d37a78`, run `33271120406` — preserved.
- diagnostic analyzer RED: `5dfaabebaacacde1d6b99a8146d8302b7845ad6e`, run `33271235790` — CA1305 before tests; preserved.
- diagnostic culture fix: `139c1058bb560756fb6139b01c87f4b277bd28f2`, run `33271302074`.
- bounded write-and-hold behavior head: `f4c241b0895d06d2e44d72f31e07f141cdc74577`, run `33271379504`.

Behavior-head evidence:
- continuity job `99150338602` PASS
- managed job `99150338647`: 18/18 restore PASS; dependency audit 18 projects/18 frameworks/0 vulnerable-result packages; Release 0 warnings/0 errors; native Explorer PASS; development package/MakeAppx PASS; direct and registered COM Invoke PASS; product Bridge→Strict Worker→FFmpeg PASS; MP3 320000 bit/s; Unicode/metacharacter path; source and existing destination preserved; numbered collision publication; 192/192 managed PASS.
- static job `99150338472`: 72/72 static and 5/5 vectors PASS before expected generated-authority zero-diff failure.
- deterministic package A/B were byte-identical at SHA `2d441ab980035dd63a4101ffb0548a3c47f6adc3049f88055bc4dd6f41b8326e`; integrity check then correctly rejected the stale package manifest for the modified test file.

## HARD PRODUCT / SECURITY INVARIANTS

Preserve Explorer → native `IExplorerCommand` → fixed app-local Bridge → strict disposable EngineWorker/provider → fixed app-local FFmpeg → private staging → transactional numbered publication.

Preserve Unicode/metacharacter filenames; source preservation; existing-destination preservation; deterministic collisions; no shell command construction; no raw FFmpeg passthrough; no user-selected converter; no PATH lookup; no network dependency; no silent Strict→Compatibility fallback; no hostile media parsing in Explorer; Bridge/Host media/process neutrality; worker/provider-only parsers/codecs/plugins; no signing private keys in repo.

Gyan FFmpeg is development qualification input only and is NOT production redistribution approval.

Do not redesign the direct shell Bridge launch or direct Host launcher. Development B2 package identity/authentication remains qualified; production signed-package B2 must still be requalified.

## HEADED WINDOWS 11 LIMITATION

There is no real headed Windows 11 Explorer environment here. Do not claim modern submenu visual acceptance, mouse-driven acceptance, screenshots, headed Explorer failure matrix or end-user UI acceptance.

## OPEN SHIPPING BLOCKERS

1. headed Windows 11 modern Explorer acceptance
2. exact-build screenshots
3. Explorer crash/hang/failure headed matrix
4. production signed-package B2 requalification
5. status/cancel wire decision
6. replay/disconnect/reconnect/session acceptance
7. production FFmpeg redistribution/license/notices/signature/hash approval
8. signed production MSIX
9. clean Windows 11 VM install/update/uninstall
10. final security/fuzz/chaos/release audit
11. end-user acceptance

## ONE PRECISE NEXT TASK

After dev.12 generated authority and exact-main delivery are frozen, start `0.1.0-dev.13` with the status/cancel wire decision and the smallest protocol slice needed to expose real job status/cancellation semantics without rerouting normal conversion through Host unnecessarily. Work test-first and keep durable commits directly on GitHub `main` under `AGENTS.md`.

## RECURSIVE HANDOVER RULE

Every completed tranche must end with a new full copy-paste handover containing repo/default branch/live SHA/tree, prior authority, all commits, RED/GREEN history, run/job/artifact IDs, exact changes/reasons, executed tests/build/security outcomes, workspace hashes/counts, blockers, explicitly unverified claims, ONE precise next task, invariants, headed limitation, production signing/FFmpeg limitation, and this same recursive rule.
