# Changelog

## 0.1.0-dev.22 — 2026-09-24 — pinned local packaged regression closure
- Recovered and SHA-256/size-verified the exact development-only FFmpeg/ffprobe archive, then rebuilt the unsigned development MSIX layout with both pinned tools. Packaged ProbeWorker, direct staged Explorer COM invocation, product smoke, Audio36, Image24, Video27, negative and repeated mixed-batch regressions, Copy/Remux/Transcode all PASS locally on the amended source subject `6292ccb`.
- Registered unsigned package COM is BLOCKED on this laptop by Windows `Add-AppxPackage` error `0x80073CFF` (missing permitted sideload/developer policy); this is not a production signed install or headed Explorer acceptance. Local test logs and exact evidence in `docs/development/DEV22_LOCAL_CI_EVIDENCE_2026-09-24.md`.
- No GitHub Actions were dispatched. Frozen main and historical qualified development evidence remain immutable; external shipping issue #13 and independent PR #14 review remain OPEN.


## 0.1.0-dev.22 — 2026-09-24 — local qualification and portability fixes (development only)
- On the non-elevated Windows 11 laptop, two reparse-point regression fixtures initially failed because directory symlink creation required a privilege the test process did not have. The test fixtures now use directory junctions, verify the reparse attribute, and assert the original security rejection; no production containment behavior was changed. Local Release build 0 warnings/errors; 395/395 managed tests PASS; native MSVC Explorer DLL PASS.
- Reproduced a platform-dependent deterministic-release defect: Python release-SBOM generation on Windows emitted CRLF and changed otherwise identical committed artifact bytes. A four-member newline regression first failed 1/4, then passed 4/4 after explicit LF writes in the source/release SBOM, package-manifest and SHA256SUMS generators. Full static suite 168/168, 5/5 contract vectors and 19-project dependency audit (0 vulnerable-result packages) PASS.
- Unsigned development MSIX layout/schema PASS without the converter. The exact pinned development FFmpeg network download failed its SHA-256 and expected byte count; the payload was rejected before extraction and packaged execution gates remain NOT VERIFIED for this amended tree. A pre-commit workspace ZIP correctly failed current-authority comparison because the package tool intentionally reads Git HEAD; final deterministic verification must be after commit.
- Local evidence and remaining gates: `docs/development/DEV22_LOCAL_CI_EVIDENCE_2026-09-24.md`. All project scratch and SDK 10.0.400 remain under `D:\converty`; no GitHub Actions were started. This new local code/docs tree is not the earlier GitHub-qualified subject and is not a customer release. Actual production approval, signed package and clean headed acceptance remain OPEN.

