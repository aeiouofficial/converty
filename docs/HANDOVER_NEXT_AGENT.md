# Converty continuation handover — dev.19 Image batch isolation pre-authority closure

Repository: `https://github.com/aeiouofficial/converty`  
Default branch: `main`

Read `docs/HANDOVER_PROMPT.txt` first; it is the canonical context-free recursive continuation prompt. Re-fetch live refs before every write or completion claim.

## Frozen baseline
Dev.18 exact-main authority: commit `ef079f7e7923e399624067c4d54b9ce7577bf090`, tree `0af729f150897d170eac9f9aebfd5bc7d5d4083a`, run `33390111824`. Managed `99481546832`, static `99481546572`, continuity `99481546655` all SUCCESS. Workspace SHA-256 `4be8d5a2f503a8a885347b647bbd0aa0b61ce6d56b3ac39d9af7b11fb801628a`.

## Dev.19 behavior anchor
Branch: `dev/0.1.0-dev.19-image-batch-isolation`.
Behavior SHA: `8a64d0a12ccf47df5df364a1c6c545f876d57d29`.
Behavior run: `33596229372`; managed `100140113348`; static `100140113457`; continuity `100140113271`.

Dev.19 adds real Image mixed-batch acceptance without changing the trust architecture. One Bridge process receives valid → malformed → valid → truncated → valid Image members. Ordinary per-file failures do not suppress later valid members; aggregate failure is reported after the selection; valid outputs publish numbered/no-overwrite; invalid members publish nothing; sources and pre-existing destinations remain unchanged; partial staging and converter processes must be cleaned.

## Qualification corrections made during this continuation
1. Run `33593266879` exposed `Source preserved invariant failed for valid-before`. Root cause: the harness derived `.png` target from a `.png` source and wrote collision sentinel bytes over the source itself.
   - Initial reproducer: `ce54e0f08b5dfd9ba1b4d6cba6132c64f7ce006f` / run `33595367172`.
   - Corrected RED: `a988204b058ada86f8909cf94e1d9f2b6e69cf39` / run `33595474461`: `1 failed, 98 passed`, exact missing source/target guard.
   - GREEN: `633fc39b5df8062496914cc641b7001adea805ee`; collision seeding is skipped only when target aliases source. Same-extension conversion remains valid.
2. The next full managed run exposed a core-test fixture defect: the test deleted `first.png` before `RunAsync`; product input validation correctly rejected it.
   - GREEN: `8a64d0a12ccf47df5df364a1c6c545f876d57d29`; the source remains present and the resolver publishes `first (1).png` naturally. Production code was not changed.

## Behavior evidence
Run `33596229372` established:
- 18/18 locked managed restore and dependency audit with 0 vulnerable-result packages;
- Release build: 0 warnings / 0 errors;
- native Explorer, development package, COM registration, packaged Bridge→FFmpeg: PASS;
- Audio 36/36 source/action plus repeated malformed/truncated rejection and mixed batch: PASS;
- Image 24/24 source/action plus repeated malformed/truncated rejection and mixed batch twice: PASS;
- 255/255 managed tests: PASS;
- 99/99 static tests: PASS;
- 5/5 raw contract vectors: PASS.

Pre-authority workspace ZIP was built twice byte-identically: SHA-256 `1c8d197941a616a25bcc4bab59550037a221309f51bc937a1ba7daa9b34bf97d`, 475353 bytes, 378 entries. The next semantic archive assertion correctly failed because the tracked package manifest still says dev.18. Therefore no verified-delivery artifact exists for this pre-authority run. Do not treat that ZIP as final delivery evidence.

## Evidence components
- `build/image-batch-isolation-smoke.ps1`
- `tests/static/test_dev19_image_batch_isolation.py`
- `tests/Converty.Core.Tests/Execution/ImageBatchIsolationTests.cs`
- `.github/workflows/ci.yml`
- `docs/development/DEV19_IMAGE_BATCH_ISOLATION_TDD_EVIDENCE.md`
- `docs/superpowers/specs/2026-09-02-dev19-image-batch-isolation-design.md`
- `docs/superpowers/plans/2026-09-02-dev19-image-batch-isolation.md`

## Required freeze sequence
1. Re-fetch canonical dev.19 and `main`; never assume this document has the live SHA.
2. Finish/verify the pre-authority metadata curation on the exact behavior lineage.
3. Let ordinary CI generate source SBOM, release SBOM, package manifest and SHA256SUMS from the exact curated head.
4. Independently verify the exact authority artifact SHA-256, ZIP CRC, exact four-member set and `0.1.0-dev.19` version alignment.
5. Use a guarded exact-parent temporary sync workflow with contents write permission; it must copy only those four generated files and self-delete.
6. Require canonical dev.19 generated-authority zero-diff and complete Windows managed qualification through deterministic workspace and verified-delivery upload.
7. Re-fetch live `main`; fast-forward non-force only if the expected base is unchanged and the candidate is a strict descendant.
8. Require a fresh exact-main CI run with continuity + supply-chain/static + managed all SUCCESS.
9. Independently verify final exact-main workspace ZIP and verified-delivery artifact; re-read main/dev refs and exact tree.
10. Only then mark dev.19 frozen and create dev.20 from the exact intended frozen/current main.

## Next after freeze
Create `dev/0.1.0-dev.20-video-foundation` from exact frozen main and begin Video foundation test-first. Preserve all Audio 36-case + malformed/truncated + mixed-batch and Image 24-case + malformed/truncated + mixed-batch regression gates.

## Architectural invariants
`IExplorerCommand DLL → fixed app-local Converty.Bridge.exe → strict disposable Converty.EngineWorker.exe / typed provider → fixed app-local FFmpeg → private staging → validated transactional numbered no-overwrite publication`.

Do not add shell command construction, raw FFmpeg argument passthrough, arbitrary converter executable paths, PATH lookup, writable plugin/DLL discovery, ordinary conversion network dependencies, or silent Strict→Compatibility fallback. Preserve Unicode/metacharacter filenames, source files, existing destinations, bounded waits, worker termination, transactional publication and current-user IPC authentication ordering. Signing private keys never enter the repository/workspace.

## Explicitly open launch gates
Headed Windows 11 modern Explorer exact-build UI/screenshots and crash/hang/failure matrix; production signed-package B2 identity/authentication; production FFmpeg/ffprobe redistribution/license/notices/signature/hash approval; signed production MSIX install/update/uninstall; UX/settings; plugin SDK; final fuzz/chaos/security/release/end-user acceptance.

## Recursive handover rule
Every completed tranche must end with a full context-free handover containing live repository/default-branch SHA/tree, prior authority, all tranche commits, RED/GREEN history, run/job/artifact IDs, exact changes and reasons, executed tests/build/security outcomes, workspace hashes/counts, blockers, explicitly unverified claims, one precise next task, architecture/security invariants, headed limitation, production signing/FFmpeg limitation, and this same recursive rule.