# Converty Cloud Documentation Map

Last reconciled: 2026-09-02.

This file is a metadata-only continuation map. It does **not** replace GitHub as code/release authority and does **not** supersede the frozen `main` release tree.

## Authority order

1. GitHub repository / exact refs / CI artifacts are authoritative for code and release evidence.
2. Slack is the live operational mirror for current project state, roadmap, implementation plan, tasks, changelog and the single open handover.
3. Google Drive is the persistent cloud documentation library and backup mirror.
4. After every completed tranche, update all three layers coherently. At least once daily, reconcile Slack/Drive against live GitHub refs and CI.

## Frozen dev.19 release authority

- Version: `0.1.0-dev.19`
- `main`: `eb0ce66dab646427d5bef1548c12e5cc4765b2f1`
- Tree: `337a4e11fb41bab6b6eeb462c3755381580f06c1`
- Exact-main run: `33597504612`
- Continuity: `100143814059` SUCCESS
- Static/supply-chain: `100143814189` SUCCESS
- Windows managed: `100143814261` SUCCESS
- Workspace SHA-256: `167b4695cca6810fe0e36e57c45a7bf11483105c0e71b955a948604f2cd9e584`
- Exact-main verified-delivery artifact: `9833955082`
- Verified-delivery digest: `sha256:6e7b7e753a4101a3e690ffe558c96e47f9dba831b858bed5f0005ec144837809`

## Slack live documentation

Workspace channels:

- `#proj-converty` — channel ID `C0BUFGMGMFG`
  - canonical anchor TS `1788366973.077219`
- `#roadmap-converty` — channel ID `C0BU2405ZMM`
  - canonical anchor TS `1788366984.732379`
- `#plan-converty` — channel ID `C0BUKLHKL65`
  - current implementation plan; keep one canonical current-plan anchor
- `#tasks-converty` — channel ID `C0BTWQZQX4P`
  - canonical anchor TS `1788327299.747159`
- `#changelog-converty` — channel ID `C0BUM4XRZ6G`
  - canonical anchor TS `1788366995.127219`
- `#handover-open-converty` — channel ID `C0BUM8J0ZEG`
  - current OPEN handover TS `1788367585.736179`
  - there must be exactly one `OPEN` handover at a time
- `#pre-devlog-converty` — channel ID `C0BV6HDMVDW`

Slack Canvas is not available in this workspace (`not_supported_free_team`). Therefore each live documentation channel uses canonical anchor messages edited in place, while full durable documents live in Google Drive.

### Open handover lifecycle

After every meaningful completed work block:

1. Re-read live GitHub authority and relevant CI/evidence.
2. Reconcile Authority + Roadmap + Plan + Tasks + Changelog + durable Recursive Handover.
3. Edit the current `OPEN` handover to `PROCESSED` and add the successor handover reference. If direct edit is impossible, add an explicit processed reply before creating the successor.
4. Publish exactly one successor `OPEN` handover containing the precise next task, current authority, acceptance criteria, blockers and architecture/security invariants.
5. Never leave more than one active `OPEN` handover for Converty.

## Google Drive library

Project Documentation Library:

- https://drive.google.com/drive/folders/1h0GoSM8MfRy8GUjQa6hMLIQbhzEfkoT_

Converty root:

- https://drive.google.com/drive/folders/1zSKLK-yKmX15xIWSj1D_39tmArymhHmn

Subfolders:

- `00 Authority`: `1Wz5Bz14GlX89UK7NN6kRUE4uUjmR5wnr`
- `01 Roadmap`: `1EszeYUEChqWOXeyQa1CCXSGH1qpgthuX`
- `02 Tasks`: `1yWwF_PyXwwXWMsN9zKvc_dqvPTU12-rI`
- `03 Changelog`: `19zRdZz3yQ3MW_q1oYkMCmZZJNilHp9wg`
- `04 Handover`: `1FOS_EQfv65Hp4fZAiXDkw2FzxUDEBz4H`
- `05 Devlogs & Release Evidence`: `1wZOfy2kzffGyXMtMq05MgSB4Xli2p-Yl`

Fixed live Google Docs — update these IDs in place; do not create replacement copies:

- Project Authority & Documentation Index
  - `1ZdDGUpSVxeEfvICLKD_VctT49MlJMyhNICyj4ebeYRw`
  - https://docs.google.com/document/d/1ZdDGUpSVxeEfvICLKD_VctT49MlJMyhNICyj4ebeYRw/edit
- Live Roadmap
  - `1p3xKxj2akSqZTzVp442QNetoZ8Eg9u6pvckjwUnBLsI`
  - https://docs.google.com/document/d/1p3xKxj2akSqZTzVp442QNetoZ8Eg9u6pvckjwUnBLsI/edit
- Current Implementation Plan
  - `1eGVajQAxw3Vjc7F_7NJgt9do6tRzZpV_Vbfcl24g9-s`
  - https://docs.google.com/document/d/1eGVajQAxw3Vjc7F_7NJgt9do6tRzZpV_Vbfcl24g9-s/edit
- Open Tasks & Gates
  - `1BH44EUYcNBexIZasxq24mlYBZk0VxF5XaG6RnUkQPrc`
  - https://docs.google.com/document/d/1BH44EUYcNBexIZasxq24mlYBZk0VxF5XaG6RnUkQPrc/edit
- Changelog
  - `1JsJfEECcWaB2UJtW0oiW45RD86RZT4i5spANV38Zzoc`
  - https://docs.google.com/document/d/1JsJfEECcWaB2UJtW0oiW45RD86RZT4i5spANV38Zzoc/edit
- Recursive Handover
  - `1HVfL2KV6LZbpl0fc4Je1dzqLs3ya9q9Onjb3YbFF9L8`
  - https://docs.google.com/document/d/1HVfL2KV6LZbpl0fc4Je1dzqLs3ya9q9Onjb3YbFF9L8/edit

## Current approved next tranche

`dev.20 Video Qualification Closure — Design A` is approved.

Use the already-existing fixed Video surface:

- Sources: `.mp4`, `.mov`, `.mkv`, `.avi`, `.webm`, `.m4v`, `.mpeg`, `.mpg`, `.wmv`
- Actions: `video.mp4.h264`, `video.webm.vp9`, `extract.audio.mp3`
- Target: 27 real packaged source/action conversions, repeated malformed/truncated rejection, mixed-valid/invalid Video batch isolation, full Audio/Image regression preservation, and the existing authority/freeze protocol.
- Explicitly out of scope for dev.20: new Video actions, remux/copy planner, hardware acceleration, HDR/subtitle/metadata expansion.

## Synchronization rules for future agents

- Re-read live GitHub refs and CI before every status claim or write.
- Edit Slack canonical anchors in place using the channel IDs/message timestamps above.
- Update the fixed Google Docs in place using the IDs above.
- Append historical changelog entries; do not erase provenance.
- Keep Roadmap strategic, Plan execution-specific, and Tasks stateful/non-duplicative.
- Keep the Recursive Handover sufficient for a fresh account/model with no conversation history.
- Keep `#handover-open-converty` as the single operational start-point queue: exactly one OPEN handover, predecessor PROCESSED before successor OPEN.
- Never let Slack/Drive narrative override contradictory GitHub evidence.
- Do not edit generated SBOM/package/hash authority by hand.
- Do not move frozen `main` for documentation-only synchronization.
