# Converty Cloud Documentation Map

Last reconciled: 2026-09-14.

This file is a **metadata-only continuation/routing map**. It does not replace GitHub as code/release authority and must never be substituted for the frozen `main` tree or the active engineering branch.

## Authority order

1. GitHub exact refs, code, CI runs/jobs and release artifacts are authoritative for code/release evidence.
2. Slack is the live operational mirror for project state, roadmap, current plan, tasks, changelog, evidence and the single OPEN handover.
3. Google Drive is the persistent cloud documentation/evidence library.
4. After every meaningful completed work block, reconcile all three layers; contradictory Slack/Drive narrative must be corrected to live GitHub evidence.

## Frozen dev.20 release authority

- Version: `0.1.0-dev.20`
- `main`: `8a1f46603aa842728247bc11b34fcccf121858fd`
- Tree: `4bd6f8d7acbadd60a3488870c773d2eafd67ba26`
- Exact-main run: `33671671714` — SUCCESS
- Continuity: `100386513722` SUCCESS
- Supply-chain/static: `100386513825` SUCCESS
- Windows managed: `100386513350` SUCCESS
- Managed tests: `260/260` PASS
- Static tests: `103/103` PASS
- Contract vectors: `5/5` PASS
- Video: `27/27` real packaged conversions + ffprobe + repeated malformed/truncated + twice-run mixed-batch PASS
- Audio 36-case and Image 24-case regressions: PASS
- Workspace SHA-256: `743c375cff7d854e0d63ea184f2423cc49ff9a7dca552442670c2b4322c5c805`
- Exact-main generated-authority artifact: `9862733877`
- Exact-main verified-delivery artifact: `9862843977`

This remains the sole release/freeze authority. Converty is **NOT CUSTOMER SHIP-READY**.

## Current dev.21 engineering authority

- Branch: `dev/0.1.0-dev.21`
- Head: `fdea3c43d8301275c39747d605f22ba9d99ac609`
- Tree: `a52b9acab1749652c260ef6325a032e88e100a1d`
- Approved design spec: `d80cc33a2e7c38738f113856e95f1451fd2df1b0`
- Implementation plan: `e54061368b476184a89dcadb1d6b8a8f6fb6cf68`
- Plan file: `docs/superpowers/plans/2026-09-03-dev21-video-copy-remux-transcode-security.md`

### Task 7 — mode-aware EngineWorker + managed Copy — VERIFIED

TDD lineage:

- Initial RED: `108ecf709a883735e26311a5a13c27c49459e2f5`, run `34879770520`.
  - Static: 108 PASS / exactly 3 Task-7 FAIL.
  - Windows Build: 0 warnings / 0 errors.
  - Existing packaged Audio/Image/Video regressions: PASS.
  - Managed: 358 total / 352 PASS / exactly 6 Task-7 FAIL.
- Bounded-mode test commit `40153d0ab1567f7ce2b54f1bfd4ea4bc4488cee6` exposed xUnit2018 in the test itself. The test-only assertion was corrected before GREEN.
- Corrected bounded-mode RED: `4dc49fa640bdb25d62a54df24081ce0dab7ad29b`, run `34880499859`.
  - Build: 0 warnings / 0 errors.
  - Existing packaged regressions: PASS.
  - Managed: 370 total / 352 PASS / exactly 18 expected Task-7 FAIL.
- GREEN: `fdea3c43d8301275c39747d605f22ba9d99ac609`, tree `a52b9acab1749652c260ef6325a032e88e100a1d`.
- GREEN run: `34880896997`.

Task-7 implementation:

- EngineWorker public surface is exactly `--preset --mode --input --output`.
- Canonical bounded mode grammar is `copy|remux|transcode|transform`.
- Managed Copy uses private staging, `FileMode.CreateNew`, streaming byte copy and SHA-256 input/output equality through `CryptographicOperations.FixedTimeEquals`.
- Copy dispatch occurs before provider compilation and FFmpeg path resolution; Copy never invokes FFmpeg.
- FFmpeg launcher receives an explicit mode and uses only the closed provider `(PresetId, ConversionMode)` compiler.
- Unsupported tuples reject before engine process resolution/start.
- No shell/raw-token/PATH/CWD/hardware-acceleration widening was introduced.
- Existing Audio/Image `Transform` compatibility is retained until Task 8 integrates planner-driven execution.

GREEN qualification:

- Dependency audit: 19 projects / 19 frameworks / 0 vulnerable-result packages.
- Build: 0 warnings / 0 errors.
- Managed: `370/370` PASS.
- Static: `111/111` PASS.
- Contract vectors: `5/5` PASS.
- Native Explorer, package validation, packaged ProbeWorker+ffprobe, COM and Product Bridge→FFmpeg: PASS.
- Audio 36-case, Image 24-case and Video 27-case matrices plus malformed/truncated/mixed-batch isolation: PASS.
- Deterministic workspace double-build: SHA-256 `846ff6f01301c54877441551847cfee464e72875f717e804e2e6a2bd2b3356fb`, 577675 bytes, 424 files.
- Workspace validation then failed only against intentionally stale tracked `.github/workflows/ci.yml` authority; delivery staging/upload skipped.
- Overall dev-branch CI remains intentionally RED only at main-authority-continuity, tracked generated-authority-current and derived workspace-authority validation.
- No generated-authority synchronization, dev.21 delivery, freeze or release authority exists.
- Task 7 self-review: PASS; external independent subagent review: NOT PERFORMED / NOT CLAIMED.

## Slack live documentation

