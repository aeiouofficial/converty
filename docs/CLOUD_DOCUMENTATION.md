# Converty Cloud Documentation Map

Last reconciled: 2026-09-04.

This file is a **metadata-only continuation/routing map**. It does not replace GitHub as code/release authority and must never be substituted for the frozen `main` tree or the active engineering branch.

## Authority order

1. GitHub exact refs, code, CI runs/jobs and release artifacts are authoritative for code/release evidence.
2. Slack is the live operational mirror for project state, roadmap, current plan, tasks, changelog and the single OPEN handover.
3. Google Drive is the persistent cloud documentation/evidence library.
4. After every meaningful completed work block, reconcile all three layers; contradictory Slack/Drive narrative must be corrected to live GitHub evidence.

## Frozen dev.20 release authority

- Version: `0.1.0-dev.20`
- `main`: `8a1f46603aa842728247bc11b34fcccf121858fd`
- Tree: `4bd6f8d7acbadd60a3488870c773d2eafd67ba26`
- Exact-main run: `33671671714`
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

Historical dev.19 authority remains provenance only: `eb0ce66dab646427d5bef1548c12e5cc4765b2f1`, tree `337a4e11fb41bab6b6eeb462c3755381580f06c1`, run `33597504612`.

## Current dev.21 engineering authority

- Branch: `dev/0.1.0-dev.21`
- Head: `8b84276af7f2353fc11b64fc3f4d649bcd06e522`
- Tree: `d60103bc2a30d4678796bb9e6276cfe0946b2b52`
- Approved design spec commit: `d80cc33a2e7c38738f113856e95f1451fd2df1b0`
- Implementation plan commit: `e54061368b476184a89dcadb1d6b8a8f6fb6cf68`
- Final Task-4 development run: `33815328422`
- Task-4 verified outcomes: build 0 warnings/0 errors; `292/292` managed; `106/106` static; `5/5` vectors; packaged ProbeWorker/fixed ffprobe acceptance PASS; Audio 36, Image 24 and Video 27 plus repeated negative/mixed regressions PASS.
- Overall dev-branch CI remains intentionally RED only at development-branch main continuity and stale tracked generated-authority/workspace verification gates.
- No dev.21 generated authority was promoted; no verified dev.21 delivery/freeze/release authority exists.

## Slack live documentation

- `#proj-converty` — `C0BUFGMGMFG` — anchor `1788366973.077219`
- `#roadmap-converty` — `C0BU2405ZMM` — anchor `1788366984.732379`
- `#plan-converty` — `C0BUKLHKL65` — anchor `1788368651.564749`
- `#tasks-converty` — `C0BTWQZQX4P` — anchor `1788327299.747159`
- `#changelog-converty` — `C0BUM4XRZ6G` — anchor `1788366995.127219`
- `#handover-open-converty` — `C0BUM8J0ZEG`
  - Handover #1 — TS `1788367585.736179` — `PROCESSED`
  - Handover #2 — TS `1788368822.626919` — `PROCESSED`
  - Handover #3 — TS `1788376926.580049` — `PROCESSED`
  - Handover #4 — TS `1788420595.825169` — `PROCESSED`
  - ACTIVE HANDOVER #5 — TS `1788476701.021959` — `OPEN`
  - exactly one OPEN handover is allowed
- `#pre-devlog-converty` — `C0BV6HDMVDW`

Slack Canvas is unavailable in this free workspace, so canonical anchor messages are edited in place and durable full documents live in Google Drive.

## Google Drive library

Project Documentation Library: `1h0GoSM8MfRy8GUjQa6hMLIQbhzEfkoT_`

Converty root: `1zSKLK-yKmX15xIWSj1D_39tmArymhHmn`

Subfolders:

- `00 Authority`: `1Wz5Bz14GlX89UK7NN6kRUE4uUjmR5wnr`
- `01 Roadmap`: `1EszeYUEChqWOXeyQa1CCXSGH1qpgthuX`
- `02 Tasks`: `1yWwF_PyXwwXWMsN9zKvc_dqvPTU12-rI`
- `03 Changelog`: `19zRdZz3yQ3MW_q1oYkMCmZZJNilHp9wg`
- `04 Handover`: `1FOS_EQfv65Hp4fZAiXDkw2FzxUDEBz4H`
- `05 Devlogs & Release Evidence`: `1wZOfy2kzffGyXMtMq05MgSB4Xli2p-Yl`

Fixed live Docs — update in place, never create competing current-state copies:

