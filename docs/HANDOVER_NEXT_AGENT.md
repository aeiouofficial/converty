# Converty 0.1.0-dev.10 — Next-Agent Handover

## Exact current authority state
- Repository: `https://github.com/aeiouofficial/converty`.
- Working branch: `dev/0.1.0-dev.10-b4`.
- Frozen pre-dev.10 main: `13ed46bcb5cb02f33965dace4adc5a3fb25e87fd`.
- Immutable dev.10 B4 behavior head: `f221563c790057344a94b4e60c309d4512a77c38`.
- Behavior qualification: run `33028554361`; managed `98375493893`; static `98375494099`.
- First fully-green generated-authority qualifier: `529216b3676b97e7a9e0b78333c2229ed3396794`, tree `af16e15820985e787e54fb0c659cf6005bd4df89`.
- First generated-authority qualification: run `33035768679`; managed `98397998679`; static `98397998510`.
- Workspace version: `0.1.0-dev.10`; next workspace after closure: `0.1.0-dev.11`.

## What B4 behavior proved
On Windows Server 2025 / .NET SDK `10.0.400`: 18/18 locked restore, zero-vulnerability audit, 0-warning/0-error Release build, native Explorer, pinned development FFmpeg/ffprobe 9.0.1, unsigned MakeAppx package, direct DLL and package COM activation/Invoke, actual Strict Bridge→EngineWorker→FFmpeg conversion, Unicode/metacharacter handling, source/external-destination preservation, numbered publication, MP3 exactly 320000 bit/s, 190/190 managed tests, 66/66 static tests, 5/5 vectors, strict output-limit canary, and the direct filesystem/network/descendant containment canaries.

B4 includes private staging, worker/provider separation, suspended launch, explicit inherited handles, Job Object kill-on-close with finite process/memory/CPU/time/output ceilings, zero-capability AppContainer Strict profile, constrained ACLs, reparse rejection, no-network/outside-write denial and no silent Strict→Compatibility fallback.

## First generated-authority qualification — PASS
Run `33035768679` was fully green at qualifier `529216b3676b97e7a9e0b78333c2229ed3396794`:
- generated authority regeneration PASS and tracked generated-authority diff CLEAN;
- all managed product/build/test gates PASS; 190/190 tests, 0 skipped;
- static tests 66/66 and vectors 5/5 PASS;
- deterministic double ZIP PASS;
- ZIP `Converty_0.1.0-dev.10_full_workspace.zip`: SHA-256 `ed2fd33e376eef060f9342a77a48cdff40a9e2c95e0c6dc2d0ef98c557197241`, 377093 bytes, 328 files;
- 326 package-manifest entries and 327 SHA entries verified;
- ZIP reopen/CRC PASS and exclusions PASS;
- verified delivery artifact `9631969967`, digest `23de3e391ddb76ef8ddbf70c05f22a3fcc307a621692dc9759001c80741ad119`, 388508 bytes;
- generated-authority artifact `9631932538`, digest `f761f6148e7e60c8a64eb41a87592e0f107f872ff813487bc17844da49d6a313`.

## Why authority closure is still not final
This handover/evidence source update records that prior fully-green qualification. It therefore changes repository bytes and intentionally makes package/hash generated authority stale again. The next operation is a final regeneration from this evidence-frozen tree followed by one independent exact-head qualification. Do not rewrite B4 behavior.

## Single highest-priority next task
1. Regenerate exactly `machine-readable/source_sbom.spdx.json`, `machine-readable/release_sbom.spdx.json`, `machine-readable/package_manifest.json`, and `SHA256SUMS.txt` from the evidence-frozen source head.
2. Commit only those generated files separately.
3. Trigger permanent exact-head CI without changing tree bytes.
4. Require static freshness CLEAN and all managed behavior/product/tests plus deterministic ZIP/CRC/manifest/SHA/exclusion and verified delivery upload PASS.
5. Record final exact SHA/run/job/artifact metadata without creating a self-referential package assertion.
6. Only then consider reviewed dev.10 merge/fast-forward to main.

## Remaining shipping blockers after dev.10 authority closure
- real headed Windows 11 modern Explorer context-menu acceptance and exact-build screenshots;
- Explorer crash/hang/failure headed matrix;
- remaining B2 connected-server anti-squatting/authentication, final status/cancel wire decision, replay/disconnect/session acceptance;
- production FFmpeg redistribution/license/notices/signature/hash approval; Gyan FFmpeg remains development input only;
- signed production MSIX;
- clean Windows 11 VM install/update/uninstall;
- final security/fuzz/chaos/release audit and end-user shipping acceptance.

## Explicitly unverified / forbidden claims
Do not claim Converty is shipped/production-ready, headed Windows 11 Explorer acceptance is complete, B2 is fully closed, production FFmpeg redistribution is approved, Windows artifacts are production-signed, or clean-VM lifecycle acceptance is complete.

## Non-negotiable product/security boundaries
- Explorer is trigger-only, cheap and bounded; no media parsing, codecs/plugins or network work.
- Host never parses hostile media or loads codec/plugin code.
- Production media parsing/conversion occurs only in disposable restricted workers.
- Strict local conversion has no network capability.
- No raw commands or pass-through FFmpeg argument vectors from Explorer/user/IPC; checked-in typed presets only.
- No shell execution.
- Worker and FFmpeg use fixed app-local trusted paths; unsafe reparse-point substitution is rejected.
- Worker receives private staging, not final publication destination.
- Never overwrite input or externally created destination; numbered transactional no-overwrite publication remains default.
- Strict and Compatibility are explicit; never silently fall back from Strict.
- Signing private keys never enter repository/workspace.
- Report only executed gates.
- Gyan FFmpeg is development qualification input only, not production redistribution approval.

## Mandatory recursive handover rule
At the end of your work — whether complete, partial, or blocked — you MUST produce a new full-context copy-paste handover prompt for the NEXT agent/new chat. It must contain:
- exact repository and working branch/main HEAD SHA;
- exact immutable qualification SHA and GitHub Actions run/job IDs;
- what you changed and why;
- tests and acceptance gates actually executed with exact results;
- remaining shipping blockers and explicit unverified claims;
- the single highest-priority next task;
- all non-negotiable product/security constraints;
- and this exact recursive rule requiring that next agent to hand over again.

Repeat that handover chain at every tranche until Converty is actually shipped.
