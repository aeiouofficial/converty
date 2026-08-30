# dev.13 Status/Cancel TDD Evidence

This file preserves development evidence for the authenticated status/cancel tranche. Final release authority still requires an ordinary successful CI run on the exact current `main` HEAD with generated authority current.

## Task 1 — contracts and strict JSON
RED `beabdc7fe0fec0e9d2e0f9f6add4fefa9eaa593b`, run `33285343578`, managed `99187464826`: restore/audit passed; Release build failed because the new tests referenced `JobControl*` contracts/serializers that did not exist. Static pre-gates passed; generated-authority freshness and side-branch continuity failed separately as expected.

GREEN added `JobControlOperation`, `JobControlFailureReason`, `JobControlRequest`, `JobControlResponse`, strict canonical-D control GUID parsing and strict V1 request/response serialization without changing legacy conversion GUID parsing.

## Task 2 — Host dispatch
Valid RED `01ac60213d91ed14b721fbe954fbb0c5143e5de3`, run `33285816631`, managed `99188726185`: Build/native/package/COM/product path passed; managed Test failed because valid control input still followed legacy admission handling.

GREEN changed only `HostRequestHandler`: authorization and bounds stay first; strict control classification dispatches to existing `HostJobQueue.TryGet` / `TryCancel`. `HostPipeServer` and `HostJobQueue` production semantics were not redesigned.

## Task 3 — Bridge control client
RED `7954def408d79e5bf31b783b472df2d02669d2d4`, run `33286056932`, managed `99189341816`: restore/audit passed; Build failed because `IBridgeJobControlClient` / status/cancel methods did not exist.

First GREEN `d51366981f1417a31309b391f23b7fe203ab1673` compiled production Bridge but hit CA1859-as-error in six new test locals. Analyzer-only test correction `630b622dc7d2d1c8df9a9ce7b44c37efc28c9401`, run `33286255211`, proved Build/native/package/COM/product/Test green before stale-authority packaging.

Bridge now implements a separate control interface but reuses one authenticated exchange helper. Source ordering is connect → `VerifyConnectedServer` → first `WriteAndFlushAsync`. Each request uses a fresh pipe.

## Task 4 — adversarial/static hardening
Expanded the IPC corpus from 7 to 12 with `control-unknown-member`, `control-duplicate-operation`, `control-noncanonical-job-id`, `control-future-schema`, and `control-hybrid-conversion-members`.

Static hardening head `3bf7ebdc7cc6b9f0456658a214ae35dc0eb1d535`, run `33318809296`, exposed one test-authoring error: 77 passed, one assertion expected nonexistent `PeerAuthorization.Unauthorized`. Production code was unchanged. Corrected head `84f1b2502c912633c8fb019da3d6860e6891cf9c`, run `33318858033`, passed all 78 static tests before generated-authority freshness.

## Pre-version behavior qualification
At `84f1b2502c912633c8fb019da3d6860e6891cf9c`, run `33318858033`, managed `99277143724`:
- 18/18 locked restore, dependency audit 18 projects/18 frameworks/0 vulnerable-result packages;
- Release build 0 warnings / 0 errors;
- native Explorer, development MakeAppx package, direct and registered COM Invoke PASS;
- product Bridge→Strict Worker→FFmpeg PASS with Unicode/metacharacter path, source/existing-destination preservation, numbered output and MP3 exactly 320000 bit/s;
- 248/248 managed PASS, 0 skipped;
- 78/78 static PASS, 5/5 vectors, repository verifier PASS after in-job generation;
- workspace A/B were byte-identical (`acd23ecb25fa2f1118c8ef18f5fadd7d2f281f04513368037dbfbda34f44f782`, 422404 bytes, 348 files), then embedded-manifest verification correctly rejected stale tracked authority at `src/Converty.Bridge/Ipc/BridgeClient.cs`.

No headed Windows 11 UI claim is made by this evidence.