- `#proj-converty` — `C0BUFGMGMFG` — anchor `1788366973.077219`
- `#roadmap-converty` — `C0BU2405ZMM` — anchor `1788366984.732379`
- `#plan-converty` — `C0BUKLHKL65` — anchor `1788368651.564749`
- `#tasks-converty` — `C0BTWQZQX4P` — anchor `1788327299.747159`
- `#changelog-converty` — `C0BUM4XRZ6G` — anchor `1788366995.127219`
- `#ci-converty` — `C0BUGDN98CD` — current Task-7 evidence anchor `1788516551.830259`
- `#engineering-converty` — `C0BUQCRE5K6` — current Task-7 engineering anchor `1788516560.218049`
- `#docs-converty` — `C0BUQCRL0TW` — current documentation anchor `1788516567.344989`
- `#handover-open-converty` — `C0BUM8J0ZEG`
  - Handover #1 — TS `1788367585.736179` — PROCESSED
  - Handover #2 — TS `1788368822.626919` — PROCESSED
  - Handover #3 — TS `1788376926.580049` — PROCESSED
  - Handover #4 — TS `1788420595.825169` — PROCESSED
  - Handover #5 — TS `1788476701.021959` — PROCESSED
  - Handover #6 — TS `1788516603.729439` — PROCESSED
  - Handover #7 — TS `1788519571.133199` — PROCESSED
  - **ACTIVE HANDOVER #8 — TS `1789410998.184939` — OPEN**
  - exactly one OPEN handover is allowed
- `#pre-devlog-converty` — `C0BV6HDMVDW`

## Google Drive live documents

- Authority / Index: `1ZdDGUpSVxeEfvICLKD_VctT49MlJMyhNICyj4ebeYRw`
- Roadmap: `1p3xKxj2akSqZTzVp442QNetoZ8Eg9u6pvckjwUnBLsI`
- Current Implementation Plan: `1eGVajQAxw3Vjc7F_7NJgt9do6tRzZpV_Vbfcl24g9-s`
- Open Tasks & Gates: `1BH44EUYcNBexIZasxq24mlYBZk0VxF5XaG6RnUkQPrc`
- Changelog: `1JsJfEECcWaB2UJtW0oiW45RD86RZT4i5spANV38Zzoc`
- Release & Test Evidence: `1LizDehSMDnBfihXnntX9z13QNai87zzMwlkzPptkcB0`
- Recursive Handover: `1HVfL2KV6LZbpl0fc4Je1dzqLs3ya9q9Onjb3YbFF9L8`

Update these documents in place; never create competing current-state copies.

## Current next tranche — ACTIVE HANDOVER #8

`0.1.0-dev.21 — B8 Video Copy/Remux/Transcode Planner` remains the active engineering tranche. Tasks 1–7 are verified.

The precise next executable block is **Task 8 RED/GREEN — stage → probe → plan → execute → post-probe TargetMediaContract → transactional publish**:

1. RED: staged Video input is probed through the existing typed bounded probe boundary.
2. RED: existing `VideoPlanningPolicy` / `ConversionPlanner` selects Copy, Remux or Transcode.
3. RED: the selected explicit `ConversionMode` reaches EngineWorker.
4. RED: staged output is post-probed and validated against a typed immutable `TargetMediaContract` before publication.
5. RED: engine exit 0 with wrong container/codec/topology/pixel-format/audio/HDR policy does not publish.
6. RED: post-probe failure, timeout or corrupt output does not publish.
7. RED: Copy hash mismatch does not publish.
8. RED: malformed/unsupported member-local failures do not block later valid batch members.
9. GREEN: integrate the existing planner/probe/execution path with additive compatibility where practical; preserve Audio/Image current behavior and source/existing-destination/no-partial/no-overwrite invariants.
10. Run full affected Core/Bridge tests and preserve all Tasks 1–7/dev.20 regression evidence.
11. Do not synchronize generated authority or claim dev.21 delivery/freeze/release during Task 8.

After Task 8: real child containment/network/filesystem canaries → runtime engine digest/package binding → real packaged Copy/Remux/Transcode qualification → governance/supply-chain hardening → guarded generated-authority stabilization → remaining production signing/headed/security/end-user gates.

## Architecture / security invariants

`IExplorerCommand DLL -> fixed app-local Bridge -> private staging -> strict disposable read-only ProbeWorker/fixed ffprobe -> typed bounded facts -> Core VideoPlanningPolicy/existing ConversionMode -> strict EngineWorker -> managed Copy OR provider-owned fixed Remux/Transcode tokens -> fixed app-local FFmpeg -> private staged output -> strict post-probe TargetMediaContract -> transactional numbered no-overwrite publication.`

Never widen to shell command construction, raw FFmpeg argument pass-through, PATH/CWD binary lookup, arbitrary converter/plugin discovery, ordinary conversion network dependency, silent Strict-to-Compatibility fallback, hardware acceleration or repository signing private keys.

## Continuation / handover lifecycle

For `weiter`, `continue`, `start current documented handover` or equivalent:

1. Read ACTIVE HANDOVER #8 TS `1789410998.184939`.
2. Fresh-read GitHub refs/CI and reconcile Drive/Slack against GitHub authority.
3. Execute Task 8 under Superpowers/TDD/security/review/evidence gates rather than re-planning approved work.
4. Verify the completed material block.
5. Update Authority + Roadmap + Plan + Tasks + Changelog + Evidence + Recursive Handover and canonical Slack anchors in place.
6. Fresh-read GitHub/CI again.
7. Mark #8 PROCESSED **before** publishing its successor.
8. Publish exactly one context-free successor OPEN, backfill its exact TS/reference into #8, Recursive Handover and this metadata routing map.
9. Re-read Slack + Recursive Handover and require exactly one OPEN.

Never hand-edit generated SBOM/package/hash authority. Never move frozen `main` merely for documentation synchronization.
