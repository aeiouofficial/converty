# Converty — current repository OPEN handover (GitHub-only)

Date: 2026-09-24 (Europe/Amsterdam). Project: `aeiouofficial/converty`. Workspace: `D:\converty\repo` (all temporary files, caches, downloads and logs strictly within `D:\converty`). Status: **OPEN / NOT CUSTOMER SHIP-READY**. This file is the single current repository continuation point; historic Slack/Drive handovers are not current authority for this GitHub-only scope.

## Authority and immutable baselines

- Frozen production/release `main`: `8a1f46603aa842728247bc11b34fcccf121858fd`, tree `4bd6f8d7acbadd60a3488870c773d2eafd67ba26`; old exact-main CI `33671671714` SUCCESS. Do not move without real production gates.
- Immutable qualified dev.21: `c3bc042aea3154e30ce2720db4722cbda27bcb34`, tree `47ecb6f1f952fcdf286e60dd81935a0d81af56ee`. Do not change.
- Development branch: `dev/0.1.0-dev.22-shipping-readiness`. Last source-code subject before this evidence-only amendment: `6292ccb0a2a1eb5bfcb3dbe23bc99fddc50d5ce3` (the previously frozen dev.22 local test source). **Fresh-read GitHub for the new final pushed commit**; a tracked handover cannot embed its own SHA without changing that SHA.
- Prior GitHub Actions-qualified dev.22 subject is **not this amended tree**: `1aea099a2878f72f8c95a5cd3bd1dee98a1f98b3` / tree `8369b929f3e8b48f76f720ae3a954818acf2a25a` / CI `35937520600`. Previous authority artifact `10783376753`; previous delivery artifact `10783368416`. Preserve this evidence historically, never transfer its qualification to new commits.
- Draft review: https://github.com/aeiouofficial/converty/pull/14 ; genuine external shipping blockers: https://github.com/aeiouofficial/converty/issues/13 .

## Completed in this local work block

- Installed repository-pinned .NET SDK 10.0.400 under `D:\converty\_toolchain\dotnet` and used PowerShell 7. Local locked restore, 19-project NuGet audit (0 vulnerable-result packages), Release build (0 warnings/errors), 5/5 contract vectors and native MSVC Explorer DLL + smoke executable build PASS.
- RED: two pre-existing Windows security tests failed before their assertion because directory symlinks require privileges unavailable on this laptop (393/395). GREEN: use non-elevated Windows directory junctions in the two test-only fixtures and explicitly verify `ReparsePoint`; all **395/395 managed tests PASS**, no skips. Production security behavior unchanged.
- RED: Python release SBOM emitted CRLF under Windows; four-file LF regression failed 1/4. GREEN: source/release SBOM, package manifest and SHA256SUMS generators now force LF; four-file regression 4/4 and full **168/168 static tests PASS**.
- Unsigned development MSIX layout/schema PASS with exact pinned FFmpeg/ffprobe. The initial corrupt download remained rejected, then a new fully verified archive unlocked packaged ProbeWorker, direct staged COM, product, Audio36, Image24, Video27 and mode/mixed regressions: all PASS. Registered unsigned MSIX COM remains BLOCKED by laptop sideload policy `0x80073CFF`. See `docs/development/DEV22_LOCAL_CI_EVIDENCE_2026-09-24.md`; no GitHub Actions were invoked.

## Current exact next executable step
1. Fresh-read GitHub current dev.22 head, frozen main, draft PR #14 and shipping issue #13. Prior engineering/source subject `6292ccb0a2a1eb5bfcb3dbe23bc99fddc50d5ce3` has completed fresh local packed regression, but **fresh-read the new metadata/authority commit SHA rather than copying the old source SHA as HEAD**.
2. Preserve the verified local exact-pinned FFmpeg evidence (192925997 bytes, SHA-256 `fe372180f20e7f9bfa3d9a481b2b1b98c8296178d8265552608736637ea6b3c8`) and all tested gates: managed 395/395, static 168/168, vectors 5/5, native MSVC, unsigned full dev MSIX, packaged ProbeWorker, direct Explorer COM Invoke, Audio36, Image24, Video27, negatives/mixed, Copy/Remux/Transcode **PASS**.
3. **The remaining local execution gate is registered unsigned MSIX COM:** `Add-AppxPackage -Register` failed on this laptop with Windows `0x80073CFF` because sideload/developer policy does not permit the unsigned package. Do not alter the laptop's developer or security settings without explicit user authorization. Use an expressly provisioned permitted Windows 11 test VM or approved signing environment; perform real registered COM, clean headed Explorer screenshot and install/update/uninstall acceptance with exact package bytes. The passing direct staged COM smoke is not a substitute.
4. Keep production FFmpeg provenance/redistribution, independently verified signed MSIX/B2, live main governance, final fuzz/chaos/security/UX/Plugin SDK/end-user approval and independent whole-branch PR review OPEN in issue #13. An unsigned development package cannot authorize production shipping.
5. For future documentation edits regenerate all four tracked deterministic authority files via scripts; run static checks and independently inspect two byte-identical ZIPs from **committed HEAD** (CRC, every package-manifest and SHA256SUMS row). Use local CI only; commit/push `[skip ci]`, do not dispatch GitHub Actions. Update issue #13 and draft PR #14 in place, preserve frozen main and exactly one OPEN repository handover.

## Completed pinned packaged block
- Predecessor handover was the 2026-09-24 local portability/FFmpeg-download OPEN state; this in-place repository file supersedes it after actual evidence and retains historical provenance in Git.
- The exact archived download now passed before extraction; historical partial downloads were never trusted. Packaged ProbeWorker and direct staged Explorer class-factory/Invoke PASS; unsigned MSIX schema PASS. All eight independent product/matrix/mixed/mode scripts PASS. Captured local logs under `D:\converty\_temp`.
- Full packaging qualification did **not** pass because registered unsigned package activation was blocked by Windows policy. No GitHub Actions, no main movement and no customer-release claim. Canonical ledger: `docs/development/DEV22_LOCAL_CI_EVIDENCE_2026-09-24.md`.

## Architecture, security and continuation rule

Explorer → fixed app-local Bridge → private staging → read-only strict ProbeWorker/fixed ffprobe → bounded facts/Core policy → strict EngineWorker → managed byte-exact Copy+SHA-256 or provider-owned fixed FFmpeg Remux/Transcode → strict post-probe TargetMediaContract → destination-volume durable temp → transactional numbered no-overwrite publish.

Never add shell/raw FFmpeg argument execution, PATH/CWD discovery, silent Strict→Compatibility fallback, ordinary conversion network, arbitrary plugins, hardware acceleration, or private signing keys. Do not reinterpret a local development PASS as production signing, independent end-user acceptance or release approval.

On `weiter` / `continue`, first fresh-read this single OPEN repository handover, current GitHub branch/PR/issue, plan, backlog, changelog and evidence; execute the immediate task, verify actual gates, update the existing canonical files **in place**, fresh-read GitHub again, and replace this same repository handover with its one OPEN successor. Prior states live in Git history. Slack/Drive reconciliation is out of scope until explicitly requested.

\n