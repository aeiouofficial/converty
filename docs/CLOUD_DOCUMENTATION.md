# Converty Cloud Documentation Map

Last reconciled: 2026-09-18.

This file is a **metadata-only continuation/routing map**. GitHub exact refs, code, CI and release evidence remain authoritative. Slack is the live operational mirror and Google Drive is the persistent documentation/evidence library.

## Frozen release authority

- Version: `0.1.0-dev.20`
- `main`: `8a1f46603aa842728247bc11b34fcccf121858fd`
- Tree: `4bd6f8d7acbadd60a3488870c773d2eafd67ba26`
- Exact-main CI: `33671671714` — SUCCESS
- This remains the sole release/freeze authority.
- Converty is **NOT CUSTOMER SHIP-READY**.

A recovered documentation-routing incident is preserved at `recovery/accidental-main-routing-20260917@dc09f895897ad682a8aade39f3daaaab9aa74f78`. Fresh `main` is restored exactly to the frozen SHA/tree above. Incident Slack TS: `1789698550.885969`.

## Current dev.21 engineering authority

- Branch: `dev/0.1.0-dev.21`
- Head: `277f6c30f5fd22b3107604304717e56839e76641`
- Tree: `b8c1f9ea203e532e235a94174740bb23821c7d86`
- Approved design: `d80cc33a2e7c38738f113856e95f1451fd2df1b0`
- Implementation plan: `e54061368b476184a89dcadb1d6b8a8f6fb6cf68`
- Plan path: `docs/superpowers/plans/2026-09-03-dev21-video-copy-remux-transcode-security.md`

### Task 11 — governance / supply-chain hardening — VERIFIED

- Canonical RED: `d454f1940e09363a7ec4c6a5109bb8b0b6894d63`
- RED CI: `35299101763` — exactly 3 new governance failures, 129 existing static PASS
- GREEN source: `2a82cd7484589d306047086e906712060c12d0ee`
- Final YAML-safe head: `277f6c30f5fd22b3107604304717e56839e76641`
- Final CI: `35299376030`
- Supply-chain/static job: `105458586278`
- Managed job: `105458586125`
- Ubuntu + Windows Python install via committed SHA-256 lock and `--require-hashes --only-binary=:all:`: PASS
- Immutable Action pins: PASS — 8 external uses / 4 approved Actions
- Workflow permission: `contents: read`
- Dependency audit: 19 projects / 19 frameworks / 0 vulnerable-result packages
- Build: 0 warnings / 0 errors
- Managed: `392/392` PASS
- Static: `132/132` PASS
- Contract vectors: `5/5` PASS
- Native/package/ProbeWorker/COM/Product, Audio36, Image24, Video27, negatives/mixed, Task9 containment and Task10 Copy/Remux/Transcode: PASS
- Deterministic workspace candidate SHA-256: `e2b1be0a4f5c8ab1f4cb0b936e3289795cabc81f0144d974c8e5a036b6bf1986`
- Candidate workspace: 613591 bytes / 442 files
- Generated-authority artifact: `10529710374`
- Generated-authority digest: `sha256:0029b1def03e329572ef221edc8779c89158a695b0be26ce02a05487e01e99ec`
- Expected development failures only: tracked generated authority stale; workspace authority assertion stale; main-continuity red on dev branch
- Delivery skipped; no dev.21 freeze/release claim

## Live repository governance

Fresh GitHub evidence at Task11 close:

- repository rulesets: `[]`
- `main` branch protection: disabled
- desired future freeze semantics are documented in `docs/supply-chain/REPOSITORY_GOVERNANCE_POLICY.md`
- source policy does **not** claim those controls are live
- Task12 must not weaken exact-candidate semantics or rewrite historical unsigned dev.20

## Operational continuation

Slack `#handover-open-converty` (`C0BUM8J0ZEG`):

