# Implementation status — 0.1.0-dev.19 frozen

## Exact frozen authority
Dev.19 is frozen at exact default-branch authority:
- `main` SHA `eb0ce66dab646427d5bef1548c12e5cc4765b2f1`
- tree `337a4e11fb41bab6b6eeb462c3755381580f06c1`
- exact-main run `33597504612`
- continuity `100143814059`: SUCCESS
- supply-chain/static `100143814189`: SUCCESS
- managed `100143814261`: SUCCESS
- generated-authority artifact `9833901138`, digest `sha256:255e9328231e0951368b468308774f1df97dc05a0b05094f8ea4d1f566749ed6`
- verified-delivery artifact `9833955082`, digest `sha256:6e7b7e753a4101a3e690ffe558c96e47f9dba831b858bed5f0005ec144837809`
- deterministic workspace SHA-256 `167b4695cca6810fe0e36e57c45a7bf11483105c0e71b955a948604f2cd9e584`, 484507 bytes, 378 ZIP entries, 376 package-manifest entries, 377 SHA256SUMS entries
- CRC, deterministic double build, exclusion policy, independent nested hash verification: PASS.

A later commit on `dev/0.1.0-dev.19-image-batch-isolation` may curate this handover metadata. That branch tip is documentation only; it is not a replacement frozen authority. Dev.20 must base from the exact frozen main SHA above unless main has subsequently been intentionally requalified.

## Dev.19 Image mixed-batch closure
- Real Windows packaged Image mixed-batch gate added and wired into CI.
- One Bridge process handles valid PNG → malformed JPG → valid WebP → truncated BMP → valid JPEG.
- Ordinary per-file conversion failures do not suppress later valid members; aggregate failure is returned after the full selection.
- Valid outputs use numbered no-overwrite publication; invalid members publish nothing.
- Source/existing-destination hashes remain unchanged; no `.converty-*.partial.*`; no test-package converter-worker/FFmpeg orphan processes.
- The mixed batch repeats twice.
- Existing Audio and Image single-file acceptance remain regression gates.

## Qualification corrections
Two fixture defects were found test-first and fixed without changing production execution behavior:
1. Same-extension `.png → image.png` collision seeding overwrote its selected source. Corrected RED `a988204b058ada86f8909cf94e1d9f2b6e69cf39` / run `33595474461`; GREEN `633fc39b5df8062496914cc641b7001adea805ee` skips synthetic collision seeding only when target aliases source.
2. The Image core batch test deleted `first.png` before `RunAsync`; production input validation correctly failed. GREEN `8a64d0a12ccf47df5df364a1c6c545f876d57d29` preserves the source and exercises normal `first (1).png` resolver publication.

## Authority closure
- Pre-authority curation: `d88412c9cdac13450806b34db7737be95190e314`.
- Curated CI authority artifact: `9833673969`, independently verified SHA-256 `d14ddad32701283f13dd587b079a151743211c8f5935fa5f1373225600bf6f61`, CRC PASS, exact four generated members, dev.19 version alignment PASS.
- Guarded one-shot sync staging: `fe3b33fe367741c7b748c2ab9863799d86867b55`.
- Self-deleting authority sync: `d5a8e10dd43c742660574dbd9b54848df7d82421`, exact authority tree `337a4e11fb41bab6b6eeb462c3755381580f06c1`.
- Same-tree qualification commit: `eb0ce66dab646427d5bef1548c12e5cc4765b2f1`.
- Branch qualification run `33597141220`: static zero-diff and managed SUCCESS.
- Promotion base `2ecf65d6131568c23e6fb4bbfe2371b6bc978407` remained unchanged; candidate was 29 ahead / 0 behind; main fast-forwarded non-force.
- Exact-main run `33597504612`: all three required jobs SUCCESS.

## Exact-main regression baseline
- 18/18 locked managed restore / 0 vulnerable-result packages
- Release build 0 warnings / 0 errors
- native Explorer, development package, COM and packaged Bridge→Strict Worker/provider→FFmpeg PASS
- Audio 36/36 source-action + repeated malformed/truncated + mixed batch PASS
- Image 24/24 source-action + repeated malformed/truncated + mixed batch PASS
- 255/255 managed PASS
- 99/99 static PASS
- 5/5 contract vectors PASS
- tracked generated-authority zero-diff PASS
- deterministic workspace semantic verification + verified delivery PASS

## Next tranche
Dev.20 Video foundation is next, but Superpowers brainstorming is a hard design gate. Inspect the existing typed registry/provider/native Explorer patterns on exact frozen main, present the Video design/options to the human, obtain explicit approval, commit the approved spec, then invoke writing-plans and implement test-first on `dev/0.1.0-dev.20-video-foundation` created from exact frozen main `eb0ce66dab646427d5bef1548c12e5cc4765b2f1`.

## Still open before customer launch
Video; UX/settings; plugin SDK; production FFmpeg/ffprobe redistribution/provenance/signature/hash/license/notices approval; production signed-package B2 requalification; signed production MSIX install/update/uninstall; headed Windows 11 Explorer exact-build UI/screenshots/crash-hang-failure acceptance; final fuzz/chaos/security/release/end-user acceptance.

Automated CI does not close those headed or production gates.