- Authority / Index: `1ZdDGUpSVxeEfvICLKD_VctT49MlJMyhNICyj4ebeYRw`
- Roadmap: `1p3xKxj2akSqZTzVp442QNetoZ8Eg9u6pvckjwUnBLsI`
- Current Implementation Plan: `1eGVajQAxw3Vjc7F_7NJgt9do6tRzZpV_Vbfcl24g9-s`
- Open Tasks & Gates: `1BH44EUYcNBexIZasxq24mlYBZk0VxF5XaG6RnUkQPrc`
- Changelog: `1JsJfEECcWaB2UJtW0oiW45RD86RZT4i5spANV38Zzoc`
- Recursive Handover: `1HVfL2KV6LZbpl0fc4Je1dzqLs3ya9q9Onjb3YbFF9L8`
- Release & Test Evidence: `1LizDehSMDnBfihXnntX9z13QNai87zzMwlkzPptkcB0`

## Current next tranche — ACTIVE HANDOVER #5

`0.1.0-dev.21 — B8 Video Copy/Remux/Transcode Planner` remains the active engineering tranche.

Tasks 1–4 are verified. The precise next executable block is **Task 5 RED/GREEN — provider-only FFmpeg token ownership**:

1. Start with RED unit/static tests.
2. Remove Core-owned FFmpeg syntax/token policy from `ProductPresetDefinition` / `ProductPresetRegistry`; Core retains product/menu/source-extension/output semantics only.
3. Add closed `providers/Converty.Provider.FFmpeg/FfmpegPresetCompiler.cs`.
4. Compiler accepts only exact supported `(PresetId, ConversionMode)` tuples and returns immutable known token vectors.
5. Unsupported tuples reject before process start; no caller-provided arbitrary engine-token surface exists.
6. Provider owns explicit stream mapping, file-only protocol posture, metadata/chapter stripping and fixed codec/pixel/audio profiles.
7. Hardware acceleration remains disabled.
8. Preserve complete existing Audio/Image/Video advertised behavior and security regressions.
9. Do not synchronize generated authority or claim dev.21 release/freeze during Task 5.

After Task 5: deterministic `VideoPlanningPolicy` → mode-aware EngineWorker + managed byte-exact Copy/SHA equality → stage/probe/plan/execute/post-probe `TargetMediaContract`/publish → real child containment/network/filesystem canaries → runtime engine digest binding → guarded generated-authority stabilization → remaining production release/headed/signing gates.

## Architecture / security invariants

`IExplorerCommand DLL -> fixed app-local Bridge -> private staging -> strict disposable read-only ProbeWorker/fixed ffprobe -> typed bounded facts -> Core VideoPlanningPolicy/existing ConversionMode -> strict EngineWorker -> managed Copy OR provider-owned fixed Remux/Transcode tokens -> fixed app-local FFmpeg -> private staged output -> strict post-probe TargetMediaContract -> transactional numbered no-overwrite publication.`

Never widen to shell command construction, raw FFmpeg argument pass-through, PATH/CWD binary lookup, arbitrary converter/plugin discovery, ordinary conversion network dependency, silent Strict-to-Compatibility fallback, hardware acceleration or repository signing private keys.

## Still open before customer launch

- remaining dev.21 B8 planner/execution/post-validation/security hardening
- UX/settings
- Plugin SDK under a separate trust architecture
- production FFmpeg/ffprobe provenance/signature/hash/license/notices/redistribution approval
- production signed-package B2 identity/authentication requalification
- signed production MSIX clean-Windows-11 lifecycle
- headed Windows 11 modern Explorer exact-build screenshots and crash/hang/failure matrix
- final fuzz/chaos/security/release/end-user acceptance

Converty is **not customer ship-ready**.

## Continuation / handover lifecycle

For `weiter`, `continue`, `start current documented handover` or equivalent:

1. Read the single current OPEN Slack handover — currently #5 TS `1788476701.021959`.
2. Fresh-read GitHub refs/CI and reconcile Drive/Slack against GitHub authority.
3. Execute the documented next task under Superpowers/TDD/security/review/evidence gates rather than re-planning approved work.
4. Verify the completed material block.
5. Update Authority + Roadmap + Plan + Tasks + Changelog + Evidence + Recursive Handover and canonical Slack anchors in place.
6. Mark the current OPEN handover PROCESSED **before** publishing its successor.
7. Publish exactly one context-free successor OPEN, then backfill exact successor TS/reference into the predecessor and all routing docs.
8. Re-read Slack + Recursive Handover and require exactly one OPEN.

Never hand-edit generated SBOM/package/hash authority. Never move frozen `main` merely for documentation synchronization.
