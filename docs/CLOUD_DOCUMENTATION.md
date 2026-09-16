# Converty Cloud Documentation Map

Last reconciled: 2026-09-16.

This file is a **metadata-only continuation/routing map**. GitHub exact refs, code, CI and release evidence remain authoritative. Slack is the live operational mirror and Google Drive the persistent documentation/evidence library.

## Frozen release authority

- Version: `0.1.0-dev.20`
- `main`: `8a1f46603aa842728247bc11b34fcccf121858fd`
- Tree: `4bd6f8d7acbadd60a3488870c773d2eafd67ba26`
- Exact-main CI: `33671671714` — SUCCESS
- Managed: `260/260`; static: `103/103`; vectors: `5/5`
- Video `27/27` plus negative/mixed qualification; Audio 36-case and Image 24-case regressions PASS
- Workspace SHA-256: `743c375cff7d854e0d63ea184f2423cc49ff9a7dca552442670c2b4322c5c805`
- Generated-authority artifact: `9862733877`
- Verified-delivery artifact: `9862843977`

This remains the sole release/freeze authority. Converty is **NOT CUSTOMER SHIP-READY**.

## Current dev.21 engineering authority

- Branch: `dev/0.1.0-dev.21`
- Head: `09de85a8ab689bc5cbfd5e9e475001f3a28e0236`
- Tree: `b4991774aea7b5ad679a1980fc4162b092132ea8`
- Approved design: `d80cc33a2e7c38738f113856e95f1451fd2df1b0`
- Implementation plan: `e54061368b476184a89dcadb1d6b8a8f6fb6cf68`
- Plan: `docs/superpowers/plans/2026-09-03-dev21-video-copy-remux-transcode-security.md`

### Task 9 — actual engine descendant qualification — VERIFIED

Task 9 qualified the fixed bundled ffprobe/ffmpeg execution boundary and exact parser/protocol attack surface.

- CI run: `35106074028`
- Windows managed job: `104827500443`
- Dependency audit: 19 projects / 19 frameworks / 0 vulnerable-result packages
- Build: 0 warnings / 0 errors
- Managed: `392/392` PASS
- Static: `125/125` PASS
- Contract vectors: `5/5` PASS
- Audio 36, Image 24, Video 27 plus malformed/truncated/mixed isolation: PASS
- Real bundled ffprobe/ffmpeg child AppContainer inheritance: PASS
- Exact/prohibited filesystem-scope enforcement: PASS
- ffprobe and ffmpeg loopback TCP denial: PASS
- Job-wide ffmpeg descendant cleanup on cancellation: PASS
- Fixed fully-qualified/reparse-resistant engine paths: guarded
- Qualified production execution/probe protocol: `file` only
- Qualified format surface: `mov,matroska,avi,mpeg,asf`; `mp3` additionally required by probe validation
- Deterministic workspace double-build SHA-256: `cccfbae3dbf52b698b1c3e901e091001effdac3dae9c4c7f0a07450424d2fe5e`
- Workspace: 599445 bytes / 437 files
- Verification afterward failed only on intentionally stale tracked `.github/workflows/ci.yml` authority; delivery skipped
- No generated-authority synchronization, dev.21 delivery, freeze or release authority exists

Development engine qualification input remains `9.0.1-essentials_build-www.gyan.dev`, archive SHA-256 `fec81ae03971d9dd4be3ebe02e263bd2ec1d789483f931bdba5f5715e65da2e9`. This is not production provenance/licensing/notices/redistribution approval.

## Operational continuation

Slack `#handover-open-converty` (`C0BUM8J0ZEG`):

- Handover #8 — TS `1789410998.184939` — PROCESSED
- Handover #9 — TS `1789561580.123109` — PROCESSED
- **ACTIVE HANDOVER #10 — TS `1789568171.911999` — OPEN**
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

Canonical Drive documents:

- Authority `1ZdDGUpSVxeEfvICLKD_VctT49MlJMyhNICyj4ebeYRw`
- Roadmap `1p3xKxj2akSqZTzVp442QNetoZ8Eg9u6pvckjwUnBLsI`
- Plan `1eGVajQAxw3Vjc7F_7NJgt9do6tRzZpV_Vbfcl24g9-s`
- Tasks `1BH44EUYcNBexIZasxq24mlYBZk0VxF5XaG6RnUkQPrc`
- Changelog `1JsJfEECcWaB2UJtW0oiW45RD86RZT4i5spANV38Zzoc`
- Evidence `1LizDehSMDnBfihXnntX9z13QNai87zzMwlkzPptkcB0`
- Recursive Handover `1HVfL2KV6LZbpl0fc4Je1dzqLs3ya9q9Onjb3YbFF9L8`

## Current next task — ACTIVE HANDOVER #10

**DEV.21 Task 10 RED→GREEN — explicit packaged B8 Copy/Remux/Transcode mode qualification.**

1. RED: require a dedicated packaged mode-qualification smoke and CI gate.
2. Copy witness: target-compliant MP4/H.264/AAC/yuv420p/BT.709 -> `video.mp4.h264`; published output must be byte-identical to source by SHA-256.
3. Remux witness: target-compatible H.264/AAC MKV -> MP4; container changes while compressed primary video/audio packet hashes remain unchanged. Verify the pinned-engine packet-hash method before relying on it.
4. Transcode witness: qualified AVI MPEG-2/MP3 -> MP4; output H.264/AAC and existing TargetMediaContract PASS.
5. Preserve source/pre-existing-destination, numbered no-overwrite, Unicode/metachar paths, zero partial/orphan processes.
6. Preserve Video 27-case + negatives/mixed, Audio 36, Image 24, managed/static/vectors and Task-9 security canaries.
7. Do not synchronize generated authority or claim dev.21 release/freeze/delivery during Task 10.

After Task 10: Task 11 governance/supply-chain hardening -> Task 12 guarded generated-authority stabilization/exact-candidate qualification/deterministic delivery/non-force exact-SHA promotion -> production FFmpeg provenance/licensing/signing/MSIX/headed/fuzz/security/end-user release gates.

## Security invariant

`IExplorerCommand DLL -> fixed Bridge -> private staging -> strict RO ProbeWorker/fixed ffprobe -> typed bounded facts -> Core planner -> strict EngineWorker -> managed Copy OR provider-owned fixed Remux/Transcode -> fixed app-local FFmpeg -> private staged output -> TargetMediaContract -> transactional numbered no-overwrite publication.`

Never widen to shell/raw FFmpeg arguments, PATH/CWD binary lookup, arbitrary executable/plugin discovery, ordinary conversion network dependency, silent Strict-to-Compatibility fallback, hardware acceleration or repository signing private keys.

## Lifecycle rule

On `weiter`/`continue`: fresh-read ACTIVE HANDOVER #10, GitHub/CI and canonical Drive; GitHub wins; execute Task 10 fully under TDD/security/evidence gates; verify; update canonical cloud docs in place; fresh-read GitHub; mark #10 PROCESSED first; publish exactly one successor OPEN; backfill its exact TS into #10, Drive and this routing map; verify exactly one OPEN.

Never hand-edit generated SBOM/package/hash authority and never move frozen `main` merely for documentation synchronization.
