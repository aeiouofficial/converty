# Main Authority Continuity Guard Design

## Problem

Converty development was qualified for multiple tranches on `dev/0.1.0-dev.11-b2-auth` while the repository default branch `main` remained at an older authority. The code was present in GitHub, but the repository homepage and default branch did not show the current work. This made completed work appear missing and allowed a side-branch CI run to be mistaken for repository authority.

## Root cause

The repository had no agent operating contract requiring authoritative work to be pushed to `main`, and CI validated code quality without distinguishing a development-only side-branch run from final default-branch qualification.

## Required behavior

1. `main` is the repository authority.
2. Durable product/source/documentation changes must be committed and pushed to GitHub immediately; work must not exist only in a local workspace or chat handover.
3. Side branches are permitted for temporary RED reproduction, diagnostics, or explicitly isolated experiments.
4. A non-`main` push whose HEAD is not already contained in `origin/main` must receive a failing `main-authority-continuity` check with a clear development-only message.
5. Pull-request events remain usable for review and are not blocked by the push-only continuity rule.
6. Final qualification/completion claims require a fresh live read proving the qualified SHA is the current `main` HEAD and the ordinary CI run on that exact SHA is successful.
7. The guard must not auto-merge or auto-promote arbitrary side branches.
8. The guard must not change product runtime behavior, Explorer activation, Bridge/Host/Worker boundaries, FFmpeg policy, signing policy, or release semantics.

## Design

Add a repository-local `AGENTS.md` defining the main-first operating contract for future agents. Add `scripts/verify_main_continuity.py`, a small deterministic verifier that receives repository/ref/event/SHA context and executes a supplied Git ancestry check. In GitHub Actions, add a dedicated `main-authority-continuity` job that checks out full history and, on non-main push events, fails unless `git merge-base --is-ancestor "$GITHUB_SHA" origin/main` succeeds.

The verifier is separated from workflow YAML so its decision logic is unit/static-testable without GitHub Actions. CI remains the enforcement point. The workflow must fetch complete history so ancestry is authoritative rather than dependent on a shallow clone.

## Test strategy

Static tests must prove:

- main pushes pass;
- pull-request events pass;
- side-branch pushes pass only when their HEAD is already contained in main;
- side-branch pushes fail when ahead of main;
- `AGENTS.md` contains the required main-first/final-qualification rules;
- `ci.yml` contains the continuity job, full-history checkout, and verifier invocation.

The first test commit must be RED because the verifier/contract/job do not yet exist. The implementation commit then makes the focused tests GREEN. Full repository CI is required on the final exact `main` commit.

## Non-goals

- No automatic merge/promotion bot.
- No new branch-protection dependency.
- No duplicate release authority system.
- No product runtime changes.
