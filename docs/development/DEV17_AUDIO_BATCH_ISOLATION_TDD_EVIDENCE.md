# Dev.17 Audio batch failure-isolation TDD evidence

Date: 2026-08-31  
Base authority: dev.16 exact main `dca3cbcba326a35801bc442ec93f16d84f58a692`.

## Risk closed
Before dev.17, `ConversionBatchRunner.RunAsync` iterated selected sources but allowed the first ordinary worker `ConversionFailedException` to escape immediately. In a native Explorer same-family multi-selection this meant one malformed/truncated file could suppress later valid selections even though each file already had independent strict worker/staging execution.

## RED 1 — production semantics
Commit `053e086fab6fcea1da83ab109e1a986379e0b82a`; run `33346968020`; managed job `99352825485`.

The new managed test supplies valid → failure → valid and requires three worker calls, first/third publication, collision safety and staging cleanup. Existing gates stayed green, while the new test failed exactly because the runner made two calls instead of three.

## RED 2 — product-path gate
Commit `285585107795045a41d85199c22fd971b1ed6191`; run `33346976504`.

Static contracts required a dedicated Windows mixed-batch smoke wired into ordinary CI, one Bridge process for a same-family selection, malformed+truncated entries, bounded wait, aggregate exit 4, transactional preservation and no partial residue. Those new assertions were intentionally RED before the component existed.

## Minimal implementation
- `dc48000696429df5f1d2c57e4a42310d8345c541` — catch only ordinary per-file `ConversionFailedException`, retain unconditional per-file staging cleanup, continue, then rethrow first media failure after iteration.
- `b50601f5836cc4d0a6962b3423693f40c1f02310` — add packaged five-file Audio mixed-batch acceptance.
- `6ee217e63de006bea31f4047d35a01e2de912721` — wire the gate into Windows CI.

## Harness defects caught test-first
The first integration never reached Bridge because a PowerShell log string used `$attempt:`. Commit `fe2886897dc03eec3942c046973e04558acaf860` added a parser-safety regression test; commit `355e5fdc47ad6d7090678a8b32461fb177a0db63` fixed it with `${attempt}:`.

The next integration still failed before Bridge because `-Arguments @(...) + ...` was bound as extra positional parameters. Commit `6fd23d346ddf5b2acecc34fef5974b559df31289` added a binding regression test; behavior head `5829c868c5d192c70f21ea0da9337250a8d9c961` prebuilds `$fixtureArguments` and passes it as one parameter value.

## GREEN behavior evidence
Run `33347652162`: managed job `99354775361`, static job `99354775208`.

Windows Server 2025 / `windows-2025-vs2026` / .NET SDK 10.0.400:
- locked restore: 18/18 PASS;
- NuGet vulnerability audit: 18 projects / 18 frameworks / 0 vulnerable-result packages;
- Release build: 0 warnings / 0 errors;
- native Explorer DLL/package/direct class-factory/loose-package COM activation+Invoke: PASS;
- existing four-target product smoke: PASS;
- dev.16 six-source × six-action Audio acceptance: 36/36 PASS;
- malformed and truncated single-file negative acceptance: PASS, deterministic exit 4;
- dev.17 five-file mixed batch: PASS twice, one Bridge process per attempt, aggregate exit 4;
- later valid items publish numbered MP3 outputs despite preceding failures;
- all sources/pre-existing destinations preserved; no invalid outputs; no partial residue;
- managed tests: 254/254 PASS, 0 skipped;
- static tests: 91/91 PASS before generated-authority freshness check;
- contract vectors: 5/5 PASS.

Pre-authority workspace built byte-identically twice: SHA-256 `4af24ae6f866c6389a3010642504aea13952ecb17d717c9974d05161fb8f6ba0`, 447903 bytes, 364 entries. The next assertion failed only because generated package/hash authority intentionally still described the previous tree.

## Finality still required
This behavior evidence does not itself freeze dev.17. Finality requires version-aligned generated authority from exact CI output, guarded synchronization, branch zero-diff qualification, non-force `main` fast-forward with unchanged base, then fresh exact-main continuity + managed + supply-chain-static SUCCESS and independently verified deterministic delivery.
