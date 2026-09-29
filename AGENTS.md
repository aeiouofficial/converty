# Converty repository operating contract

## Frozen main and development authority

`main` is the frozen release authority: `0.1.0-dev.20` at `8a1f46603aa842728247bc11b34fcccf121858fd`. Do not move, reset or merge into frozen main while real production release prerequisites remain OPEN.

The qualified dev.21 candidate `c3bc042aea3154e30ce2720db4722cbda27bcb34` is immutable development evidence. The current dev.22 development branch is `dev/0.1.0-dev.22-shipping-readiness`; GitHub code/CI on that branch is authoritative for ongoing development, **not customer ship-ready** and not release approval.

Main is the repository authority for frozen releases. Push durable commits immediately to GitHub, including development work on the active branch while main is frozen. A side branch is a temporary engineering surface; side-branch-only CI is development evidence, never production release authority. Before any production-completion claim, the qualified SHA must be the current `main` HEAD after genuine promotion and exact-main CI.

Push durable work to GitHub promptly. Never treat an unpushed local workspace, chat summary, stale Slack/Drive handover or a development CI run as final release authority. Keep `main` frozen unless the exact production candidate passes all real release, governance and independent-evidence gates. Do not force-push, auto-merge, bypass red shipping checks or hand-edit generated SBOM/package/hash files.

## Before and after an engineering block

1. Fresh-read the current GitHub branch, frozen main, CI, evidence, backlog, plan and current repository handover before any write.
2. Use TDD for code changes and preserve exact RED/GREEN evidence. Update canonical plan, backlog, changelog and handover in place.
3. On the exact candidate commit require all relevant managed/native/product/static checks, generated-authority zero-diff and independent delivery validation. A technically qualified development branch is not a production release.
4. Treat external signing, FFmpeg redistribution, live main governance, headed Windows 11, fuzz/chaos/security, UX/Plugin SDK and end-user acceptance as OPEN until independently verified.
5. GitHub is the code/release authority. Reconcile Slack and Drive only when the current user instruction includes cloud-documentation scope.

## Local storage — no Converty project files on C:

All local Converty source clones, archives, FFmpeg downloads, temp files, package caches, build outputs, diagnostic logs and test artifacts belong **inside a non-C project workspace**. On the connected laptop the workspace is `D:\converty`, with temporary data under `D:\converty\_temp`. Before local build/test/package commands, run `build/use-workspace-temp.ps1 -Workspace D:\converty` in the current PowerShell session; the script refuses C: and redirects `TEMP`, `TMP`, `TMPDIR`, pip, NuGet, npm and .NET CLI caches into that workspace.

Never create new Converty temp/cache directories under `C:\Users\...\Temp`. If existing Converty-owned files must be migrated from C:, copy to the non-C workspace, verify file counts and SHA-256 for every file, and only then delete the verified originals. Do not delete unknown user data or move the Windows OS, installed applications or unrelated caches. GitHub-hosted Windows runner system installations are separate ephemeral machines, not the user's local C: drive; project-owned output on those runners must stay inside their GitHub workspace.

## Product invariants

Preserve Explorer → fixed Bridge → private staging → read-only ProbeWorker/fixed ffprobe → typed facts/planner → strict EngineWorker → managed Copy or provider-owned fixed FFmpeg tokens → post-probe contract → destination-volume durable temp → transactional numbered no-overwrite publication. No shell construction, PATH/CWD executable lookup, raw FFmpeg arguments, silent Strict→Compatibility fallback, ordinary conversion network dependence, hardware acceleration or private signing keys in the repository.

Current external release blockers: GitHub issue #13. Development review PR: draft #14. Exact current commit and latest checks must always be re-read rather than copied from this file.
