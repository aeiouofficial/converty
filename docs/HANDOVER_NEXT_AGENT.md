# Converty 0.1.0-dev.10 — Next-Agent Handover

## Exact current authority state
- Repository: `https://github.com/aeiouofficial/converty`.
- Working branch: `dev/0.1.0-dev.10-b4`.
- Frozen main before dev.10: `13ed46bcb5cb02f33965dace4adc5a3fb25e87fd`.
- Immutable dev.10 B4 behavior head: `f221563c790057344a94b4e60c309d4512a77c38`.
- Behavior qualification run: `33028554361`; managed job `98375493893`; static job `98375494099`.
- Workspace version authority is now `0.1.0-dev.10`, but generated SBOM/package/hash authority is intentionally not frozen until regenerated from this synchronized tree and exact-head CI is fully green.

## What B4 behavior proved
On Windows Server 2025 / `windows-2025-vs2026` / .NET SDK `10.0.400`:
- 18/18 locked restore PASS; vulnerability audit 18 projects/18 frameworks/0 vulnerable-result packages.
- Release build PASS, 0 warnings, 0 errors.
- native Explorer, pinned dev FFmpeg/ffprobe 9.0.1, MakeAppx unsigned package, direct DLL Invoke, loose package registration + COM activation/Invoke all PASS.
- actual `Bridge → Strict EngineWorker → FFmpeg` product smoke PASS with Unicode/metacharacter filename, source preservation, pre-existing destination preservation, numbered output, MP3 exactly 320000 bit/s.
- 190/190 managed tests PASS, 0 skipped; static tests 66/66 PASS; contract vectors 5/5 PASS.
- private staging, worker/provider split, suspended launch, explicit handle list, Job Object kill-on-close, process/memory/CPU/time/output ceilings, zero-capability AppContainer, constrained ACLs, no-network/outside-write canaries and no Strict→Compatibility fallback are implemented.
- output-growth canary proves a strict worker is terminated after exceeding a 64 KiB budget. Conversion default is 8 GiB; hard configured maximum is 16 GiB.
- direct strict-canary run `33027104465` (managed `98370929641`, static `98370929814`) proves staging write allowed, sibling/outside denied, loopback connection denied and descendant/resource containment.

The behavior run built two byte-identical workspace ZIPs: SHA-256 `a0edd6e15a63d71cc2ef493ef33f6bb6e3f0b16ee0d8f484ebc981b800f749de`, 369035 bytes, 328 files. It then failed exactly on stale tracked `machine-readable/package_manifest.json`, so delivery upload was correctly skipped.

## Single highest-priority next task
Finish **dev.10 generated-authority closure** without changing the frozen B4 behavior:
1. let CI regenerate source/release SBOM, package manifest and SHA256SUMS from the synchronized dev.10 authority tree;
2. commit exactly those generated files as a separate generated-authority commit;
3. require generated-authority zero diff;
4. run exact-head CI and require all behavior gates plus deterministic ZIP twice, ZIP reopen/CRC, package-manifest bytes/hashes, SHA256SUMS, exclusions and verified delivery upload;
5. record the fully-green authority qualification in machine-readable/handover evidence using the established non-self-referential evidence-freeze pattern, regenerate authority again if that evidence update changes generated manifests, and independently requalify the final tree;
6. only after fully-green reviewed authority consider merging/fast-forwarding dev.10 to main.

## Remaining shipping blockers after dev.10 authority closure
- real headed Windows 11 modern Explorer menu visibility/usability and fresh exact-build screenshots;
- Explorer crash/hang/failure headed matrix;
- remaining B2 connected-server anti-squatting/authentication, final status/cancel wire decision, replay/disconnect/session acceptance;
- production FFmpeg redistribution/license/notices/signature/hash approval; Gyan FFmpeg is development input only;
- signed production MSIX;
- clean Windows 11 VM install/update/uninstall;
- final security/fuzz/chaos/release audit and end-user shipping acceptance.

## Explicitly unverified / forbidden claims
Do not claim Converty is shipped/production-ready, headed Windows 11 Explorer acceptance is complete, B2 is fully closed, production FFmpeg redistribution is approved, Windows artifacts are production-signed, or clean-VM lifecycle acceptance is complete.

## Non-negotiable product/security boundaries
- Explorer is trigger-only, cheap and bounded; it never parses media or loads codecs/plugins.
- Host never parses hostile media or loads codec/plugin code.
- Production media parsing/conversion occurs in disposable restricted workers.
- Strict local conversion has no network capability.
- No raw command strings or pass-through FFmpeg argument vectors from Explorer/user/IPC.
- FFmpeg arguments come only from checked-in typed presets; no shell execution.
- Worker and FFmpeg paths are fixed app-local trusted paths; unsafe reparse-point substitution is rejected.
- Worker receives private staging, never final publication destination.
- Never overwrite input or externally created destination; numbered no-overwrite publication remains race-safe default.
- Strict and Compatibility are explicit; never silently retry Compatibility after Strict failure.
- Signing private keys never enter repository/workspace.
- Report only gates actually executed.
- Gyan FFmpeg remains development qualification input, not production redistribution approval.

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
