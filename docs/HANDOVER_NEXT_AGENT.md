# Converty — single current repository OPEN handover
Status: OPEN / NOT CUSTOMER SHIP-READY

Repository: aeiouofficial/converty
Branch: dev/0.1.0-dev.22-shipping-readiness
Workspace version: 0.1.0-dev.22. Planned next workspace version: 0.1.0-dev.23 only after dev.22 closure.
Exact verified workspace-staging code/evidence subject: 3ddac2be5a16ab93dcd383786fc648aeca69271a
Tree: 115a7424521f13182778b2a28418d30b8ed3a38e
Frozen main: 8a1f46603aa842728247bc11b34fcccf121858fd — DO NOT MOVE.
Main ruleset 24086373 ACTIVE, main protected, no bypass.
Draft PR #14. External shipping gates issue #13.
Slack continuation: Handover #13 TS 1790641889.866779 OPEN; predecessor #12 TS 1789698886.030639 PROCESSED.
GitHub is authoritative; Slack and Drive are synchronized mirrors.

## Completed and verified in the latest engineering block
A continuation audit found a concrete storage-contract defect: ConversionStagingDirectory ignored CONVERTY_WORKSPACE_ROOT and could place private conversion staging under LocalApplicationData on C: during workspace-qualified local work.

Fix behavior:
- with explicit workspace authority, require a fully-qualified non-C root;
- stage under <workspace>\_temp\runtime\Converty\WorkerStaging;
- reject relative/C:-root workspace authority fail-closed;
- preserve normal installed-product LocalApplicationData fallback when no workspace authority is present;
- regression coverage includes a source guard plus real ConversionBatchRunner staging assertion.

Evidence bound to exact code subject 3ddac2be5a16ab93dcd383786fc648aeca69271a:
- predecessor b1485ce committed source RED: no workspace-root staging support;
- focused workspace storage static 3/3 PASS;
- Converty.Core.Tests 152/152 PASS;
- full managed 395/395 PASS, zero skipped;
- full static 169/169 PASS;
- contract vectors 5/5 PASS;
- release-input, repository and immutable Action-pin preflight PASS;
- four generated authority files deterministic and exact HEAD zero-diff PASS;
- two independent exact Git-HEAD workspace ZIPs byte-identical: SHA-256 67ba2ffc12bb8ebea22fb252482cd42024493b1c440e16938e13634188443b7d, 673449 bytes / 466 entries, CRC PASS, 464/464 manifest, 465/465 SHA rows, 4/4 exact authority PASS;
- GitHub branch readback matched; no hosted Actions; frozen main unchanged.

## Cloud state
Slack #12 is PROCESSED; #13 TS 1790641889.866779 is the single OPEN continuation. Canonical Drive Authority, Roadmap, Plan, Tasks, Changelog, Release/Test Evidence and Recursive Handover were reconciled in place to the exact code/evidence subject above.
Drive IDs:
Authority 1ZdDGUpSVxeEfvICLKD_VctT49MlJMyhNICyj4ebeYRw
Roadmap 1p3xKxj2akSqZTzVp442QNetoZ8Eg9u6pvckjwUnBLsI
Plan 1eGVajQAxw3Vjc7F_7NJgt9do6tRzZpV_Vbfcl24g9-s
Tasks 1BH44EUYcNBexIZasxq24mlYBZk0VxF5XaG6RnUkQPrc
Changelog 1JsJfEECcWaB2UJtW0oiW45RD86RZT4i5spANV38Zzoc
Evidence 1LizDehSMDnBfihXnntX9z13QNai87zzMwlkzPptkcB0
Recursive 1HVfL2KV6LZbpl0fc4Je1dzqLs3ya9q9Onjb3YbFF9L8

## Existing product evidence and OPEN release gates
Packaged-development product evidence remains bound to unchanged source 6292ccb0a2a1eb5bfcb3dbe23bc99fddc50d5ce3: exact pinned dev-only FFmpeg/ffprobe, unsigned MSIX layout, packaged ProbeWorker, direct staged Explorer COM, Audio36/Image24/Video27, Copy/Remux/Transcode and negative/mixed PASS. It is not production approval.

Registered unsigned MSIX COM on the current laptop remains blocked by Windows 0x80073CFF. Do not alter sideload/developer security policy. Production FFmpeg provenance/license/redistribution, signed MSIX/B2, clean headed Windows 11 Explorer and install/update/uninstall lifecycle, final security/fuzz/chaos/end-user acceptance, UX/settings and Plugin SDK, and independent release review remain OPEN. PR #14 stays DRAFT.

## Exact next executable task
Fresh-read the current GitHub documentation successor, Slack #13 and Drive mirrors, then continue full dev.22 correctness/security review. Flag/fix only confirmed defects with plausible downstream failure and RED-to-GREEN evidence. If an expressly approved/provisioned Windows 11 lifecycle host becomes available, prioritize real registered MSIX COM + headed Explorer + install/upgrade/uninstall evidence against exact artifact bytes.

Acceptance for the next block:
- concrete defect evidence or genuine external lifecycle evidence, not speculation;
- required local tests/gates PASS;
- all Converty-owned source/build/temp/cache/toolchain/logs remain within D:\converty;
- no Actions dispatch, no governance bypass and no main movement;
- repo, Drive and Slack reconciled; predecessor processed before one new OPEN successor.

## Security invariants
Explorer -> fixed app-local Bridge -> private staging -> strict RO ProbeWorker/fixed ffprobe -> typed facts -> bounded planner -> strict EngineWorker -> managed byte-exact Copy or provider-owned fixed FFmpeg Remux/Transcode -> post-probe TargetMediaContract -> durable same-folder no-overwrite publication.
No raw shell FFmpeg tokens, PATH/CWD executable discovery, silent Strict-to-Compatibility fallback, ordinary conversion network, arbitrary plugins, GPU acceleration, private signing keys, hand-edited generated authority, force push or ruleset bypass.