- Handover #10 — TS `1789568171.911999` — PROCESSED
- Handover #11 — TS `1789607844.489119` — PROCESSED
- **ACTIVE HANDOVER #12 — TS `1789698886.030639` — OPEN**
- Exactly one OPEN handover is allowed.

Canonical Slack anchors:

- project `C0BUFGMGMFG` / `1788366973.077219`
- roadmap `C0BU2405ZMM` / `1788366984.732379`
- plan `C0BUKLHKL65` / `1788368651.564749`
- tasks `C0BTWQZQX4P` / `1788327299.747159`
- changelog `C0BUM4XRZ6G` / `1788366995.127219`
- CI `C0BUGDN98CD` / `1788516551.830259`
- engineering `C0BUQCRE5K6` / `1788516560.218049`
- docs `C0BUQCRL0TW` / `1788516567.344989`
- incidents `C0BUEFME0D9`

Canonical Drive documents:

- Authority `1ZdDGUpSVxeEfvICLKD_VctT49MlJMyhNICyj4ebeYRw`
- Roadmap `1p3xKxj2akSqZTzVp442QNetoZ8Eg9u6pvckjwUnBLsI`
- Plan `1eGVajQAxw3Vjc7F_7NJgt9do6tRzZpV_Vbfcl24g9-s`
- Tasks `1BH44EUYcNBexIZasxq24mlYBZk0VxF5XaG6RnUkQPrc`
- Changelog `1JsJfEECcWaB2UJtW0oiW45RD86RZT4i5spANV38Zzoc`
- Evidence `1LizDehSMDnBfihXnntX9z13QNai87zzMwlkzPptkcB0`
- Recursive Handover `1HVfL2KV6LZbpl0fc4Je1dzqLs3ya9q9Onjb3YbFF9L8`

## Current next task — ACTIVE HANDOVER #12

**DEV.21 Task 12 — guarded generated-authority stabilization, exact-candidate qualification and conditional freeze lifecycle.**

1. Inspect ordinary CI-generated deterministic authority diffs.
2. Independently verify generated package/hash/SBOM authority.
3. Synchronize generated authority only through the existing guarded exact-parent/self-deleting workflow. Never hand-edit.
4. Require zero-diff deterministic qualification on the exact candidate SHA.
5. Independently verify final workspace/delivery artifacts.
6. Only if every required governance/release gate is genuinely GREEN and exact-candidate compatible, promote/freeze by non-force exact-SHA movement. Never create a merge/squash/post-qualification SHA merely to satisfy governance.
7. Fresh-read exact `main` and CI after any promotion. No freeze claim until exact-main continuity/static/managed checks complete SUCCESS.
8. If live governance/signature/provenance/headed-release prerequisites cannot genuinely be satisfied, stop before promotion and document the exact blocker; never weaken gates.

## Security invariant

`IExplorerCommand DLL -> fixed Bridge -> private staging -> strict RO ProbeWorker/fixed ffprobe -> typed bounded facts -> Core planner -> strict EngineWorker -> managed Copy OR provider-owned fixed Remux/Transcode -> fixed app-local FFmpeg -> private staged output -> TargetMediaContract -> transactional numbered no-overwrite publication.`

Never widen to shell/raw FFmpeg arguments, PATH/CWD binary lookup, arbitrary executable/plugin discovery, ordinary conversion network dependency, silent Strict-to-Compatibility fallback, hardware acceleration or repository signing private keys.

## Lifecycle rule

On `weiter`/`continue`: fresh-read ACTIVE HANDOVER #12, GitHub/CI and canonical Drive; GitHub wins; execute Task12 fully; verify; update canonical cloud docs in place; fresh-read GitHub; mark #12 PROCESSED first; publish exactly one successor OPEN; backfill its exact reference into #12, Drive and this routing map; verify exactly one OPEN.

Never hand-edit generated SBOM/package/hash authority and never move frozen `main` merely for documentation synchronization.
