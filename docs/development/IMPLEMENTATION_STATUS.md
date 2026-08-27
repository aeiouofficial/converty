# Implementation status — 0.1.0-dev.10

## Tranche result
`0.1.0-dev.10` behavior-qualifies B4 disposable-worker containment while preserving the functional Explorer product introduced by ADR-013:

`IExplorerCommand → fixed Bridge → Strict EngineWorker → typed preset/provider → fixed app-local FFmpeg → private staging → validated numbered publication`

The immutable B4 behavior head remains `f221563c790057344a94b4e60c309d4512a77c38`. The first generated-authority tree has also passed full permanent CI; this source evidence-freeze records that prior run. A final generated-authority regeneration/requalification is still required because this evidence update changes repository bytes.

## Behavior qualification
Immutable dev.10 B4 behavior head: `f221563c790057344a94b4e60c309d4512a77c38`.
GitHub Actions run: `33028554361`; managed job `98375493893`; static job `98375494099`.

Executed behavior results:
- 18/18 managed projects locked restore PASS.
- NuGet vulnerability audit PASS; 18 projects / 18 frameworks / 0 vulnerable-result packages.
- Release build PASS; 0 warnings, 0 errors.
- Native Explorer, development FFmpeg/ffprobe 9.0.1, unsigned MakeAppx package, direct DLL Invoke and packaged COM activation/Invoke: PASS.
- Real strict Bridge→EngineWorker→FFmpeg product smoke PASS with Unicode/metacharacter path, source preservation, pre-existing base destination preservation, numbered output and MP3 exactly 320000 bit/s.
- Microsoft Testing Platform/xUnit: 190/190 PASS, 0 skipped.
- Contract vectors 5/5 PASS; repository verifier PASS; static tests 66/66 PASS.
- Direct strict canary run `33027104465` proves private staging write, outside-scope write denial, loopback-network denial and Job Object descendant/resource containment.
- Output-growth canary PASS; default ceiling 8 GiB, hard configuration maximum 16 GiB.

## First generated-authority qualification
Generated authority tree: `af16e15820985e787e54fb0c659cf6005bd4df89`.
Qualifier commit: `529216b3676b97e7a9e0b78333c2229ed3396794`.
Permanent run: `33035768679`; managed `98397998679`; static `98397998510`.

Executed closure results:
- generated source/release SBOM, package manifest and SHA256SUMS regeneration PASS;
- tracked generated authority current / zero diff PASS;
- 18/18 locked restore and zero-vulnerability audit PASS;
- Release build PASS, 0 warnings, 0 errors;
- native/package/COM/strict product smokes PASS;
- managed tests 190/190 PASS, 0 skipped;
- static tests 66/66 PASS; vectors 5/5 PASS;
- deterministic workspace ZIP built twice with identical bytes;
- `Converty_0.1.0-dev.10_full_workspace.zip` SHA-256 `ed2fd33e376eef060f9342a77a48cdff40a9e2c95e0c6dc2d0ef98c557197241`, 377093 bytes, 328 files;
- 326 package-manifest entries and 327 SHA256SUMS entries verified;
- ZIP reopen/CRC PASS; exclusion policy PASS;
- verified delivery artifact upload PASS: artifact `9631969967`, digest `23de3e391ddb76ef8ddbf70c05f22a3fcc307a621692dc9759001c80741ad119`, 388508 bytes.

## B4 containment implemented and qualified
- Unique Converty-owned private staging; worker never receives final publication destination.
- Core owns only the worker-client boundary; FFmpeg execution is isolated in EngineWorker/provider.
- Typed worker CLI and checked-in presets only; no raw FFmpeg argument surface and no shell.
- Suspended native launch, explicit inherited handles, Job Object assignment before resume, kill-on-close and finite process/memory/CPU/time/output bounds.
- Zero-capability AppContainer Strict profile with application read/execute and staging read/write ACLs; reparse-point rejection and cleanup.
- No-network and outside-scope-write canaries PASS; no silent Strict→Compatibility fallback.
- Transactional/race-safe numbered publication preserves original and externally created destinations.

## Authority closure state
The first generated-authority qualification is green and retained as non-self-referential evidence. This evidence-freeze changes repository bytes, therefore generated package/hash authority must now be regenerated from the evidence-frozen tree and one final exact-head permanent run must again prove clean generated authority, behavior gates, deterministic ZIP verification and delivery upload. Only after that reviewed green result should dev.10 be considered for merge/fast-forward to `main`.

## Remaining shipping gates
1. Real headed Windows 11 Explorer acceptance and current-build screenshots, plus Explorer crash/hang/failure headed matrix.
2. Remaining B2 connected-server anti-squatting/authentication, final status/cancel wire decision, and replay/disconnect/session acceptance.
3. Production FFmpeg redistribution/license/notices/signature/hash approval; Gyan 9.0.1 remains development qualification only.
4. Signed production MSIX and clean Windows 11 VM install/update/uninstall acceptance.
5. Final security/fuzz/chaos/release audit and end-user shipping acceptance.

## Boundary status
Explorer remains trigger-only. Host remains media/process neutral. Core coordinates typed presets, staging and publication but does not launch FFmpeg. Media parsing/conversion occurs in a disposable strict worker/provider process. Strict local conversion has no network capability. Original inputs and externally created destinations are never overwritten.
