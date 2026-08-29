# Converty 0.1.0-dev.12 Output-Budget Determinism Plan

**Goal:** Eliminate the intermittent `StrictWorkerIsTerminatedWhenStagingGrowthExceedsOutputBudget` failure without weakening the production private-staging output budget, strict AppContainer isolation, Job Object containment, or kill-on-close behavior.

**Authority:** Start from live `main` `ac475b5a51e19c7618a424ca689657cdf75edcaa`. Durable work is committed directly to GitHub `main` under `AGENTS.md` main-first authority.

## Root-cause evidence

Historical failing qualification reached the expected `WorkerOutputLimitExceededException` but observed a final canary file of 610,304 bytes, above the test-only 512 KiB upper assertion. The production security contract requires a finite output-growth cap, worker-tree termination, and no final publication after breach; it does not define a 512 KiB maximum overshoot.

The launcher samples private staging growth every 25 ms. The existing canary writes 4 KiB, flushes, then delays 5 ms. Under runner scheduling pressure, the launcher/test process may not be scheduled for several nominal poll periods while the separately scheduled worker continues writing. Therefore final on-disk overshoot is scheduler-dependent even though detection and termination are correct.

The existing canary also has a natural successful end at 1 MiB, so the test mixes two concerns: proving the monitor detects a live breach and asserting an arbitrary amount of post-breach overshoot.

## Hypothesis

The intermittent is caused by a brittle test-harness timing assertion, not by evidence that the production output-growth enforcement failed. A deterministic canary that writes until forcibly terminated can prove the real invariant without relying on a scheduler-dependent maximum final file size.

## TDD sequence

### 1. RED — require an unbounded slow-write canary

Modify `WorkerOutputLimitTests.StrictWorkerIsTerminatedWhenStagingGrowthExceedsOutputBudget` to invoke a new `--write-slow-unbounded <path>` canary mode that does not yet exist.

The test must:

- require `WorkerOutputLimitExceededException`;
- assert `MaximumOutputBytes` equals the configured 64 KiB budget;
- assert `ObservedOutputGrowthBytes > MaximumOutputBytes`;
- assert the staged file exists and grew beyond the configured budget;
- remove the arbitrary `<= 512 KiB` final-size assertion.

Expected RED: current canary rejects the new mode, so the launcher returns normally instead of throwing the output-limit exception.

### 2. GREEN — implement only the deterministic canary mode

Add `--write-slow-unbounded <path>` to `tests/Converty.WorkerCanary/Program.cs`.

It must:

- create a new staging file;
- repeatedly write 4096 bytes;
- flush each write;
- delay 5 ms between writes;
- have no natural success completion; termination is supplied by the production Job Object path after the launcher observes a budget breach.

Do not change `WindowsWorkerProcessLauncher.OutputPollInterval` or production `WorkerResourceLimits` merely to satisfy the test.

### 3. Verify the narrow fix

Require on Windows:

- the target output-budget test passes repeatedly;
- the full managed suite passes;
- strict filesystem/network/descendant containment remains green;
- production Bridge→Strict Worker→FFmpeg smoke remains green.

### 4. Close dev.12 authority

Synchronize `0.1.0-dev.12` version/docs/machine-readable authority only from executed evidence. Regenerate deterministic SBOM/package/hash authority, require zero diff, build the workspace twice byte-identically, and run final ordinary CI on the exact live `main` SHA.

## Non-negotiable invariants

- Do not increase or relax the configured production output budget to make the test pass.
- Do not weaken strict isolation, AppContainer policy, Job Object limits, kill-on-close, or staging-only filesystem access.
- Do not introduce a shell, PATH converter lookup, raw FFmpeg passthrough, or alternate converter path.
- Preserve Explorer → Bridge → strict disposable EngineWorker/provider → FFmpeg → private staging → transactional numbered publication.
- Preserve source and existing destination files on failure.
- Preserve failed RED evidence in GitHub history.
- A tranche is not complete until a fresh live fetch proves the qualified commit is current `main` and ordinary CI succeeded on that exact SHA.
