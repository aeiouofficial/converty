# Converty

Windows 11 modern-context-menu file conversion platform. Converty keeps Explorer, Bridge, disposable ProbeWorker/EngineWorker, provider/media-engine, private staging and transactional publication trust boundaries explicit.

## Workspace version
**0.1.0-dev.22** — audit remediation and shipping-readiness hardening are behavior-qualified on `dev/0.1.0-dev.22-shipping-readiness`; exact-candidate development qualification succeeded; this documentation amendment requires fresh authority synchronization and qualification. This branch is **not release authority** and Converty is **not customer ship-ready**.

## Frozen release authority
The sole frozen release remains **0.1.0-dev.20** at exact `main` `8a1f46603aa842728247bc11b34fcccf121858fd`, tree `4bd6f8d7acbadd60a3488870c773d2eafd67ba26`, exact-main CI `33671671714` SUCCESS.

The immutable qualified dev.21 development candidate is `c3bc042aea3154e30ce2720db4722cbda27bcb34`, tree `47ecb6f1f952fcdf286e60dd81935a0d81af56ee`. It was not promoted because required live governance and external production release prerequisites remain open.

## Exact dev.22 development qualification — 2026-09-24
Subject commit `1aea099a2878f72f8c95a5cd3bd1dee98a1f98b3`, tree `8369b929f3e8b48f76f720ae3a954818acf2a25a`; ordinary CI [35937520600](https://github.com/aeiouofficial/converty/actions/runs/35937520600):
- `managed` SUCCESS: 395/395 tests, full Native Explorer/package/ProbeWorker/COM/product smoke, Audio36/Image24/Video27 plus negative/mixed and Copy/Remux/Transcode PASS;
- `supply-chain-static` SUCCESS: 162/162 tests, vectors 5/5, verified tracked generated-authority zero-diff;
- `candidate-base-continuity` SUCCESS; development-branch `main-authority-continuity` remains expected RED, and `candidate-release-readiness` is correctly BLOCKED by genuinely OPEN external approvals.
- Generated-authority artifact 10783376753, SHA-256 `c3b79e9db86c52585c9c3db00d05504d43a0b6991f6c323c9f6cdf302eb1df59`.
- Verified-delivery artifact 10783368416, SHA-256 `2697a85aa3b2a5f51f1446afb37e348bb242177dc29b11dbf3b8416b422ba207`. Independently checked embedded workspace ZIP SHA-256 `337bdb018274866803e8b6e15f7b9ea0fad4436ac3d2b912d61f340cfc76cf47`, 646844 bytes, 462 entries, CRC clean, 460/460 manifest hashes and 461/461 SHA256SUMS entries.
- This documentation/storage-policy amendment must itself be regenerated and requalified at its final exact GitHub SHA. See draft PR #14 for the latest exact-head CI and external-gate issue #13.

## Local workspace storage
Local Converty temp/caches are prohibited on the laptop's C: drive. Two previously identified C: temp folders (13 files, approximately 2.6 MB) were copied with exact per-file SHA-256 verification to `D:\converty\_temp\migrated-from-C` and originals removed. `D:\converty\setup-workspace-env.ps1` configures the laptop's per-process caches/temp. Repository agents must source `build/use-workspace-temp.ps1` before local project commands; hosted GitHub runner OS tools are separate from the laptop. See `AGENTS.md`.

## Dev.22 hardening
- production release evidence is structured, hash-validated and bound to an exact qualified subject/artifact;
- authoritative readiness requires live GitHub governance data; offline reporting cannot declare ship-ready;
- candidate-base continuity is non-circular and separate from post-promotion main authority;
- content-level private-key/token scanning is part of release preflight;
- partial batch failures preserve and report successful members;
- validated outputs publish via a durable destination-volume temporary file followed by same-volume no-overwrite rename;
- stale worker staging cleanup is marker-owned, age-bounded and does not traverse reparse points;
- development FFmpeg qualification uses a tagged BtbN asset with exact asset ID, source commit, byte size and SHA-256; it is not production redistribution authority.

## Trust architecture
`IExplorerCommand → fixed Bridge → private staging → strict read-only ProbeWorker / fixed ffprobe → typed bounded facts → Core planner → strict EngineWorker → managed Copy OR provider-fixed Remux/Transcode → fixed app-local FFmpeg → post-probe TargetMediaContract → destination-volume durable temp → transactional numbered no-overwrite publication`.

Never introduce shell command construction, raw FFmpeg argument pass-through, PATH/CWD lookup, arbitrary converter/plugin execution, ordinary conversion network dependency, silent Strict→Compatibility fallback, hardware acceleration, or repository signing private keys.

## Exact next closure sequence
1. Run ordinary CI on the exact metadata-curated dev.22 head.
2. Independently verify the generated-authority ZIP digest, CRC, exact four-member set and `0.1.0-dev.22` alignment.
3. Synchronize only the four generated authority files through a guarded exact-parent/exact-branch workflow.
4. If bot sync does not trigger CI, create a no-tree-change qualification-trigger commit.
5. Require generated-authority zero-diff, full Windows managed/product qualification and deterministic verified delivery on the exact synchronized candidate.
6. Independently verify final generated-authority and delivery artifacts.
7. Do **not** promote `main` while any genuine external release prerequisite remains OPEN.

## Still open before customer launch
- live GitHub ruleset/main branch-protection enforcement;
- headed Windows 11 modern Explorer exact-build UI/screenshots and crash/hang/failure matrix;
- production signed-package B2 identity/authentication requalification;
- production FFmpeg/ffprobe provenance, signatures, hashes, license/notices and redistribution approval;
- signed production MSIX clean Windows 11 install/update/uninstall acceptance;
- UX/settings and Plugin SDK release gates;
- final fuzz/chaos/security/release/end-user acceptance.

## Start here
1. `docs/HANDOVER_PROMPT.txt`
2. `docs/HANDOVER_NEXT_AGENT.md`
3. `machine-readable/handover_state.json`
4. `machine-readable/build_evidence.json`
5. `docs/development/IMPLEMENTATION_STATUS.md`
6. `docs/superpowers/plans/2026-09-21-dev22-audit-remediation-shipping-hardening.md`
7. `docs/TASK_BACKLOG.md`

Generated SBOM/package/hash authority is CI-derived only. Never hand-edit `machine-readable/source_sbom.spdx.json`, `machine-readable/release_sbom.spdx.json`, `machine-readable/package_manifest.json` or `SHA256SUMS.txt`.
