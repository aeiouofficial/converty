# Converty — single repository OPEN handover (GitHub-only)
Date: 2026-09-27, Europe/Berlin. Status: **OPEN / NOT CUSTOMER SHIP-READY**.
Project: aeiouofficial/converty. Local workspace: D:\converty\repo; all project downloads, tools, temp, caches and logs strictly under D:\converty.
Predecessor: the 2026-09-27 test-only qualification handover in Git commit 29ce1b260adbb4d84e8e0e720c19186d0db4e84d is **PROCESSED** by this in-place replacement. This file is the only current repository OPEN continuation; older Slack/Drive handovers are historic and outside current GitHub-only scope.

## GitHub authority
- Frozen main: 0.1.0-dev.20, commit 8a1f46603aa842728247bc11b34fcccf121858fd, tree 4bd6f8d7acbadd60a3488870c773d2eafd67ba26. Historical exact-main CI 33671671714 SUCCESS; do not move main.
- Immutable qualified dev.21: c3bc042aea3154e30ce2720db4722cbda27bcb34, tree 47ecb6f1f952fcdf286e60dd81935a0d81af56ee. Never rewrite it.
- Development branch: dev/0.1.0-dev.22-shipping-readiness; current workspace version 0.1.0-dev.22, planned successor 0.1.0-dev.23 only after dev.22 closure.
- Earlier hosted dev.22 qualification: subject 1aea099a2878f72f8c95a5cd3bd1dee98a1f98b3, CI 35937520600; historical only, not an approval of later commits.
- Unchanged product implementation and pinned packaged local tests: subject 6292ccb0a2a1eb5bfcb3dbe23bc99fddc50d5ce3.
- Latest test-only engineering subject: 29ce1b260adbb4d84e8e0e720c19186d0db4e84d, tree 86f814dc461660d3f76919883c8e7e31c2683763. The follow-up documentation commit containing this file has a different HEAD; **fresh-read GitHub instead of guessing or embedding its own SHA**.
- Draft PR https://github.com/aeiouofficial/converty/pull/14; external shipping checklist https://github.com/aeiouofficial/converty/issues/13.

## Completed and verified
- Test-only Windows FFmpeg descendant cancellation fix: reliably awaits worker teardown, bounded retry of transient staged-DLL locks, 12-second bounded child PID wait, early worker-exit diagnostics. Actual child AppContainer and orphan assertions remain strict. Prior RED 394/395 plus intermittent PID startup failure; repeated isolated GREEN 3/3.
- Fresh 2026-09-27 laptop Release build under D:\converty and .NET SDK 10.0.400: 0 warnings/errors; full managed 395/395 PASS, no skips; Python 3.13.13 hash-locked environment, static 168/168 PASS, contract vectors 5/5 PASS; preflight, repository checks and immutable external action pins PASS.
- Four generated authority members regenerate deterministically and match tracked 29ce1b2 byte-for-byte. Two independent ZIPs built from that committed HEAD are identical: SHA-256 1aec74d58dc8586deec6b4b9e96e0ee2ff35266a91f2166afa99036450c89a1a, 667625 bytes/466 entries, CRC PASS, 464/464 manifest, 465/465 checksum rows, 4/4 exact authority bytes PASS. Log: D:\converty\_temp\postcommit-package-20260927.log.
- Historical exact pinned development-only FFmpeg archive: 192925997 bytes, SHA-256 fe372180f20e7f9bfa3d9a481b2b1b98c8296178d8265552608736637ea6b3c8, version n9.0.2-3-ga5923073bf. On unchanged product source 6292ccb, unsigned development MSIX schema, native Explorer DLL, packaged ProbeWorker and direct staged COM Invoke, Audio36/Image24/Video27, Copy/Remux/Transcode, negative/repeated mixed and orphan assertions PASS. This is **not** production provenance or a signed package.
- No GitHub Actions were dispatched for 29ce1b2; its commit used [skip ci]. Keep hosted Actions unused while budget is exhausted.

## Unverified gates and exact next executable task
- The local laptop **cannot register unsigned MSIX COM**: Add-AppxPackage failed 0x80073CFF (sideload/developer policy). Do not alter that machine's security settings. Direct staged COM PASS is not registered COM or clean headed Windows 11 acceptance.
- As freshly read 2026-09-27, main branch protected=false, repo rulesets=[], draft PR #14 has zero submitted independent reviews. Production FFmpeg redistribution/license/provenance, signed MSIX/B2, controlled clean headed Windows 11 Explorer install/update/uninstall, final security/fuzz/chaos, UX/settings and Plugin SDK and end-user approvals remain OPEN in issue #13.
- **Next executable step:** freshly inspect GitHub main branch/ruleset APIs and authenticated administration scope; implement and independently read back an applicable non-bypass main protection policy only if permissions and real required-check contexts permit accurate enforcement. Do not invent passing CI checks, start GitHub Actions or promote frozen main. Document any governance permission or required-check blocker in existing issue #13.
- Then use an expressly approved, correctly provisioned Windows 11 test host for exact-build registered COM, headed Explorer screenshots and complete MSIX install/update/uninstall acceptance. Do not claim it complete without real evidence. Obtain independent reviewer and the remaining external approvals before any customer release.

## Security invariants and continuation contract
Explorer -> fixed app-local Bridge -> private staging -> strict read-only ProbeWorker/fixed ffprobe -> typed facts -> bounded policy -> strict EngineWorker -> managed byte-exact Copy or provider-owned fixed FFmpeg Remux/Transcode -> post-probe TargetMediaContract -> same-volume durable no-overwrite transaction.
Never introduce raw shell FFmpeg arguments, PATH/CWD discovery, silent Strict-to-Compatibility fallback, ordinary conversion network, arbitrary plugins, GPU acceleration or private signing keys. Preserve exact subject/artifact qualification boundaries.
On a short continue: fresh-read this one OPEN handover, GitHub branch, frozen main, PR #14, issue #13, backlog and evidence; execute the next verifiable task; verify locally under D:\converty; update canonical plan/backlog/changelog/evidence in place and regenerate four deterministic authority files; commit/push [skip ci] without GitHub Actions; independently double-verify the exact final committed-HEAD ZIP; reconcile PR/issue; mark this predecessor PROCESSED in Git history and publish one in-place OPEN successor. No Slack/Drive updates unless explicitly restored to scope.
