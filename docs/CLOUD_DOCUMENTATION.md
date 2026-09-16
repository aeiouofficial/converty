# Converty Cloud Documentation Map

Last reconciled: 2026-09-16.

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
- Managed tests: `260/260` PASS
- Static tests: `103/103` PASS
- Contract vectors: `5/5` PASS
- Video: `27/27` real packaged conversions plus negative/mixed qualification PASS
- Audio 36-case and Image 24-case regressions: PASS
- Workspace SHA-256: `743c375cff7d854e0d63ea184f2423cc49ff9a7dca552442670c2b4322c5c805`
- Exact-main generated-authority artifact: `9862733877`
- Exact-main verified-delivery artifact: `9862843977`

This remains the sole release/freeze authority. Converty is **NOT CUSTOMER SHIP-READY**.

## Current dev.21 engineering authority

- Branch: `dev/0.1.0-dev.21`
- Head: `c229fdeca638184b05b5359e735922cc7c9da3c4`
- Tree: `0ebbe25928f06890922760c839361e602ae872b8`
- Approved design spec: `d80cc33a2e7c38738f113856e95f1451fd2df1b0`
- Implementation plan: `e54061368b476184a89dcadb1d6b8a8f6fb6cf68`
- Plan file: `docs/superpowers/plans/2026-09-03-dev21-video-copy-remux-transcode-security.md`

### Task 8 — stage/probe/plan/execute/post-validate/publish — VERIFIED

Task 8 integrates staged Video probing, existing `VideoPlanningPolicy`/`ConversionPlanner` mode selection, explicit-mode execution, independent Copy SHA verification, staged-output post-probing, immutable `TargetMediaContract` validation and transactional numbered no-overwrite publication. Invalid or unqualified outputs never publish; member-local failures preserve later valid batch continuation.

Qualification hardening also established:

- provider-owned MP4/WebM Transcode enforces the qualified BT.709 SDR target semantics;
- known technical muxer tags are narrowly classified as non-policy metadata while unknown/untrusted tag names or values remain policy-relevant and fail closed;
- managed MP4 Copy remains closed to the canonical `.mp4|.m4v` alias set; WebM Copy remains `.webm`;
- final fixture-only commit `c229fdeca638184b05b5359e735922cc7c9da3c4` aligns Video runner tests with production `provider.engine` authority and changes no production file.

Final Task-8 evidence:

- CI run: `35094861575`
- Windows managed job: `104789579843`
- Dependency audit: 19 projects / 19 frameworks / 0 vulnerable-result packages
- Build: 0 warnings / 0 errors
- Managed: `387/387` PASS
- Static: `119/119` PASS
- Contract vectors: `5/5` PASS
- Native Explorer, package validation, packaged ProbeWorker+ffprobe, COM and Product Bridge→FFmpeg: PASS
- Audio 36-case, Image 24-case and Video 27-case plus malformed/truncated/mixed isolation: PASS
- Deterministic workspace double-build: SHA-256 `6d9eeeec8832ae50eeb00934b58553cf21371c83fbc1b13fea026b19fc23794c`, 592645 bytes, 435 files
- Workspace validation then failed only against intentionally stale tracked `.github/workflows/ci.yml` authority; delivery staging/upload skipped
- Overall dev-branch CI remains intentionally RED only at main-authority-continuity and stale generated/workspace authority
- No generated-authority synchronization, dev.21 delivery, freeze or release authority exists
- External independent review: NOT PERFORMED / NOT CLAIMED

Development engine qualification input:

- FFmpeg/ffprobe: `9.0.1-essentials_build-www.gyan.dev`
- Archive SHA-256: `fec81ae03971d9dd4be3ebe02e263bd2ec1d789483f931bdba5f5715e65da2e9`
- Development qualification input only; not production provenance, licensing/notices, redistribution or release approval

## Slack live documentation

- `#proj-converty` — `C0BUFGMGMFG` — anchor `1788366973.077219`
- `#roadmap-converty` — `C0BU2405ZMM` — anchor `1788366984.732379`
- `#plan-converty` — `C0BUKLHKL65` — anchor `1788368651.564749`
- `#tasks-converty` — `C0BTWQZQX4P` — anchor `1788327299.747159`
- `#changelog-converty` — `C0BUM4XRZ6G` — anchor `1788366995.127219`
- `#ci-converty` — `C0BUGDN98CD` — anchor `1788516551.830259`
- `#engineering-converty` — `C0BUQCRE5K6` — anchor `1788516560.218049`
- `#docs-converty` — `C0BUQCRL0TW` — anchor `1788516567.344989`
- `#handover-open-converty` — `C0BUM8J0ZEG`
  - Handover #1 through #8: PROCESSED
  - Handover #8 — TS `1789410998.184939` — PROCESSED
  - **ACTIVE HANDOVER #9 — TS `1789561580.123109` — OPEN**
  - exactly one OPEN handover is allowed

