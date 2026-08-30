# CONVERTY — CONTINUATION HANDOVER
# 0.1.0-dev.15 AUDIO PRESET MATRIX

Repository: `https://github.com/aeiouofficial/converty`
Default branch: `main`
Current workspace: `0.1.0-dev.15`
Next workspace: `0.1.0-dev.16`

## Prior frozen authority
Dev.14 exact main `e0d9a00c3cb832e8109bf7ba7320215302da2177`, tree `ea530b37b25c89cfea8a1d51566e5288c416c389`, exact-main CI `33328195635`. Deterministic workspace SHA-256 `532d1916d33bff2f440e96ab9a3cabc0b0f1898ea5ad2b35bfccb1a9eb63ca44`.

## Dev.15 behavior
Audio actions now include MP3 320k, FLAC, M4A/AAC 256k, Opus 192k VBR, Ogg Vorbis q6, and WAV. Explorer exposes the same typed preset IDs. Conversion continues through the fixed Bridge → Strict Worker/provider → app-local FFmpeg → private staging → numbered no-overwrite publication path.

## Evidence
RED `f729f821c98bc8841e585bad7764d8f7446d1c65`, run `33330926712`: managed 253 total / 249 passed / 4 intended new-feature failures; static 78 existing passes / 3 intended new-feature failures.

GREEN behavior `335754a7d99c99f918fa7f2bc29a89f691f0fd2a`, run `33331186761`: managed 253/253, static 81/81, vectors 5/5, Release 0 warnings/errors, native/package/COM PASS, real MP3 + M4A/AAC + Opus + Ogg Vorbis product matrix PASS. Pre-authority deterministic ZIP SHA-256 `7430cc7aaf3b86a37f84edd6467925d9d9e05d9fa5c27917fafa3f2c813af70d`, 426577 bytes, 353 files; final integrity stopped only on stale generated authority.

## Finality
Do not call dev.15 frozen until generated authority is synchronized from the exact versioned CI artifact, branch zero-diff qualification passes, `main` is fast-forwarded, and an ordinary CI run on exact current `main` has continuity, managed, and supply-chain-static all successful with deterministic verified delivery.

## Next task
After dev.15 freeze, implement `0.1.0-dev.16`: Audio source-format and malformed-input acceptance across the fixed Audio actions. Cover representative supported inputs, malformed/truncated inputs, deterministic failures, source preservation, destination preservation, and partial-output cleanup. Do not begin Image/Video expansion before this Audio tranche is frozen.

## Recursive handover
Every completed tranche must end with a copy-paste handover containing repo/default branch/live SHA/tree, prior authority, relevant commits, RED/GREEN history, run/job/artifact IDs, changes/reasons, tests/build/security outcomes, workspace hashes/counts, blockers, unverified claims, one precise next task, invariants, headed limitation, production signing/FFmpeg limitation, and this same recursive rule.
