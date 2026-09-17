# Converty
<img width="1536" height="1024" alt="Converty Architecture Blueprint" src="https://github.com/user-attachments/assets/985f38c5-5c04-4b45-b69f-5deb0cdcc374" />

Windows 11 modern-context-menu file conversion platform. Converty keeps Explorer, Bridge, disposable worker/provider, media engine, private staging, and transactional publication trust boundaries explicit.

## Workspace version
**0.1.0-dev.20** — existing fixed Video actions are behavior-qualified on the development branch; final CI-derived generated-authority synchronization, branch zero-diff, promotion, and exact-main freeze remain open.

## Frozen release authority
`main` remains frozen at dev.19 commit `eb0ce66dab646427d5bef1548c12e5cc4765b2f1`, tree `337a4e11fb41bab6b6eeb462c3755381580f06c1`, exact-main run `33597504612`. Continuity `100143814059`, supply-chain/static `100143814189`, and managed `100143814261` are SUCCESS. That authority includes 255/255 managed tests, 99/99 static tests, 5/5 contract vectors, workspace SHA-256 `167b4695cca6810fe0e36e57c45a7bf11483105c0e71b955a948604f2cd9e584`, generated-authority artifact `9833901138`, and verified-delivery artifact `9833955082`.

## Dev.20 Video qualification
The product path remains:

`IExplorerCommand → fixed Converty.Bridge.exe → Strict Converty.EngineWorker.exe → typed preset/provider → fixed app-local ffmpeg.exe → private staging → validated no-overwrite numbered publication`

Branch `dev/0.1.0-dev.20-video-foundation` qualifies the already-existing Video surface without adding new action IDs or a parallel media subsystem.

Accepted Video sources: `.mp4`, `.mov`, `.mkv`, `.avi`, `.webm`, `.m4v`, `.mpeg`, `.mpg`, `.wmv`.

Fixed actions under qualification:
- `video.mp4.h264` → H.264 + AAC MP4;
- `video.webm.vp9` → VP9 + Opus WebM;
- `extract.audio.mp3` → MP3 audio with no video stream.

Behavior run `33669379940` on `b8019ecf926fce9813fdcd2cbd74e5f59e439d08` proved 27/27 packaged Video source/action conversions, ffprobe codec contracts, repeated malformed and physically truncated rejection, Unicode/metacharacter paths, source/pre-existing-destination byte preservation, same-extension numbered publication, zero partial staging, and a twice-run five-member mixed batch `valid MP4 → malformed AVI → valid MOV → truncated MKV → valid WebM` with aggregate exit 4 and later valid publication. Audio and Image product regressions remained green; 260/260 managed tests and 103/103 static tests passed within that behavior qualification. No production source code change was required.

The behavior-run workspace ZIP was deterministic byte-for-byte (`5db1e06c084e58354aa2445f3645d1b53c760fc63f565fe7960224c2eec2467f`, 505672 bytes, 383 entries) but semantic archive validation correctly stopped on stale tracked generated authority. That is pre-authority evidence, not a final delivery artifact.

Run `33669979101` subsequently generated artifact `9862090305` with SHA-256 `7cb43be8a0a67cf5e0deb19c79827c9df786a26777ddd7f11bdc9cf95d5edba2`, but this repository curation changes workspace bytes, so that artifact is explicitly not eligible for final sync. Final generated authority must come from ordinary CI on the exact curated dev.20 head.

## Current freeze sequence
1. Complete non-generated dev.20 repository curation.
2. Run ordinary CI on the exact curated branch head.
3. Independently verify the generated-authority ZIP digest, CRC, exact four-member set, and dev.20 version alignment.
4. Synchronize only those four generated files through a guarded exact-parent, exact-branch, self-deleting temporary workflow. Never hand-edit generated authority.
5. Require branch generated-authority zero-diff and complete Windows deterministic workspace/delivery qualification.
6. Re-read `main`; fast-forward non-force only if frozen dev.19 `main` is unchanged and the candidate is a strict descendant.
7. Require fresh exact-main continuity + supply-chain/static + managed SUCCESS on the exact promoted SHA.
8. Independently verify exact-main generated authority and verified-delivery artifacts before calling dev.20 frozen.

## Still open before customer launch
- dev.21 B8 Video Copy/Remux/Transcode planner after dev.20 freeze;
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
6. `docs/development/DEV20_VIDEO_QUALIFICATION_TDD_EVIDENCE.md`
7. `docs/superpowers/specs/2026-09-02-dev20-video-qualification-design.md`
8. `docs/superpowers/plans/2026-09-02-dev20-video-qualification.md`
9. `docs/TASK_BACKLOG.md`
10. `docs/Converty_Master_Build_Plan.md`

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
./build/video-input-acceptance-smoke.ps1
./build/video-batch-isolation-smoke.ps1
./build/test.ps1 -Configuration Release
```

Disposable build, test, media, package, and log output stays below excluded `artifacts/`, `bin/`, `obj/`, and cache directories. Development Gyan FFmpeg remains qualification input only and is not production redistribution approval.