## Google Drive live documents

- Authority / Index: `1ZdDGUpSVxeEfvICLKD_VctT49MlJMyhNICyj4ebeYRw`
- Roadmap: `1p3xKxj2akSqZTzVp442QNetoZ8Eg9u6pvckjwUnBLsI`
- Current Implementation Plan: `1eGVajQAxw3Vjc7F_7NJgt9do6tRzZpV_Vbfcl24g9-s`
- Open Tasks & Gates: `1BH44EUYcNBexIZasxq24mlYBZk0VxF5XaG6RnUkQPrc`
- Changelog: `1JsJfEECcWaB2UJtW0oiW45RD86RZT4i5spANV38Zzoc`
- Release & Test Evidence: `1LizDehSMDnBfihXnntX9z13QNai87zzMwlkzPptkcB0`
- Recursive Handover: `1HVfL2KV6LZbpl0fc4Je1dzqLs3ya9q9Onjb3YbFF9L8`

Update these documents in place; never create competing current-state copies.

## Current next tranche — ACTIVE HANDOVER #9

Tasks 1–8 of `0.1.0-dev.21 — B8 Video Copy/Remux/Transcode Planner` are verified.

The precise next executable block is **Task 9 RED/GREEN — actual packaged ffprobe/ffmpeg descendant containment and parser/protocol attack-surface qualification**:

1. Prove fixed package locations for ProbeWorker+ffprobe and EngineWorker+ffmpeg; missing/untrusted/reparse paths fail closed.
2. Prove no PATH/CWD/user-binary fallback.
3. Demonstrate actual `ffprobe.exe` and `ffmpeg.exe` descendants inside the strict Job/AppContainer containment model, not only a generic WorkerCanary.
4. Prove DNS/TCP/network attempts from descendant context are denied.
5. Prove reads/writes outside authorized staging/tool scope are denied; ProbeWorker remains exact staged-input read-only.
6. Prove timeout/cancel/failure kills the complete descendant Job and leaves zero ffprobe/ffmpeg/worker orphans.
7. Qualify the exact pinned-engine local `file` protocol posture.
8. Qualify the exact demuxer/format set required by the supported dev.21 source matrix; do not guess or broaden an allowlist.
9. Preserve all Tasks 1–8 and dev.20 Audio/Image/Video/security regressions.
10. Keep generated authority unsynchronized until guarded stabilization; make no production redistribution or release claim.

After Task 9: Task 10 packaged real Copy/Remux/Transcode qualification → Task 11 governance/supply-chain hardening → Task 12 guarded generated-authority stabilization/exact-candidate freeze → remaining production engine provenance/licensing/signing/MSIX/headed/fuzz/security/end-user release gates.

## Architecture / security invariants

`IExplorerCommand DLL -> fixed app-local Bridge -> private staging -> strict disposable read-only ProbeWorker/fixed ffprobe -> typed bounded facts -> Core VideoPlanningPolicy/existing ConversionMode -> strict EngineWorker -> managed Copy OR provider-owned fixed Remux/Transcode tokens -> fixed app-local FFmpeg -> private staged output -> strict post-probe TargetMediaContract -> transactional numbered no-overwrite publication.`

Never widen to shell command construction, raw FFmpeg argument pass-through, PATH/CWD binary lookup, arbitrary converter/plugin discovery, ordinary conversion network dependency, silent Strict-to-Compatibility fallback, hardware acceleration or repository signing private keys.

## Continuation / handover lifecycle

For `weiter`, `continue`, `start current documented handover` or equivalent:

1. Read ACTIVE HANDOVER #9 TS `1789561580.123109`.
2. Fresh-read GitHub refs/CI and reconcile Drive/Slack against GitHub authority.
3. Execute Task 9 under Superpowers/TDD/security/review/evidence gates rather than re-planning approved work.
4. Verify the completed material block.
5. Update Authority + Roadmap + Plan + Tasks + Changelog + Evidence + Recursive Handover and canonical Slack anchors in place.
6. Fresh-read GitHub/CI again.
7. Mark #9 PROCESSED **before** publishing its successor.
8. Publish exactly one context-free successor OPEN, backfill its exact TS/reference into #9, Recursive Handover and this metadata routing map.
9. Re-read Slack + Recursive Handover and require exactly one OPEN.

Never hand-edit generated SBOM/package/hash authority. Never move frozen `main` merely for documentation synchronization.
