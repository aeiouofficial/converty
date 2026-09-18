# Converty continuation handover — dev.21 Task 12 authority stabilization

Repository: `aeiouofficial/converty`  
Default branch: `main`  
Development branch: `dev/0.1.0-dev.21`

Read `docs/HANDOVER_PROMPT.txt` first and fresh-read GitHub before writes or completion claims.

## Frozen release authority
`0.1.0-dev.20` remains frozen at exact main `8a1f46603aa842728247bc11b34fcccf121858fd`, tree `4bd6f8d7acbadd60a3488870c773d2eafd67ba26`, exact-main CI `33671671714` SUCCESS.

## Current dev.21 pre-authority evidence
Engineering head before Task-12 metadata curation: `277f6c30f5fd22b3107604304717e56839e76641` / tree `b8c1f9ea203e532e235a94174740bb23821c7d86`. CI `35299376030`: 392/392 managed, 132/132 static, 5/5 vectors, 19/19 dependency audit with zero vulnerable-result packages, Build 0 warnings/errors, all recursive product/security gates PASS before the expected stale-authority closure.

Generated artifact `10529710374` / `sha256:0029b1def03e329572ef221edc8779c89158a695b0be26ce02a05487e01e99ec` independently passes digest, CRC and exact four-member verification, but its package/SBOM version is still `0.1.0-dev.20`. Do not synchronize it as final dev.21 authority.

## Exact next action
1. Let ordinary CI run on the exact metadata-curated dev.21 head.
2. Obtain and independently verify the fresh generated-authority artifact: digest, CRC, exact four members and `0.1.0-dev.21` alignment.
3. Use the historical guarded exact-parent/exact-branch/self-deleting workflow to synchronize only those four files.
4. Require exact synchronized candidate generated-authority zero-diff and complete Windows deterministic workspace/delivery qualification.
5. Independently verify final generated-authority and verified-delivery artifacts.
6. Stop before main promotion while any required governance/signing/provenance/headed-release prerequisite is open; document the blocker rather than weakening a gate.

## Invariants
`Explorer → fixed Bridge → private staging → strict RO ProbeWorker/fixed ffprobe → typed bounded facts → Core planner → strict EngineWorker → managed Copy OR provider-fixed Remux/Transcode → fixed app-local FFmpeg → post-probe TargetMediaContract → transactional numbered no-overwrite publication`.

No shell/raw FFmpeg/PATH-CWD lookup/arbitrary executable or plugin/ordinary conversion network/silent Strict→Compatibility/hardware acceleration/private signing keys.

## Recursive lifecycle
Reconcile GitHub first, then canonical Drive/Slack in place. Mark the current OPEN Slack handover PROCESSED before publishing exactly one successor OPEN. Preserve exact evidence and explicitly unverified release claims.
