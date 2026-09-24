# Converty — current repository OPEN handover (GitHub-only)

Date: 2026-09-24 (Europe/Amsterdam). Project: `aeiouofficial/converty`. Workspace: `D:\converty\repo` (all temporary files, caches, downloads and logs strictly within `D:\converty`). Status: **OPEN / NOT CUSTOMER SHIP-READY**. This file is the single current repository continuation point; historic Slack/Drive handovers are not current authority for this GitHub-only scope.

## Authority and immutable baselines

- Frozen production/release `main`: `8a1f46603aa842728247bc11b34fcccf121858fd`, tree `4bd6f8d7acbadd60a3488870c773d2eafd67ba26`; old exact-main CI `33671671714` SUCCESS. Do not move without real production gates.
- Immutable qualified dev.21: `c3bc042aea3154e30ce2720db4722cbda27bcb34`, tree `47ecb6f1f952fcdf286e60dd81935a0d81af56ee`. Do not change.
- Development branch: `dev/0.1.0-dev.22-shipping-readiness`. Last observed remote baseline before this local block: `ca018a2cdafcfef9e4198a0aa721b6fa66464a74`, tree `f589ce536e95f470789ab4a146af904fe54b62de`. **Fresh-read GitHub for the new final pushed commit**; a tracked handover cannot embed its own SHA without changing that SHA.
- Prior GitHub Actions-qualified dev.22 subject is **not this amended tree**: `1aea099a2878f72f8c95a5cd3bd1dee98a1f98b3` / tree `8369b929f3e8b48f76f720ae3a954818acf2a25a` / CI `35937520600`. Previous authority artifact `10783376753`; previous delivery artifact `10783368416`. Preserve this evidence historically, never transfer its qualification to new commits.
- Draft review: https://github.com/aeiouofficial/converty/pull/14 ; genuine external shipping blockers: https://github.com/aeiouofficial/converty/issues/13 .

## Completed in this local work block

- Installed repository-pinned .NET SDK 10.0.400 under `D:\converty\_toolchain\dotnet` and used PowerShell 7. Local locked restore, 19-project NuGet audit (0 vulnerable-result packages), Release build (0 warnings/errors), 5/5 contract vectors and native MSVC Explorer DLL + smoke executable build PASS.
- RED: two pre-existing Windows security tests failed before their assertion because directory symlinks require privileges unavailable on this laptop (393/395). GREEN: use non-elevated Windows directory junctions in the two test-only fixtures and explicitly verify `ReparsePoint`; all **395/395 managed tests PASS**, no skips. Production security behavior unchanged.
- RED: Python release SBOM emitted CRLF under Windows; four-file LF regression failed 1/4. GREEN: source/release SBOM, package manifest and SHA256SUMS generators now force LF; four-file regression 4/4 and full **168/168 static tests PASS**.
- Native unsigned development MSIX layout/schema validation PASS; FFmpeg-dependent packaged execution tests NOT VERIFIED because the exact pinned archive download failed size/digest verification. Evidence ledger: `docs/development/DEV22_LOCAL_CI_EVIDENCE_2026-09-24.md`; machine-readable local evidence is separate from historical Actions results. No GitHub Actions were invoked.

## Immediate next executable step

1. Obtain the **exact hash-pinned development-only** FFmpeg/ffprobe archive specified in `eng/ffmpeg-development.json`; verify byte count and archive SHA-256 **before** extraction or use. The attempted archive download failed size/digest verification (150933805 bytes vs required 192925997; corrupt payload rejected before extraction). Do not trust this partial archive or substitute unpinned executables.
2. Run all real packaged native/unsigned-MSIX/ProbeWorker/COM/product Audio36, Image24, Video27, negative/mixed, Copy/Remux/Transcode and strict containment checks locally. Record PASS/FAIL/NOT VERIFIED per gate and retain non-sensitive logs under `D:\converty\_temp`.
3. After final source/docs edits, regenerate the **four** authority files with committed Python generators, never hand-edit them. Run 395 managed, 168 static and 5 vectors again if any code changes; verify deterministic repeat generation, build workspace ZIP twice byte-identically, independently inspect CRC, each manifest entry and each SHA256SUMS row.
4. Push durable validated commits to the existing dev.22 branch with `[skip ci]` and **do not dispatch GitHub Actions** (minutes exhausted). Fresh-read PR #14, GitHub branch and Codex review comments; reconcile this handover, backlog, changelog and evidence. Do not merge frozen main.
5. Remaining external production approvals in #13 require actual immutable evidence: live main governance, production FFmpeg licenses/provenance/redistribution, signed MSIX/B2 identity, clean headed Windows 11 install/update/uninstall and Explorer screenshots, crash/hang/fuzz/chaos/security/end-user acceptance, UX/settings/Plugin SDK and independent release review.

## Architecture, security and continuation rule

Explorer → fixed app-local Bridge → private staging → read-only strict ProbeWorker/fixed ffprobe → bounded facts/Core policy → strict EngineWorker → managed byte-exact Copy+SHA-256 or provider-owned fixed FFmpeg Remux/Transcode → strict post-probe TargetMediaContract → destination-volume durable temp → transactional numbered no-overwrite publish.

Never add shell/raw FFmpeg argument execution, PATH/CWD discovery, silent Strict→Compatibility fallback, ordinary conversion network, arbitrary plugins, hardware acceleration, or private signing keys. Do not reinterpret a local development PASS as production signing, independent end-user acceptance or release approval.

On `weiter` / `continue`, first fresh-read this single OPEN repository handover, current GitHub branch/PR/issue, plan, backlog, changelog and evidence; execute the immediate task, verify actual gates, update the existing canonical files **in place**, fresh-read GitHub again, and replace this same repository handover with its one OPEN successor. Prior states live in Git history. Slack/Drive reconciliation is out of scope until explicitly requested.

\n