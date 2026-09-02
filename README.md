# Converty
<img width="1536" height="1024" alt="Converty Architecture Blueprint" src="https://github.com/user-attachments/assets/985f38c5-5c04-4b45-b69f-5deb0cdcc374" />

Windows 11 modern-context-menu file conversion platform. Converty keeps Explorer, Bridge, disposable worker/provider, media engine, private staging, and transactional publication trust boundaries explicit.

## Workspace version
**0.1.0-dev.19 — frozen.** Exact frozen authority is `main` `eb0ce66dab646427d5bef1548c12e5cc4765b2f1`, tree `337a4e11fb41bab6b6eeb462c3755381580f06c1`, exact-main run `33597504612`.

A later tip of `dev/0.1.0-dev.19-image-batch-isolation` may contain metadata-only handover curation. That descendant is documentation, not a replacement frozen authority and not the dev.20 code base.

## Frozen evidence
Exact-main run `33597504612` closed all required jobs:
- continuity `100143814059`: SUCCESS
- supply-chain/static `100143814189`: SUCCESS, including tracked generated-authority zero-diff
- managed `100143814261`: SUCCESS
- dependency audit: 18 projects / 18 frameworks / 0 vulnerable-result packages
- Release build: 0 warnings / 0 errors
- Audio: 36/36 source-action + repeated malformed/truncated rejection + mixed-batch PASS
- Image: 24/24 source-action + repeated malformed/truncated rejection + mixed-batch PASS
- managed tests: 255/255 PASS
- static tests: 99/99 PASS
- contract vectors: 5/5 PASS

Final deterministic workspace:
- SHA-256 `167b4695cca6810fe0e36e57c45a7bf11483105c0e71b955a948604f2cd9e584`
- 484507 bytes
- 378 ZIP entries
- 376 package-manifest entries
- 377 SHA256SUMS entries
- CRC / deterministic double build / exclusion policy: PASS

Exact-main generated-authority artifact: `9833901138`, digest `sha256:255e9328231e0951368b468308774f1df97dc05a0b05094f8ea4d1f566749ed6`.

Exact-main verified-delivery artifact: `9833955082`, digest `sha256:6e7b7e753a4101a3e690ffe558c96e47f9dba831b858bed5f0005ec144837809`. Independent verification confirmed the exact four outer delivery files, nested CRC/root/version, all 376 package hashes, all 377 SHA-manifest hashes, and zero obvious exclusion-policy violations.

## Product path
`IExplorerCommand → fixed Converty.Bridge.exe → Strict Converty.EngineWorker.exe → typed preset/provider → fixed app-local ffmpeg.exe → private staging → validated no-overwrite numbered publication`

Dev.19 adds a real Windows packaged mixed-Image batch gate without a parallel image subsystem or widened executable/argument surface. One Bridge invocation receives valid PNG → malformed JPG → valid WebP → truncated BMP → valid JPEG. Ordinary per-file conversion failures do not suppress later valid members; aggregate failure is reported after the full selection. Successful outputs use numbered no-overwrite publication, invalid members publish nothing, sources/existing destinations remain byte-identical, partial staging is removed, and test-package converter-worker/FFmpeg processes must not remain.

## Qualification corrections
- Corrected RED `a988204b058ada86f8909cf94e1d9f2b6e69cf39` / run `33595474461` proved that same-extension Image collision setup must never seed a target over the selected source.
- GREEN `633fc39b5df8062496914cc641b7001adea805ee` added the source/target alias guard to the Windows harness while preserving valid PNG→PNG conversion.
- GREEN `8a64d0a12ccf47df5df364a1c6c545f876d57d29` fixed a separate core-test fixture that deleted `first.png` before `RunAsync`; no production execution logic changed.

## Next: dev.20 Video foundation
Before implementation, use Superpowers brainstorming against exact frozen main `eb0ce66dab646427d5bef1548c12e5cc4765b2f1`, present Video design/options to the human, obtain explicit design approval, commit the approved spec, then use writing-plans and TDD. The dev.20 branch must start from the exact frozen main authority—not from metadata-only dev.19 handover curation.

## Still open before customer launch
- Video action/source/malformed/batch qualification
- UX/settings defaults, mixed-selection UX, progress/results, output/concurrency/isolation settings
- plugin SDK manifest/API/signature/hash gate
- production FFmpeg/ffprobe provenance, signatures, hashes, license/notices and redistribution approval
- production signed-package B2 identity/authentication requalification
- signed production MSIX clean Windows 11 install/update/uninstall acceptance
- headed Windows 11 modern Explorer exact-build UI/screenshots and crash/hang/failure matrix
- final fuzz/chaos/security/release audit and headed end-user acceptance

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

Disposable build/test/media/package/log output stays below excluded `artifacts/`, `bin/`, `obj/`, and cache directories. Gyan FFmpeg remains development qualification input only and is not production redistribution approval.