# Converty Cloud Documentation Map

Last reconciled: 2026-09-04.

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
- Head: `d377623e28025d3291605e9a8f8a97cd64c825cd`
- Tree: `bdfe8b681923ef528d332e017ae8db44559cec81`
- Approved design spec: `d80cc33a2e7c38738f113856e95f1451fd2df1b0`
- Implementation plan: `e54061368b476184a89dcadb1d6b8a8f6fb6cf68`
- Plan file: `docs/superpowers/plans/2026-09-03-dev21-video-copy-remux-transcode-security.md`

### Task 6 — deterministic VideoPlanningPolicy — VERIFIED

- RED: `a01a5fa001d527dce4265059f5fc6fad47d5a90e`
- RED run: `33864838213`; Windows job `100997215966`; expected compile failure on missing Task-6 policy/reason types.
- GREEN: `d377623e28025d3291605e9a8f8a97cd64c825cd`
- GREEN run: `33865077024`
- GREEN Build: 0 warnings / 0 errors
- Managed: `352/352` PASS
- Static: `109/109` PASS
- Contract vectors: `5/5` PASS
- Dependency audit: 19 projects / 19 frameworks / 0 vulnerable-result packages
- Package/ProbeWorker/native Explorer/COM/Product Bridge: PASS
- Audio 36-case, Image 24-case, Video 27-case plus repeated negatives/mixed isolation: PASS
- Deterministic workspace double-build: SHA-256 `65234fb92c343888e526c97a3302fadd509bf1cfd015eec3285318c8abf5e533`, 569784 bytes, 417 files
- Workspace authority verification then failed only on intentionally stale tracked `.github/workflows/ci.yml` authority; delivery staging/upload skipped.
- Overall dev-branch CI remains intentionally RED only at main-authority-continuity, tracked generated-authority-current and derived workspace-authority validation.
- No generated-authority synchronization, dev.21 delivery, freeze or release authority exists.
- Task 6 self-review: PASS; external independent subagent review: NOT PERFORMED / NOT CLAIMED.

Task-6 architecture reuses existing `ConversionMode`, `ConversionPlan`, `CapabilityGraph` and `ConversionPlanner`. Typed bounded probe facts feed a deterministic bounded `VideoPlanningPolicy`; unknown/unsupported/incomplete/unqualified/ambiguous/HDR/high-bit-depth facts fail closed. No second planner hierarchy, raw FFmpeg token ingress or stderr-driven fallback exists.

## Slack live documentation

- `#proj-converty` — `C0BUFGMGMFG` — anchor `1788366973.077219`
- `#roadmap-converty` — `C0BU2405ZMM` — anchor `1788366984.732379`
- `#plan-converty` — `C0BUKLHKL65` — anchor `1788368651.564749`
- `#tasks-converty` — `C0BTWQZQX4P` — anchor `1788327299.747159`
- `#changelog-converty` — `C0BUM4XRZ6G` — anchor `1788366995.127219`
- `#ci-converty` — `C0BUGDN98CD`
- `#engineering-converty` — `C0BUQCRE5K6`
- `#docs-converty` — `C0BUQCRL0TW`
- `#handover-open-converty` — `C0BUM8J0ZEG`
  - Handover #1 — TS `1788367585.736179` — PROCESSED
  - Handover #2 — TS `1788368822.626919` — PROCESSED
  - Handover #3 — TS `1788376926.580049` — PROCESSED
  - Handover #4 — TS `1788420595.825169` — PROCESSED
  - Handover #5 — TS `1788476701.021959` — PROCESSED
  - Handover #6 — TS `1788516603.729439` — PROCESSED
  - **ACTIVE HANDOVER #7 — TS `1788519571.133199` — OPEN**
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

## Current next tranche — ACTIVE HANDOVER #7

`0.1.0-dev.21 — B8 Video Copy/Remux/Transcode Planner` remains the active engineering tranche. Tasks 1–6 are verified.

The precise next executable block is **Task 7 RED/GREEN — mode-aware EngineWorker + managed byte-exact Copy**:

1. RED: EngineWorker surface is exactly `--preset --mode --input --output`.
2. RED: Copy launches no FFmpeg and performs managed byte copy.
3. RED: staged input/output SHA-256 equality is mandatory.
4. RED: Remux/Transcode invoke only provider-compiled fixed tokens.
5. RED: unsupported `(PresetId, ConversionMode)` / mode combinations reject before engine start.
6. GREEN: add bounded `ConversionMode` input; managed Copy + SHA verification; provider execution only for Remux/Transcode.
7. Run affected Bridge/Core worker/provider tests and preserve all Task1–6/dev.20 regressions.
8. Do not synchronize generated authority or claim dev.21 delivery/freeze/release during Task 7.

After Task 7: stage/probe/plan/execute/post-probe `TargetMediaContract`/publish -> real child containment/network/filesystem canaries -> runtime engine digest binding -> guarded generated-authority stabilization -> remaining production governance/signing/headed/security/end-user gates.

## Architecture / security invariants

`IExplorerCommand DLL -> fixed app-local Bridge -> private staging -> strict disposable read-only ProbeWorker/fixed ffprobe -> typed bounded facts -> Core VideoPlanningPolicy/existing ConversionMode -> strict EngineWorker -> managed Copy OR provider-owned fixed Remux/Transcode tokens -> fixed app-local FFmpeg -> private staged output -> strict post-probe TargetMediaContract -> transactional numbered no-overwrite publication.`

Never widen to shell command construction, raw FFmpeg argument pass-through, PATH/CWD binary lookup, arbitrary converter/plugin discovery, ordinary conversion network dependency, silent Strict-to-Compatibility fallback, hardware acceleration or repository signing private keys.

## Continuation / handover lifecycle

For `weiter`, `continue`, `start current documented handover` or equivalent:

1. Read ACTIVE HANDOVER #7 TS `1788519571.133199`.
2. Fresh-read GitHub refs/CI and reconcile Drive/Slack against GitHub authority.
3. Execute Task 7 under Superpowers/TDD/security/review/evidence gates rather than re-planning approved work.
4. Verify the completed material block.
5. Update Authority + Roadmap + Plan + Tasks + Changelog + Evidence + Recursive Handover and canonical Slack anchors in place.
6. Mark #7 PROCESSED **before** publishing its successor.
7. Publish exactly one context-free successor OPEN, backfill exact successor TS/reference into #7 and all routing docs.
8. Re-read Slack + Recursive Handover and require exactly one OPEN.

Never hand-edit generated SBOM/package/hash authority. Never move frozen `main` merely for documentation synchronization.
