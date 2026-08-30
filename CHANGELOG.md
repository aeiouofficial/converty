# Changelog

## 0.1.0-dev.16 — 2026-08-31
- Added a dedicated Windows Audio input acceptance matrix covering WAV, FLAC, MP3, M4A/AAC, Ogg/Vorbis and Opus sources against all six fixed Audio actions (36 real product-path conversions).
- Every matrix conversion enters through packaged `Converty.Bridge.exe` → Strict `Converty.EngineWorker`/typed FFmpeg provider, uses Unicode/metacharacter paths, preserves source and pre-existing destination bytes, uses numbered no-overwrite publication, leaves no partial output and is ffprobe codec-verified.
- Added repeated malformed-WAV and truncated-FLAC negative acceptance. The first RED integration run exposed a real failure-lifecycle hang: Bridge reached its error handler but synchronous `MessageBoxW` blocked noninteractive callers.
- Added explicit automation-only `CONVERTY_BRIDGE_NONINTERACTIVE=1` error reporting. Explorer does not set the variable and retains the normal modal error dialog; automation receives the same bounded error on stderr and the normal nonzero Bridge exit.
- Both malformed and truncated cases now reject deterministically with exit code 4 across repeated attempts, without source mutation, destination overwrite, numbered publication or partial-file residue.
- Preserved TDD RED evidence: `251b1c54901d212e03961e6bed947bc828df6bc7` / run `33339926916` (3 new failures, 81 existing static tests green), and lifecycle RED `673f92e43738554db364a8db5ea44a00cdd903b7` / run `33340234688` (1 new failure, 84 existing tests green).
- GREEN behavior head `061ad75600fee6fd4b34e4a24bd8d571ac17ce90` / run `33340338502`: 36 valid conversions + 2 repeated negative cases PASS, 253/253 managed, 85/85 static, 5/5 vectors, Release 0 warnings/errors; pre-authority deterministic workspace `27b6f96ea8c42afee8de2d67a2ea9d43f48607ab13a4b124cbab6acd3b55a643`, 436519 bytes, 358 entries, then expected stale generated-authority failure.

## 0.1.0-dev.15 — 2026-08-30
- Expanded the fixed typed Audio action matrix with `audio.m4a.aac` (M4A/AAC 256k + faststart), `audio.opus` (libopus 192k VBR, application=audio), and `audio.ogg.vorbis` (libvorbis q6), preserving MP3 320k, FLAC and WAV.
- Mirrored all fixed Audio preset IDs in the native Explorer submenu with stable canonical GUIDs; no raw FFmpeg argument surface or arbitrary executable/path discovery was added.
- Extended the real packaged product smoke from MP3-only to MP3 + M4A/AAC + Opus + Ogg Vorbis with source/destination preservation, numbered publication, no partials and ffprobe validation.
- Frozen exact-main authority: `dc46bd4dd25fe672f1695a0895cdb06152a743a7`, tree `b170701a80377280065ce758f56076aa8eb044f0`, run `33339019327`, deterministic workspace SHA-256 `8f518bf3b70ca0e51f13d554e9a6966bb17f2897dcf249624cd48fe43ee69c52`.

## 0.1.0-dev.14 — 2026-08-30
- Added test-first replay/disconnect/reconnect acceptance for the existing authenticated one-shot Host IPC; no second pipe or persistent-session protocol was introduced.
- Frozen exact-main authority: `e0d9a00c3cb832e8109bf7ba7320215302da2177`, run `33328195635`.

## 0.1.0-dev.13 — 2026-08-30
- Added typed one-shot `status` and `cancel` requests/responses on the existing authenticated Host named pipe while preserving normal Explorer→Bridge→Strict Worker→FFmpeg routing.

## 0.1.0-dev.12 — 2026-08-29
- Eliminated the historical scheduler-dependent containment test assumption without changing production output limits or containment.

## 0.1.0-dev.11 — 2026-08-29
- Closed development B2 connected-server identity/authentication without rerouting normal conversion through Host.

## 0.1.0-dev.10 — 2026-08-27
- Moved conversion execution into fixed app-local `Converty.EngineWorker` and `Converty.Provider.FFmpeg` with private per-job staging and strict containment.

## 0.1.0-dev.9 — 2026-08-26
- Delivered the first automated functional Windows product path: packaged native Explorer command → fixed Bridge → typed preset → fixed app-local FFmpeg → same-folder numbered output.

Earlier foundation history remains available in repository history and prior handovers.
