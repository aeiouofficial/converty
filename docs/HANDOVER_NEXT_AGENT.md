# Converty 0.1.0-dev.10 — Next-Agent Handover

## Exact current authority state
- Repository: `https://github.com/aeiouofficial/converty`.
- Working branch: `dev/0.1.0-dev.10-b4`.
- Frozen pre-dev.10 main: `13ed46bcb5cb02f33965dace4adc5a3fb25e87fd`.
- Immutable dev.10 B4 behavior head: `f221563c790057344a94b4e60c309d4512a77c38`.
- Immutable B4 qualification: run `33028554361`; managed `98375493893`; static `98375494099`.
- Pre-audit final authority `c57e0188d24adf7bd81e15e7860648aa9fc9035c` / run `33036173795` was green but is now superseded by post-qualification security hardening.
- Post-audit security behavior head: `b0ec6445abe7d7d2b53329ba7b4f2a07c51b7eed`.
- Post-audit qualification: run `33044395097`; managed `98424925818`; static `98424926101`.
- Workspace version: `0.1.0-dev.10`; next workspace after closure: `0.1.0-dev.11`.

## Why the previously final dev.10 authority is superseded
A focused pre-merge security audit found two concrete reparse-ancestry gaps after the earlier fully-green authority had been produced:
1. `WorkerFileSystemScope` rejected only the leaf writable staging directory. A normal `job-*` directory below a reparse-point ancestor could therefore pass the leaf check.
2. `TrustedFfmpegPath` checked the application root, `tools/ffmpeg`, and `ffmpeg.exe`, but did not reject an intermediate `tools` reparse point.

Both were fixed test-first and without changing strict/compatibility fallback, resource limits, network isolation, publication, preset, shell, or FFmpeg argument semantics.

## RED/GREEN evidence
### Staging ancestry
- RED head `ee6362532db6fe2dfbd8b29306660b936a6ff597`, run `33043835266`, managed `98423183994`, static `98423183716`.
- Expected RED observed: 191 total, 190 passed, 1 failed, 0 skipped; the new test proved no exception was thrown for a child under a reparse ancestor.
- Fix head `ba19112673ba6514586ffd735f3322bae1203f9c`: `WorkerFileSystemScope` now walks existing directory ancestry and fails closed on any reparse component.

### Fixed FFmpeg path ancestry
- RED head `0cb5ca28107c9905e944cf9104c661060766dec9`, run `33044109667`, managed `98424034894`, static `98424035106`.
- Expected RED observed: 192 total, 191 passed, 1 failed, 0 skipped; an intermediate `app/tools` symlink was accepted before the fix.
- Fix heads `16ef67ac55750901d8911900322236702272987b` and verifier-compatible `b0ec6445abe7d7d2b53329ba7b4f2a07c51b7eed`: `tools` is now explicitly reparse-checked while retaining literal fixed `Path.Combine(root, "tools", "ffmpeg")` repository policy.

## Post-audit behavior qualification — PASS, generated authority intentionally stale
Run `33044395097` at `b0ec6445...` observed:
- 18/18 locked restore PASS;
- vulnerability audit PASS: 18 projects / 18 frameworks / 0 vulnerable-result packages;
- Release build PASS: 0 warnings / 0 errors;
- native Explorer Release PASS;
- pinned development FFmpeg/ffprobe 9.0.1 PASS;
- unsigned MakeAppx development package PASS;
- direct shell DLL class-factory + Invoke PASS;
- loose package registration + packaged COM activation/Invoke PASS;
- actual Strict Bridge→EngineWorker→FFmpeg product smoke PASS;
- Unicode/metacharacter source, source preservation, existing destination preservation, numbered output PASS;
- ffprobe MP3 exactly 320000 bit/s PASS;
- managed tests **192/192 PASS**, 0 failed, 0 skipped;
- static tests **66/66 PASS**; contract vectors **5/5 PASS**; repository verifier PASS;
- deterministic workspace ZIP built twice with identical SHA-256 `ec89921565e89d092a63dc3a7e6d0768ae580ee72805f6a67837571bda3551ed`, 378809 bytes, 329 files.

Packaging then failed exactly when verifying tracked generated authority against changed source (`providers/Converty.Provider.FFmpeg/TrustedFfmpegPath.cs`). Static freshness likewise failed after all substantive static gates passed. This is expected and means current post-audit generated/release authority is NOT frozen yet.

## Single highest-priority next task
1. Commit this post-audit evidence synchronization.
2. Regenerate exactly `machine-readable/source_sbom.spdx.json`, `machine-readable/release_sbom.spdx.json`, `machine-readable/package_manifest.json`, and `SHA256SUMS.txt` from that evidence-synchronized tree.
3. Commit only those four generated files separately.
4. Require final exact-head CI to prove generated-authority zero diff, all 192 managed tests, all substantive static gates, deterministic ZIP twice, ZIP reopen/CRC, manifest bytes/hashes, SHA entries, exclusion policy, and verified delivery upload.
5. Record final exact-head run/artifact evidence externally (for example PR body) without mutating the final archive tree into self-reference.
6. Do not merge solely because CI is green; PR review/release-policy preconditions remain separate.

## Remaining shipping blockers after dev.10 authority closure
- real headed Windows 11 modern Explorer context-menu acceptance and exact-build screenshots;
- Explorer crash/hang/failure headed matrix;
- remaining B2 connected-server anti-squatting/authentication, final status/cancel wire decision, replay/disconnect/session acceptance;
- production FFmpeg redistribution/license/notices/signature/hash approval; Gyan FFmpeg remains development input only;
- signed production MSIX;
- clean Windows 11 VM install/update/uninstall;
- final security/fuzz/chaos/release audit and end-user shipping acceptance.

## Explicitly unverified / forbidden claims
Do not claim Converty is shipped/production-ready, current post-audit dev.10 generated/release authority is frozen before final regeneration/requalification, headed Windows 11 Explorer acceptance is complete, B2 is fully closed, production FFmpeg redistribution is approved, Windows artifacts are production-signed, or clean-VM lifecycle acceptance is complete.

## Non-negotiable product/security boundaries
- Explorer is trigger-only, cheap and bounded; no media parsing, codecs/plugins or network work.
- Host never parses hostile media or loads codec/plugin code.
- Production media parsing/conversion occurs only in disposable restricted workers.
- Strict local conversion has no network capability.
- No raw commands or pass-through FFmpeg argument vectors from Explorer/user/IPC; checked-in typed presets only.
- No shell execution.
- Worker and FFmpeg use fixed app-local trusted paths; unsafe reparse-point substitution is rejected, including staging ancestry and the FFmpeg `tools` path component.
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
