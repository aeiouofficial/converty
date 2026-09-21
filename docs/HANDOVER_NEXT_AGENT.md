# Converty continuation handover — dev.22 Task 8 authority stabilization

Repository: `aeiouofficial/converty`  
Default branch: `main`  
Development branch: `dev/0.1.0-dev.22-shipping-readiness`  
Workspace version after this curation: `0.1.0-dev.22`

## Frozen authorities
Frozen release authority remains `0.1.0-dev.20` at exact main `8a1f46603aa842728247bc11b34fcccf121858fd`, tree `4bd6f8d7acbadd60a3488870c773d2eafd67ba26`, exact-main CI `33671671714` SUCCESS.

Qualified immutable dev.21 candidate: `c3bc042aea3154e30ce2720db4722cbda27bcb34`, tree `47ecb6f1f952fcdf286e60dd81935a0d81af56ee`. Do not mutate it.

## Dev.22 pre-authority evidence
Behavior head before this metadata curation: `7e50fa9a1df4d0a8336d16596af488335424b792` / tree `a3a6deec5d753c6176fb40368a56d263b3beaa62`. CI `35664296931`:
- 395/395 managed PASS;
- 162/162 static PASS;
- 5/5 contract vectors PASS;
- dependency audit PASS, 0 vulnerable-result packages;
- Release build 0 warnings / 0 errors;
- Native Explorer/package/ProbeWorker/COM/product gates PASS;
- Audio36/Image24/Video27 plus negative/mixed regression PASS;
- packaged Copy/Remux/Transcode PASS;
- development FFmpeg tagged/hash-locked BtbN qualification PASS;
- deterministic double workspace ZIP PASS at SHA-256 `526588af945a4eeb04984dc17792af8dd10fc1d7f2a9e9006ab3976d448b2b0a`, 642060 bytes / 462 files;
- semantic archive check stops only on stale tracked generated authority.

Tasks 1-7 in the dev.22 audit-remediation plan are complete. Task 8 remains authority synchronization and exact-candidate qualification.

## Exact next action
1. Fresh-read the branch after this metadata commit.
2. Let ordinary CI generate dev.22 authority.
3. Download and independently verify the generated-authority artifact: digest, CRC, exact four members, package/SBOM version `0.1.0-dev.22`.
4. Guarded exact-parent/exact-branch synchronization of only:
   - `SHA256SUMS.txt`
   - `machine-readable/package_manifest.json`
   - `machine-readable/release_sbom.spdx.json`
   - `machine-readable/source_sbom.spdx.json`
5. Never hand-edit those files.
6. Trigger exact synchronized candidate qualification if bot sync does not trigger ordinary CI.
7. Require zero-diff generated authority, 395/395 managed or later exact count, all packaged gates, deterministic delivery and independent final artifact inspection.
8. Stop before `main` promotion while genuine external release gates remain OPEN.

## Open external release blockers
Live main governance/protection; production FFmpeg redistribution/provenance; production signed MSIX/B2; clean signed Windows 11 lifecycle; headed exact-build Explorer acceptance; UX/settings and Plugin SDK release gates; final fuzz/chaos/security/end-user acceptance.

## Invariants
`Explorer → fixed Bridge → private staging → strict RO ProbeWorker/fixed ffprobe → typed bounded facts → Core planner → strict EngineWorker → managed Copy OR provider-fixed Remux/Transcode → fixed app-local FFmpeg → post-probe TargetMediaContract → destination-volume durable temp → transactional numbered no-overwrite publication`.

No shell/raw FFmpeg/PATH-CWD lookup/arbitrary executable or plugin/ordinary conversion network/silent Strict→Compatibility/hardware acceleration/private signing keys.

## Scope note
Current user instruction is GitHub-only. The historical Slack/Drive handover #12 is stale relative to GitHub and was not mutated during dev.22 work.
