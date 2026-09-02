# Converty continuation handover — dev.19 frozen authority

Repository: `https://github.com/aeiouofficial/converty`  
Default branch: `main`

Read `docs/HANDOVER_PROMPT.txt` first. Re-fetch live refs before every write or completion claim.

## Exact frozen dev.19 authority
- Main SHA: `eb0ce66dab646427d5bef1548c12e5cc4765b2f1`
- Tree: `337a4e11fb41bab6b6eeb462c3755381580f06c1`
- Version: `0.1.0-dev.19`
- Exact-main run: `33597504612`
- Continuity `100143814059`: SUCCESS
- Static `100143814189`: SUCCESS
- Managed `100143814261`: SUCCESS
- Generated-authority artifact: `9833901138`, digest `sha256:255e9328231e0951368b468308774f1df97dc05a0b05094f8ea4d1f566749ed6`
- Verified-delivery artifact: `9833955082`, digest `sha256:6e7b7e753a4101a3e690ffe558c96e47f9dba831b858bed5f0005ec144837809`
- Deterministic workspace: SHA-256 `167b4695cca6810fe0e36e57c45a7bf11483105c0e71b955a948604f2cd9e584`, 484507 bytes, 378 entries
- Package manifest: 376 entries
- SHA256SUMS: 377 entries
- CRC / deterministic double build / exclusion policy: PASS
- Independent final delivery verification: PASS; exact four outer files, nested CRC/root/version PASS, 0 package-hash failures, 0 SHA-manifest failures, 0 obvious exclusion violations.

Immediately after qualification both live `main` and `dev/0.1.0-dev.19-image-batch-isolation` pointed to the exact frozen SHA/tree above. This handover may be committed later as a metadata-only descendant on the dev.19 branch. Such a descendant is documentation only and is **not** the dev.20 base. Dev.20 must start from the exact frozen main authority unless a later tranche has intentionally and fully qualified a new main.

## Dev.19 closure
Dev.19 proves same-family Image mixed-batch failure isolation through the packaged product path. One Bridge process receives valid PNG → malformed JPG → valid WebP → truncated BMP → valid JPEG; ordinary per-file conversion failures do not suppress later valid members; aggregate failure returns only after the selection; successful outputs use numbered no-overwrite publication; invalid members publish nothing; source/existing-destination bytes remain unchanged; partials and test-package worker/FFmpeg processes are cleaned. The batch is repeated twice.

Qualification also corrected two test/harness defects without changing production execution behavior:
1. Same-extension `.png → image.png` collision setup overwrote its own source. Corrected RED `a988204b058ada86f8909cf94e1d9f2b6e69cf39` / run `33595474461`; GREEN `633fc39b5df8062496914cc641b7001adea805ee`.
2. The core test deleted `first.png` before `RunAsync`; product validation correctly rejected it. GREEN `8a64d0a12ccf47df5df364a1c6c545f876d57d29` preserved the source and exercised normal `first (1).png` resolver publication.

Authority closure used pre-authority curation `d88412c9cdac13450806b34db7737be95190e314`, independently verified exact four-file artifact `9833673969` / SHA-256 `d14ddad32701283f13dd587b079a151743211c8f5935fa5f1373225600bf6f61`, guarded temporary sync staging `fe3b33fe367741c7b748c2ab9863799d86867b55`, self-deleting bot sync `d5a8e10dd43c742660574dbd9b54848df7d82421`, then same-tree qualification `eb0ce66dab646427d5bef1548c12e5cc4765b2f1`.

Before promotion, live main was still exactly `2ecf65d6131568c23e6fb4bbfe2371b6bc978407`; compare showed dev.19 29 ahead / 0 behind. Main was fast-forwarded non-force. Exact-main run `33597504612` then closed all three required jobs.

## Exact-main regression evidence
- Dependency audit: 18 projects / 18 frameworks / 0 vulnerable-result packages
- Release build: 0 warnings / 0 errors
- Native Explorer/package/COM/product path: PASS
- Audio: 36/36 source-action + repeated malformed/truncated rejection + mixed batch PASS
- Image: 24/24 source-action + repeated malformed/truncated rejection + mixed batch PASS
- Managed: 255/255 PASS
- Static: 99/99 PASS
- Raw contract vectors: 5/5 PASS
- Generated-authority zero-diff: PASS
- Deterministic workspace semantic verification and verified-delivery upload: PASS

## One precise next task
Use Superpowers brainstorming for **dev.20 Video foundation** from the exact frozen main SHA `eb0ce66dab646427d5bef1548c12e5cc4765b2f1`. Treat it as an architectural feature tranche: inspect existing typed registry/provider/native Explorer patterns, present design options and the recommended design to the human, obtain explicit approval, commit the approved spec, then invoke writing-plans and implement test-first on `dev/0.1.0-dev.20-video-foundation` created from the exact frozen main—not from this metadata-only handover descendant.

The Video tranche must keep fixed typed actions, no raw FFmpeg surface, representative source × action acceptance, malformed/truncated deterministic rejection, mixed-valid/invalid Video batch isolation, Unicode/metacharacter/collision preservation, bounded waits and cleanup, packaged Bridge → Strict Worker/provider → app-local FFmpeg proof, and every existing Audio/Image regression gate.

## Architectural invariants
`IExplorerCommand DLL → fixed app-local Converty.Bridge.exe → strict disposable Converty.EngineWorker.exe / typed provider → fixed app-local FFmpeg → private staging → validated transactional numbered no-overwrite publication`.

No shell command construction, raw FFmpeg argument passthrough, arbitrary converter executable paths, PATH lookup, writable plugin/DLL discovery, ordinary conversion network dependency, or silent Strict→Compatibility fallback. Preserve Unicode/metacharacter filenames, source/existing destination bytes, bounded waits, worker termination, transactional publication and current-user IPC authentication. No private signing material in repo/workspace.

## Still open before customer launch
Video; UX/settings; plugin SDK; production FFmpeg/ffprobe provenance/signature/hash/license/notices/redistribution approval; production signed-package B2 requalification; signed production MSIX lifecycle; headed Windows 11 Explorer exact-build UI/screenshots/crash-hang-failure acceptance; final fuzz/chaos/security/release/end-user acceptance.

Automated CI does **not** close those headed/production gates.

## Recursive handover rule
Every completed tranche ends with a context-free handover containing live repo/default-branch SHA/tree, prior authority, tranche commits, RED/GREEN history, run/job/artifact IDs, exact changes/reasons, executed tests/build/security results, workspace hashes/counts, blockers, explicitly unverified claims, one precise next task, architecture/security invariants, headed limitation, production signing/FFmpeg limitation, and this same recursive rule.