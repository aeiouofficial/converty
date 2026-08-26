# Converty 0.1.0-dev.9 — Next-Agent Handover

## Current authority
- Delivered workspace: `0.1.0-dev.9`.
- Repository: `https://github.com/aeiouofficial/converty`.
- Shipping target: Windows 11 x64 modern Explorer context-menu converter.
- Next workspace: `0.1.0-dev.10`.
- Resolve the current `main` HEAD before changing source; the final frozen authority SHA may be later than the immutable dev.9 behavior head because generated authority is synchronized after behavior qualification.
- Immutable dev.9 behavior head: `b71aa06fb024afe85f64707b05d996e86c37d8c8`.
- Read `machine-readable/handover_state.json` and `machine-readable/build_evidence.json` before changing source.

## What dev.9 proved
Permanent GitHub Actions run `33001019450` on Windows Server 2025 / .NET SDK `10.0.400` proved:
- 15/15 locked restore; NuGet audit PASS with zero vulnerable-result packages;
- Release build PASS, zero warnings/errors;
- native MSVC Explorer DLL build PASS;
- pinned development FFmpeg/ffprobe 9.0.1 hash/execution PASS;
- MakeAppx unsigned development package PASS;
- direct staged DLL class-factory + `IExplorerCommand::Invoke` conversion PASS;
- loose package registration + packaged COM activation + `Invoke` conversion PASS;
- Bridge→FFmpeg product smoke PASS with Unicode/metacharacters, numbered collision handling, source/base-destination preservation, no partial leak, ffprobe MP3/320000 bit/s;
- 176/176 managed tests PASS;
- 54/54 static tests PASS after in-job authority generation.

This means the automated minimum product path exists. Do not revert it to an infrastructure-only prototype.

## Product-first decision
ADR-013 is authoritative: dev.9 was allowed to qualify a fixed development `Explorer → Bridge/Core → app-local FFmpeg` path before final B2/B4 closure. This is a **development exception**, not a production architecture waiver. Final shipping still requires worker containment and the other release gates.

## Immediate next task — highest priority
Perform **real headed Windows 11 Explorer acceptance** for the exact current development package:
1. use an interactive Windows 11 x64 environment, not Windows Server/headless COM-only evidence;
2. build/stage/register or install the current dev package using the repository scripts;
3. right-click a supported WAV in Explorer and prove `Converty` appears in the Windows 11 modern context menu, with the expected fixed submenu entries;
4. invoke `Convert to MP3` through the real Explorer UI;
5. prove the source remains, a pre-existing base MP3 is not overwritten, numbered output is created, output is non-empty and MP3/320k;
6. capture current-version screenshots/evidence of the menu and resulting files; do not reuse old screenshots;
7. add an executable/headed acceptance record to the repository without weakening automated CI.

If the current environment cannot provide headed Windows 11 Explorer, do not fake this gate. Continue the next code-bearing shipping task below and leave headed acceptance explicitly open.

## Next code-bearing shipping work after/alongside headed acceptance
Prioritize **B4 containment and migration of the dev.9 conversion spike into the final worker boundary**:
- private per-job staging;
- disposable restricted worker launcher;
- Job Object kill-on-close and memory/CPU/process/output/time limits;
- no-network canary;
- outside-scope file-write canary;
- strict-vs-compatibility profile with no silent downgrade;
- move FFmpeg process execution out of the Core/Bridge spike into the worker/provider architecture while preserving typed presets, numbered transactional output, Unicode paths and current Explorer UX.

Then finish the remaining B2 connected-server anti-squatting/status-wire/replay-session acceptance, qualify production FFmpeg licensing/redistribution/signature/hash/notices, create the signed production MSIX, perform clean Windows 11 VM install/update/uninstall, and run final security/fuzz/chaos/release gates.

## Non-negotiable boundaries
- Explorer remains trigger-only: no media parsing, FFmpeg loading, network or long-running conversion in Explorer.
- Host never parses hostile media and never loads codec/plugin code.
- Production media probing/conversion belongs in disposable restricted workers.
- Standard local conversion requires no network; strict worker profile denies it.
- Presets/IPC never expose arbitrary executable command text or pass-through argument vectors.
- Numbered copy remains the safe default; never overwrite source or an externally created destination.
- Strict isolation never silently downgrades.
- Production signing private keys never enter the repository/workspace.
- The Gyan FFmpeg development payload is not production redistribution approval.

## Required recursive handover rule
**At the end of your work, whether the next shipping milestone succeeds, partially succeeds, or is blocked, you MUST prepare a new complete copy-paste handover prompt for the following agent/new chat.** The new prompt must include the exact repository, branch/main HEAD SHA, immutable qualification SHA/run/job IDs, what changed, tests/gates actually executed, remaining blockers, the single highest-priority next task, shipping caveats, and this same recursive handover requirement. Continue this chain on every handoff until Converty is actually shipped.

Never claim a gate that was not executed. Never call an unsigned/dev-only package production-ready. At each tranche end synchronize version/docs/machine-readable authority, regenerate SBOM/package/hash authority, require zero generated diff, deterministically build/reopen/verify the workspace ZIP, update GitHub, and provide the next handover prompt.
