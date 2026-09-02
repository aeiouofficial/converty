# Converty continuation handover — dev.20 Video qualification pre-authority closure

Repository: `aeiouofficial/converty`  
Default branch: `main`  
Development branch: `dev/0.1.0-dev.20-video-foundation`

Read `docs/HANDOVER_PROMPT.txt` first. Re-fetch live refs before writes or completion claims.

## Frozen baseline
Dev.19 exact-main authority remains `eb0ce66dab646427d5bef1548c12e5cc4765b2f1`, tree `337a4e11fb41bab6b6eeb462c3755381580f06c1`, run `33597504612`; continuity `100143814059`, static `100143814189`, managed `100143814261` SUCCESS.

## Dev.20 behavior
Approved Design A qualifies the existing nine Video source extensions against the three existing fixed actions; it does not add a new Video subsystem.

RED: `ea6987ddea57de71661b73e9f073cddb1c3f0bd3`, run `33668918551`, static job `100377398869`.

GREEN behavior head: `b8019ecf926fce9813fdcd2cbd74e5f59e439d08`, run `33669379940`.
- Video 27/27 real packaged conversions PASS.
- MP4 ffprobe h264+aac; WebM vp9+opus; MP3 audio-only PASS.
- malformed/truncated Video repeated deterministic exit 4 PASS.
- mixed batch valid MP4 → malformed AVI → valid MOV → truncated MKV → valid WebM repeated twice, aggregate exit 4 after all members, later valids publish PASS.
- sources/pre-existing destinations preserved; numbered publication; no partials/orphan worker/FFmpeg PASS.
- Audio/Image regressions PASS.
- 260/260 managed, 103/103 static within managed qualification, 5/5 vectors PASS.
- no production source-code change required.

Version is now `0.1.0-dev.20`; CI action pin authority has been version-aligned. Pre-curation generated artifact `9862090305` / `sha256:7cb43be8a0a67cf5e0deb19c79827c9df786a26777ddd7f11bdc9cf95d5edba2` is deliberately not final because this curation changes the workspace.

## Required next action
Run ordinary CI on the exact curated dev.20 head, independently verify the newly generated four-file authority artifact, synchronize it only via a guarded exact-parent/self-deleting workflow, then require branch zero-diff + full managed workspace/delivery qualification. Re-read unchanged frozen dev.19 `main` before non-force promotion; after promotion require fresh exact-main three-job SUCCESS and independently verified exact-main artifacts.

## Next tranche after freeze
`0.1.0-dev.21` — current roadmap target: B8 Video Copy/Remux/Transcode planning/qualification. Reconcile live Roadmap/Plan/Tasks before starting it.

## Invariants / still open
`IExplorerCommand DLL → fixed app-local Bridge → strict disposable EngineWorker/provider → fixed app-local FFmpeg → private staging → validated transactional numbered no-overwrite publication`.

No shell construction, raw FFmpeg passthrough, PATH lookup, arbitrary converter/plugin discovery, ordinary conversion network dependency, silent Strict→Compatibility fallback, or repository signing keys. Headed Windows 11 Explorer acceptance, production FFmpeg redistribution approval, production signed-package B2, signed MSIX lifecycle, UX/settings, plugin SDK, final fuzz/chaos/security/release/end-user gates remain OPEN.

## Recursive handover rule
At every completed work block, reconcile GitHub authority with Slack/Drive, update canonical docs in place, mark the current OPEN handover PROCESSED with successor reference, then publish exactly one context-free successor OPEN containing current authority/evidence, completed work, blockers/unverified items, invariants, exact next task and acceptance criteria.
