# Converty Cloud Documentation Map

Last reconciled: 2026-09-02.

This file is a **metadata-only continuation/routing map**. It does not replace GitHub as code/release authority and must never be substituted for the frozen `main` tree.

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
- Workspace bytes: `506224`
- Workspace ZIP entries: `384`
- Package-manifest entries: `382`
- SHA256SUMS entries: `383`
- Exact-main generated-authority artifact: `9862733877`
- Generated-authority digest: `sha256:dc7e8e8e9c220d9acad96a9a3c5bb0fc1e18555fb18cf02ec80e0ca0e399f627`
- Exact-main verified-delivery artifact: `9862843977`
- Verified-delivery digest: `sha256:1ab1a6b8127a5e1cf56dd44f1c9547a8d46293bdc3a2aa0aee911e3137386a11`
- Independent generated-authority/delivery verification: CRC, exact members, root/version, 382 package hashes, 383 SHA hashes and exclusion policy PASS; zero violations.

Historical dev.19 authority remains provenance only: `eb0ce66dab646427d5bef1548c12e5cc4765b2f1`, tree `337a4e11fb41bab6b6eeb462c3755381580f06c1`, run `33597504612`.

## Slack live documentation

- `#proj-converty` — `C0BUFGMGMFG` — anchor `1788366973.077219`
- `#roadmap-converty` — `C0BU2405ZMM` — anchor `1788366984.732379`
- `#plan-converty` — `C0BUKLHKL65` — anchor `1788368651.564749`
- `#tasks-converty` — `C0BTWQZQX4P` — anchor `1788327299.747159`
- `#changelog-converty` — `C0BUM4XRZ6G` — anchor `1788366995.127219`
- `#handover-open-converty` — `C0BUM8J0ZEG`
  - Handover #1 — TS `1788367585.736179` — `PROCESSED`
  - Handover #2 — TS `1788368822.626919` — `PROCESSED`
  - ACTIVE HANDOVER #3 — TS `1788376926.580049` — `OPEN`
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

## Current next tranche — ACTIVE HANDOVER #3

`0.1.0-dev.21 — B8 Video Copy/Remux/Transcode Planner`.

The precise next executable block is **Superpowers design / decision-model work before production code**:

1. Fresh-read exact frozen dev.20 `main` and current Video/probe/provider/domain/contracts/tests.
2. Compare bounded planner approaches and recommend one.
3. Define typed auditable execution mode `Copy | Remux | Transcode` and exact compatibility decision table from authoritative probe/capability facts.
4. Define deterministic conservative behavior for unknown/ambiguous/incompatible input.
5. Define compatibility pixel-format/audio defaults and explicit subtitle/HDR/metadata/non-primary-stream policy.
6. Keep Host/Bridge media/process neutral; provider translates typed plan only to fixed known engine tokens; no raw FFmpeg argument pass-through.
7. Preserve strict disposable worker isolation, fixed app-local engine, private staging, transactional numbered no-overwrite publication and all Audio/Image/dev.20 Video gates.
8. Hardware acceleration remains disabled/out of scope.
9. Produce RED acceptance matrix and implementation file map.
10. Obtain design approval before production behavior changes.
11. After approval only, create `dev/0.1.0-dev.21` from exact frozen dev.20 `main`, write the detailed implementation plan and execute RED -> GREEN TDD.

## Still open before customer launch

- dev.21 B8 planner/compatibility
- UX/settings
- Plugin SDK
- production FFmpeg/ffprobe provenance/signature/hash/license/notices/redistribution approval
- production signed-package B2 identity/authentication requalification
- signed production MSIX clean-Windows-11 lifecycle
- headed Windows 11 modern Explorer exact-build screenshots and crash/hang/failure matrix
- final fuzz/chaos/security/release/end-user acceptance

Converty is **not customer ship-ready**.

## Continuation / handover lifecycle

For `weiter`, `continue`, `start current documented handover` or equivalent:

1. Read the single current OPEN Slack handover — currently #3 TS `1788376926.580049`.
2. Fresh-read GitHub refs/CI and reconcile Drive/Slack against GitHub authority.
3. Execute the documented next task under Superpowers/TDD/security/review/evidence gates rather than re-planning approved work.
4. Verify the completed material block.
5. Update Authority + Roadmap + Plan + Tasks + Changelog + Evidence + Recursive Handover and canonical Slack anchors in place.
6. Mark the current OPEN handover PROCESSED **before** publishing its successor.
7. Publish exactly one context-free successor OPEN, then backfill exact successor TS/reference into the predecessor and all routing docs.
8. Re-read Slack + Recursive Handover and require exactly one OPEN.

Never hand-edit generated SBOM/package/hash authority. Never move frozen `main` merely for documentation synchronization.