## 0.1.0-dev.22 — 2026-09-24 — exact candidate qualified; local storage contract
- Exact development subject `1aea099a2878f72f8c95a5cd3bd1dee98a1f98b3`, tree `8369b929f3e8b48f76f720ae3a954818acf2a25a`, CI `35937520600`: Managed 395/395 PASS, Static 162/162 PASS, vectors 5/5 PASS, tracked generated authority zero-diff PASS; all real packaged Audio/Image/Video and negative/mixed regressions PASS.
- Generated authority artifact `10783376753` (SHA-256 `c3b79e9db86c52585c9c3db00d05504d43a0b6991f6c323c9f6cdf302eb1df59`); verified delivery `10783368416` (SHA-256 `2697a85aa3b2a5f51f1446afb37e348bb242177dc29b11dbf3b8416b422ba207`). Independent nested archive verification: SHA-256 `337bdb018274866803e8b6e15f7b9ea0fad4436ac3d2b912d61f340cfc76cf47`, 646844 bytes, 462 files, CRC/460 manifest/461 SHA256SUMS PASS.
- Local laptop storage policy: relocated two old Converty C: temp directories (13 files; hashes compared before original deletion) to `D:\converty\_temp\migrated-from-C`; created D:-local cache/temp bootstrap and repository agent contract/test. No Converty project scratch is authorized on the user's local C:.
- Opened [draft PR #14](https://github.com/aeiouofficial/converty/pull/14) for independent review and [shipping issue #13](https://github.com/aeiouofficial/converty/issues/13) for genuine production approvals, live main governance and headed Windows 11 acceptance.
- External approvals remain OPEN; `main` and dev.21 stay frozen. This follow-up documentation/storage-policy change itself requires fresh generated authority and exact-head CI before claiming final PR-head qualification.


## 0.1.0-dev.22 — 2026-09-21 — audit remediation / shipping hardening in progress
- Tasks 1-7 of the dev.22 remediation plan are implementation/evidence complete: exact-subject/artifact release evidence, live governance/readiness checks, production engine/package/acceptance fail-closed boundaries, content secret scanning, structured partial-batch results, destination-volume durable publication and owned stale-staging recovery.
- Pre-authority behavior head `7e50fa9a1df4d0a8336d16596af488335424b792` / tree `a3a6deec5d753c6176fb40368a56d263b3beaa62`; CI `35664296931` proves 395/395 managed, 162/162 static, 5/5 vectors, dependency audit 0 vulnerable-result packages, Release build 0 warnings/errors and all Native/package/ProbeWorker/COM/Audio/Image/Video/Copy/Remux/Transcode gates PASS.
- Development FFmpeg qualification was repinned after the historical Gyan URL returned HTTP 404. The replacement is BtbN tag `autobuild-2026-09-21-13-55`, asset `579145936`, source `a5923073bf`, SHA-256 `fe372180f20e7f9bfa3d9a481b2b1b98c8296178d8265552608736637ea6b3c8`; it is explicitly development-only.
- Deterministic pre-authority workspace built twice byte-identically at SHA-256 `526588af945a4eeb04984dc17792af8dd10fc1d7f2a9e9006ab3976d448b2b0a`, 642060 bytes / 462 files, then correctly stopped on stale tracked generated authority.
- Task 8 now curates non-generated `0.1.0-dev.22` authority. The four generated SBOM/package/hash files remain CI-derived and untouched.
- Frozen `main` remains `8a1f46603aa842728247bc11b34fcccf121858fd`; qualified dev.21 candidate `c3bc042aea3154e30ce2720db4722cbda27bcb34` remains immutable and unpromoted.
- Converty remains **NOT CUSTOMER SHIP-READY** because live governance and external production signing/provenance/headed/final-acceptance gates remain OPEN.

## 0.1.0-dev.21 — 2026-09-18 — development / pre-authority
- Completed the dev.21 Tasks 1–11 implementation line under the approved security design and committed plan: bounded probe contracts/serialization, streaming stdout limits and read-only probe scope, strict ProbeWorker/ffprobe boundary, provider-only FFmpeg token ownership, deterministic Video planning, explicit mode-aware EngineWorker execution, managed byte-exact Copy with SHA-256 proof, post-probe TargetMediaContract publication gating, actual ffprobe/ffmpeg descendant containment, packaged Copy/Remux/Transcode qualification, and CI governance/supply-chain hardening.
- Current pre-authority engineering head `277f6c30f5fd22b3107604304717e56839e76641` / tree `b8c1f9ea203e532e235a94174740bb23821c7d86`; Task-11 run `35299376030` proves 392/392 managed, 132/132 static, 5/5 vectors, 19/19 dependency audit with zero vulnerable-result packages, Release 0 warnings/errors, Audio36/Image24/Video27 + negatives/mixed, Task9 containment and Task10 mode qualification.
- Permanent CI now installs Python static dependencies from a committed SHA-256 hash lock under `--require-hashes --only-binary=:all:`; external Actions remain exact full-SHA pinned and permanent workflow permission remains `contents: read`.
- Task-11 generated-authority artifact `10529710374` / `sha256:0029b1def03e329572ef221edc8779c89158a695b0be26ce02a05487e01e99ec` independently passes ZIP digest, CRC and exact four-member verification, but is still versioned `0.1.0-dev.20`. It is retained as Task-12 input only and is not eligible as final dev.21 authority.
- Task 12 starts with non-generated dev.21 authority/evidence curation. The four generated authority files remain untouched until ordinary CI regenerates them from the exact curated head.
- Frozen release authority remains `0.1.0-dev.20` on exact main `8a1f46603aa842728247bc11b34fcccf121858fd`; no dev.21 freeze/release claim is made.
- Dev.21 was subsequently stabilized and qualified as immutable candidate `c3bc042aea3154e30ce2720db4722cbda27bcb34` / tree `47ecb6f1f952fcdf286e60dd81935a0d81af56ee`; promotion remained intentionally blocked by live governance and external production release prerequisites.

## 0.1.0-dev.19 — 2026-09-02
- Added focused Image multi-file failure-isolation coverage to prove later valid selections survive ordinary malformed/truncated members.
- Added a real packaged Windows Image mixed-batch acceptance smoke using one Bridge process for valid PNG → malformed JPG → valid WebP → truncated BMP → valid JPEG, repeated twice.
- The acceptance contract requires aggregate Bridge exit code 4 after the full selection, later valid outputs to publish with numbered no-overwrite semantics, invalid members to publish nothing, sources and pre-existing destinations to remain byte-identical, no `.converty-*.partial.*` residue and no test-package converter-worker/FFmpeg orphan processes.
- Added the dev.19 managed CI gate and static contract; existing Audio and Image single-file acceptance gates remain mandatory regression gates.
- Qualification exposed a same-extension harness defect: collision setup treated `first.png → image.png` as a separate destination and overwrote the selected source. Corrected RED `a988204b058ada86f8909cf94e1d9f2b6e69cf39` / run `33595474461` failed exactly on the missing source/target guard; GREEN `633fc39b5df8062496914cc641b7001adea805ee` skips synthetic collision seeding only when target aliases source, matching the already-qualified single-Image gate.
- The following managed run exposed an independent core-test fixture defect: the test deleted `first.png` before `RunAsync`, so production input validation correctly raised `FileNotFoundException`. GREEN `8a64d0a12ccf47df5df364a1c6c545f876d57d29` preserves the source and exercises normal `OutputPathResolver` publication to `first (1).png`; no production execution logic changed.
- Pre-authority behavior run `33596229372` on `8a64d0a12ccf47df5df364a1c6c545f876d57d29` passed 255/255 managed tests, 99/99 static tests, 5/5 contract vectors, dependency audit with 0 vulnerable-result packages, Release build with 0 warnings/errors, all recursive Audio gates, 24/24 Image single-file conversions plus negatives, and the Image mixed batch twice.
- That run built the workspace ZIP twice byte-identically at SHA-256 `1c8d197941a616a25bcc4bab59550037a221309f51bc937a1ba7daa9b34bf97d`, 475353 bytes, 378 entries. Archive semantic verification then correctly stopped on the still-tracked dev.18 package manifest; final generated-authority synchronization, branch zero-diff, exact-main qualification and verified delivery remain open.
- Preserved the full granular historical changelog and machine-readable evidence vocabulary; generated SBOM/package/hash authority remains CI-derived and must not be hand-edited.

## 0.1.0-dev.18 — 2026-08-31
- Added a dedicated packaged Windows Image acceptance component for the already-existing fixed Image product surface; no second image engine, raw FFmpeg argument surface or new executable path was introduced.
- Qualified all advertised Image source extensions (`png`, `jpg`, `jpeg`, `webp`, `bmp`, `gif`, `tif`, `tiff`) against `image.png`, `image.jpeg`, and `image.webp`: 24 real Bridge→Strict Worker/provider→FFmpeg conversions.
- Every successful conversion verifies expected ffprobe codec and 64×48 dimensions, preserves the source and a pre-existing destination byte-for-byte, uses numbered no-overwrite publication, and leaves no partial output.
- Added repeated malformed and physically truncated Image rejection; both return deterministic Bridge exit code 4, publish nothing, preserve existing files and leave zero partial residue.
- RED `7388aec6ffb673e0101b09106d646d417f77a7b3` / run `33349908668`: 92 existing static tests PASS and exactly 3 new dev.18 assertions FAIL because the Image acceptance component did not exist.
- Added the Image matrix at `0841395904960945a1988dcedb8b6ccf352a57e0` and ordinary CI wiring at behavior head `6075aa3973b75e170cb5f9b812a8ca3b9b71f528`.
- GREEN behavior run `33350141373`: 24/24 Image conversions plus both repeated negative cases PASS; existing Audio product/matrix/batch gates PASS; 254/254 managed, 95/95 static, 5/5 vectors, dependency audit 0 vulnerable-result packages, Release build 0 warnings/errors. Pre-authority deterministic workspace `50324f542c263cb7b23f43a6e9c87b68ec773f08c1a7b828dc41da7e369cdda2`, 459142 bytes, 369 entries, then expected stale tracked-authority failure.

## 0.1.0-dev.17 — 2026-08-31
- Closed Audio mixed-valid/invalid multi-file failure isolation without adding a second batch subsystem or changing the native one-Bridge-per-selection topology.
- `ConversionBatchRunner` now catches ordinary per-file `ConversionFailedException`, always cleans that file's private staging, continues later selected files, and rethrows the first media conversion failure only after the batch has been attempted. Cancellation, contract/programmer faults and global infrastructure errors remain fail-fast.
- Added a real packaged Windows batch smoke using one Bridge process for valid WAV → malformed WAV → valid FLAC → truncated FLAC → valid WAV. The five-file selection is executed twice and must return aggregate exit code 4 while all three valid files publish numbered MP3 outputs and both bad inputs publish nothing.
- The gate proves source and pre-existing destination hashes are preserved for every item, successful outputs are ffprobe-verified, collision numbering advances from `(1)` to `(2)`, and no `.converty-*.partial.*` files remain.
- Preserved RED evidence at `053e086fab6fcea1da83ab109e1a986379e0b82a` / run `33346968020`: existing product gates green, 254 managed tests with exactly one new failure because only two of three worker calls occurred. Added the independent static product-gate RED at `285585107795045a41d85199c22fd971b1ed6191` / run `33346976504`.
- Fixed two acceptance-harness defects test-first: unsafe `$attempt:` interpolation (`fe2886897dc03eec3942c046973e04558acaf860` → `355e5fdc47ad6d7090678a8b32461fb177a0db63`) and PowerShell inline array-concatenation binding (`6fd23d346ddf5b2acecc34fef5974b559df31289` → `5829c868c5d192c70f21ea0da9337250a8d9c961`).
- GREEN behavior run `33347652162`: real mixed batch PASS twice, 254/254 managed, 91/91 static, 5/5 vectors, Release 0 warnings/errors; pre-authority deterministic workspace `4af24ae6f866c6389a3010642504aea13952ecb17d717c9974d05161fb8f6ba0`, 447903 bytes, 364 entries, then expected stale generated-authority failure.

## 0.1.0-dev.16 — 2026-08-31
- Added a dedicated Windows Audio input acceptance matrix covering WAV, FLAC, MP3, M4A/AAC, Ogg/Vorbis and Opus sources against all six fixed Audio actions (36 real product-path conversions).
- Every matrix conversion enters through packaged `Converty.Bridge.exe` → Strict `Converty.EngineWorker`/typed FFmpeg provider, uses Unicode/metacharacter paths, preserves source and pre-existing destination bytes, uses numbered no-overwrite publication, leaves no partial output and is ffprobe codec-verified.
- Added repeated malformed-WAV and truncated-FLAC negative acceptance. The first RED integration run exposed a real failure-lifecycle hang: Bridge reached its error handler but synchronous `MessageBoxW` blocked noninteractive callers.
- Added explicit automation-only `CONVERTY_BRIDGE_NONINTERACTIVE=1` error reporting. Explorer does not set the variable and retains the normal modal error dialog; automation receives the same bounded error on stderr and the normal nonzero Bridge exit.
- Both malformed and truncated cases now reject deterministically with exit code 4 across repeated attempts, without source mutation, destination overwrite, numbered publication or partial-file residue.
- Preserved TDD RED evidence: `251b1c54901d212e03961e6bed947bc828df6bc7` / run `33339926916` (3 new failures, 81 existing static tests green), and lifecycle RED `673f92e43738554db364a8db5ea44a00cdd903b7` / run `33340234688` (1 new failure, 84 existing tests green).
- GREEN behavior head `061ad75600fee6fd4b34e4a24bd8d571ac17ce90` / run `33340338502`: 36 valid conversions + 2 repeated negative cases PASS, 253/253 managed, 85/85 static, 5/5 vectors, Release 0 warnings/errors; pre-authority deterministic workspace `27b6f96ea8c42afee8de2d67a2ea9d43f48607ab13a4b124cbab6acd3b55a643`, 436519 bytes, 358 entries, then expected stale generated-authority failure.

## 0.1.0-dev.15 — 2026-08-30
- Expanded the fixed typed Audio action matrix with `audio.m4a.aac`, `audio.opus`, and `audio.ogg.vorbis`, preserving MP3, FLAC and WAV.
- Mirrored all fixed Audio preset IDs in the native Explorer submenu with stable canonical GUIDs and qualified MP3/M4A-AAC/Opus/Ogg-Vorbis through the packaged product path.

## 0.1.0-dev.14 — 2026-08-30
- Added replay/disconnect/reconnect acceptance for authenticated one-shot Host IPC and idempotent admission by `requestId`.

## 0.1.0-dev.13 — 2026-08-30
- Added typed one-shot `status` and `cancel` requests/responses on the authenticated Host named pipe.

## 0.1.0-dev.12 — 2026-08-29
- Eliminated the historical scheduler-dependent containment test assumption without changing production output limits or containment.

## 0.1.0-dev.11 — 2026-08-29
- Closed development B2 connected-server identity/authentication without rerouting normal conversion through Host.

## 0.1.0-dev.10 — 2026-08-27
- Moved conversion execution into fixed app-local `Converty.EngineWorker` and `Converty.Provider.FFmpeg` with private per-job staging and strict containment.

## 0.1.0-dev.9 — 2026-08-26
- Delivered the first automated functional Windows product path: packaged native Explorer command → fixed Bridge → typed preset → fixed app-local FFmpeg → same-folder numbered output.

Earlier foundation history remains available in repository history and prior handovers.
