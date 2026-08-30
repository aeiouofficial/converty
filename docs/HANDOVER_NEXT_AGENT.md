# CONVERTY — CONTINUATION HANDOVER
# PRODUCT-FIRST ROADMAP — 0.1.0-dev.13 STATUS/CANCEL WIRE

Continue development directly in `https://github.com/aeiouofficial/converty`. Default branch: `main`.

Repository `main` is the only durable completed authority. Re-fetch live `main` before every write and completion/freeze claim. The dev.13 implementation branch is a clean fast-forward line from planning authority `3ccbe5a4e42ee84c8d4616f826ca36997718d4c7`; final main/generated-authority freeze is still pending at this metadata stage.

## DEV.13 DELIVERED BEHAVIOR
- Existing conversion admission is unchanged.
- New typed one-shot control request: schema version + `status`/`cancel` + canonical-D non-empty job ID.
- Status returns existing `JobStatusSnapshot`.
- Cancellation is queued-only and transactional through existing queue/journal. Unknown → `jobNotFound`; non-queued → `notCancellable`; journal failure with unchanged queued state → `persistenceFailure`.
- Bridge implements separate `IBridgeJobControlClient`, opens a fresh pipe for every call, and authenticates the connected Host before first application-frame write.
- Host expected-user authorization remains before application-frame parsing.
- No second pipe, persistent session protocol, polling service, UI, or Host reroute of normal conversion was added.
- IPC hostile corpus is now 12 cases; static gates lock the approved architecture.

## TDD / QUALIFICATION EVIDENCE
- Task 1 RED `beabdc7fe0fec0e9d2e0f9f6add4fefa9eaa593b`, run `33285343578`, managed `99187464826`.
- Task 2 RED `01ac60213d91ed14b721fbe954fbb0c5143e5de3`, run `33285816631`, managed `99188726185`.
- Task 3 RED `7954def408d79e5bf31b783b472df2d02669d2d4`, run `33286056932`, managed `99189341816`.
- Analyzer-only test correction behavior `630b622dc7d2d1c8df9a9ce7b44c37efc28c9401`, run `33286255211`.
- Strengthened pre-version behavior head `84f1b2502c912633c8fb019da3d6860e6891cf9c`, tree `959a994c5a533e85cb79b1a1ff1d2e73f11c62d5`, run `33318858033`, managed `99277143724`, static `99277143808`, continuity `99277143780`.
- Observed: 18/18 locked restore; 0 vulnerable-result packages; Release 0 warnings/errors; native/package/MakeAppx/direct+registered COM/product conversion PASS; Unicode/metacharacter paths; source/existing destination preserved; numbered output; MP3 320000 bit/s; 248/248 managed; 78/78 static; 5/5 vectors.
- Workspace A/B were byte-identical at SHA-256 `acd23ecb25fa2f1118c8ef18f5fadd7d2f281f04513368037dbfbda34f44f782`, 422404 bytes, 348 files, then correctly failed embedded-manifest verification because tracked generated authority still described old Bridge bytes.

## HARD INVARIANTS
Preserve Explorer → native `IExplorerCommand` → fixed app-local Bridge → strict disposable EngineWorker/provider → fixed app-local FFmpeg → private staging → transactional numbered no-overwrite publication.

Preserve Unicode/metacharacter filenames; source and existing-destination preservation; no shell command construction; no raw FFmpeg passthrough; no user-selected converter; no PATH lookup; no network dependency; no silent Strict→Compatibility fallback; no hostile media parsing in Explorer; Host/Bridge media-neutral; parsers/codecs/plugins worker/provider-only; no signing private keys in repo.

Gyan FFmpeg remains development qualification input only, not production redistribution approval. Do not redesign the direct shell Bridge launch or fixed Host launcher without new evidence.

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
First finish dev.13 closure: qualify this version/metadata candidate, synchronize only the four runner-generated authority files from the exact candidate, fast-forward `main`, and require ordinary CI success on exact current `main` including zero-diff authority and deterministic verified `Converty_0.1.0-dev.13_full_workspace.zip`. After that, dev.14 is replay/disconnect/reconnect/session acceptance for existing one-shot admission/status/cancel IPC without adding unnecessary persistent-session architecture.

## RECURSIVE HANDOVER RULE
Every completed tranche must end with a new full copy-paste handover containing repo/default branch/live SHA/tree, prior authority, all commits, RED/GREEN history, run/job/artifact IDs, exact changes/reasons, executed tests/build/security outcomes, workspace hashes/counts, blockers, explicitly unverified claims, ONE precise next task, invariants, headed limitation, production signing/FFmpeg limitation, and this same recursive rule.
