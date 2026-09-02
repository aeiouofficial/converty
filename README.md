# Converty
<img width="1536" height="1024" alt="Converty Architecture Blueprint" src="https://github.com/user-attachments/assets/985f38c5-5c04-4b45-b69f-5deb0cdcc374" />

Windows 11 modern-context-menu file conversion platform. Converty keeps Explorer, Bridge, disposable worker/provider, media engine, private staging, and transactional publication trust boundaries explicit.

## Workspace version
**0.1.0-dev.19** — Image mixed-valid/invalid batch isolation is behavior-qualified and awaiting final CI-derived generated-authority synchronization, branch zero-diff, and exact-main freeze.

## Current evidence-backed state
The product path remains:

`IExplorerCommand → fixed Converty.Bridge.exe → Strict Converty.EngineWorker.exe → typed preset/provider → fixed app-local ffmpeg.exe → private staging → validated no-overwrite numbered publication`

Dev.19 adds one real Windows packaged mixed-Image batch gate without a parallel image subsystem or a widened executable/argument surface. One Bridge invocation receives valid PNG → malformed JPG → valid WebP → truncated BMP → valid JPEG. Ordinary per-file conversion failures do not suppress later valid members; aggregate failure is reported only after the full selection is attempted. Successful outputs use numbered no-overwrite publication, failing members publish nothing, sources and existing destinations remain byte-identical, partial staging is removed, and test-package converter-worker/FFmpeg processes must not remain.

Behavior anchor `8a64d0a12ccf47df5df364a1c6c545f876d57d29`, run `33596229372`: locked restore PASS; dependency audit 18 projects / 18 frameworks / 0 vulnerable-result packages; Release build 0 warnings / 0 errors; native Explorer, development package, COM, Bridge→FFmpeg, Audio 36-case + negatives + mixed batch, Image 24-case + negatives + mixed batch PASS; **255/255 managed tests**, **99/99 static tests**, and **5/5 contract vectors** PASS.

The same run produced two byte-identical pre-authority workspace ZIPs, SHA-256 `1c8d197941a616a25bcc4bab59550037a221309f51bc937a1ba7daa9b34bf97d`, 475353 bytes, 378 entries. Final archive semantic verification and delivery were intentionally blocked because tracked generated authority still contains dev.18 package metadata. That is the remaining authority-sync gate, not a product or determinism success claim.

The frozen dev.18 exact-main qualification anchor remains `ef079f7e7923e399624067c4d54b9ce7577bf090`, tree `0af729f150897d170eac9f9aebfd5bc7d5d4083a`, run `33390111824`.

## Dev.19 qualification corrections
- Windows harness source/target alias RED: same-extension `PNG → image.png` collision setup overwrote its own source before Bridge execution.
- Corrected RED contract requires collision seeding to be skipped when resolved target equals source; same-extension conversion remains valid.
- GREEN harness fix mirrors the already-qualified single-Image acceptance behavior.
- Core-test fixture defect then deleted `first.png` before `RunAsync`; production input validation correctly rejected it.
- GREEN core-test fix preserves the source and lets `OutputPathResolver` naturally publish `first (1).png`.

## Still open before dev.19 freeze
- curate exact pre-authority metadata on the canonical dev.19 branch;
- generate and independently verify the exact four-file CI authority artifact;
- guarded exact-parent generated-authority synchronization;
- branch generated-authority zero-diff qualification;
- non-force fast-forward of unchanged `main` only;
- fresh exact-main continuity + static/supply-chain + Windows managed SUCCESS;
- independent final deterministic workspace and verified-delivery artifact verification.

## Still open before customer launch
- Video action/source/malformed/batch qualification after dev.19 freeze;
- UX/settings defaults, mixed-selection UX, progress/results, output/concurrency/isolation settings;
- plugin SDK manifest/API/signature/hash gate;
- production FFmpeg/ffprobe provenance, signatures, hashes, license/notices, and redistribution approval;
- production signed-package B2 identity/authentication requalification;
- signed production MSIX clean Windows 11 install/update/uninstall acceptance;
- headed Windows 11 modern Explorer exact-build UI/screenshots and crash/hang/failure matrix;
- final fuzz/chaos/security/release audit and headed end-user acceptance.

## Start here
1. `docs/HANDOVER_PROMPT.txt`
2. `docs/HANDOVER_NEXT_AGENT.md`
3. `machine-readable/handover_state.json`
4. `machine-readable/build_evidence.json`
5. `docs/development/IMPLEMENTATION_STATUS.md`
6. `docs/development/DEV19_IMAGE_BATCH_ISOLATION_TDD_EVIDENCE.md`
7. `docs/TASK_BACKLOG.md`
8. `docs/Converty_Master_Build_Plan.md`
9. `docs/SECURITY_THREAT_MODEL.md`
10. `docs/TEST_AND_RELEASE_GATES.md`

## Verification
On Windows with .NET SDK `10.0.400`:
```powershell
./build/bootstrap.ps1
./build/dependency-audit.ps1
./build/build.ps1 -Configuration Release
./build/native-smoke.ps1
./build/prepare-dev-ffmpeg.ps1
./build/stage-dev-package.ps1 -Configuration Release -FfmpegPath ./artifacts/dev-ffmpeg/ffmpeg.exe
./build/validate-dev-package.ps1
./build/explorer-registration-smoke.ps1
./build/product-conversion-smoke.ps1
./build/audio-input-acceptance-smoke.ps1
./build/audio-batch-isolation-smoke.ps1
./build/image-input-acceptance-smoke.ps1
./build/image-batch-isolation-smoke.ps1
./build/test.ps1 -Configuration Release
```

Disposable build, test, media, package, and log output stays below excluded `artifacts/`, `bin/`, `obj/`, and cache directories. Gyan FFmpeg remains development qualification input only and is not production redistribution approval.