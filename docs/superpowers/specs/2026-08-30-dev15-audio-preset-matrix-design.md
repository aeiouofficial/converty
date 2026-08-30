# Dev.15 Audio preset matrix design — 2026-08-30

## Goal
Expand the existing fixed typed Audio actions without creating a generic FFmpeg front end or changing the product trust boundaries.

## Approved shape
Keep `ProductPresetRegistry` as the source of reviewed codec/container arguments. Add three user-facing fixed actions:

- `audio.m4a.aac` — M4A container, AAC 256k, faststart.
- `audio.opus` — Opus container, libopus 192k VBR, `application=audio`.
- `audio.ogg.vorbis` — Ogg container, libvorbis quality 6.

Retain MP3 320k, FLAC and WAV. Native Explorer mirrors the managed IDs and continues to hand only `--preset <id>` plus literal selected paths to Bridge.

## Acceptance
The real product smoke must exercise MP3 plus all three new actions using the same Unicode/metacharacter WAV source. For every action it must prove source preservation, preservation of a pre-existing base destination, numbered publication, no leftover partial output, and ffprobe codec identity. Keep the exact 320000 bit/s assertion for the established MP3 baseline.

## Non-goals
No Image/Video work, no arbitrary codec settings, no raw FFmpeg switches, no alternate process-discovery path, no persistent daemon/session work, and no containment changes.
