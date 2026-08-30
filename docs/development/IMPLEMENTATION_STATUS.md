# Implementation status — 0.1.0-dev.15

## Dev.15 fixed typed Audio preset/action matrix — 2026-08-30
- Added fixed `audio.m4a.aac`, `audio.opus`, and `audio.ogg.vorbis` presets alongside existing MP3/FLAC/WAV actions.
- M4A/AAC is fixed at 256k with faststart; Opus uses libopus 192k VBR/application=audio; Ogg Vorbis uses libvorbis quality 6. No user-controlled codec arguments were introduced.
- Native `IExplorerCommand` submenu now exposes the same stable typed IDs with stable canonical GUIDs. Identity-output actions remain hidden by the existing applicability rule.
- `build/product-conversion-smoke.ps1` now exercises MP3 plus all three new targets through the staged packaged Bridge→Strict Worker→FFmpeg route using one Unicode/metacharacter WAV source.
- For every exercised target, the source remains byte-identical, a pre-existing base destination remains byte-identical, numbered `(1)` output is created, no `.partial` file remains, and ffprobe confirms the expected codec. MP3 remains exactly 320000 bit/s.
- Host/Bridge media neutrality, strict worker containment, private staging, typed preset resolution and no-overwrite publication remain unchanged.

## TDD evidence
- RED head `f729f821c98bc8841e585bad7764d8f7446d1c65`, run `33330926712`: managed `99309222229` had 253 total / 249 passed / exactly 4 new preset expectation failures; static `99309222365` had 78 existing passes and exactly 3 new dev.15 failures.
- Managed registry implementation `3b68e76f02b8e502ec0e37737ac366627336b41d`.
- Real product-matrix implementation `75276340d71a79ee21053506d6624ef5a1494095`.
- Native Explorer action implementation `8f5ffccb31b8fa17205fcc29ffccb327fd8df33d`.
- Static boundary correction `ba3e7930ef3c9754f181263992239f695cf0f01f` and structured legacy smoke assertion correction `335754a7d99c99f918fa7f2bc29a89f691f0fd2a`.
- GREEN behavior head `335754a7d99c99f918fa7f2bc29a89f691f0fd2a`, run `33331186761`, managed `99309902055`, static `99309901887`.

## Observed behavior qualification
- Windows Server 2025 / .NET SDK 10.0.400.
- 18/18 locked restore; dependency audit PASS across 18 projects/18 frameworks with 0 vulnerable-result packages.
- Release build PASS with 0 warnings / 0 errors.
- Native Explorer, unsigned development package/MakeAppx, direct class-factory Invoke and loose-package COM activation/Invoke PASS.
- Real product matrix PASS: MP3, AAC-in-M4A, Opus and Vorbis-in-Ogg through Bridge→Strict Worker→FFmpeg; Unicode/metacharacter source and existing destinations preserved; numbered publication and no-partial cleanup PASS.
- Managed tests 253/253 PASS, 0 skipped; Python static tests 81/81 PASS; contract vectors 5/5 PASS.
- Pre-authority deterministic A/B workspace SHA-256 `7430cc7aaf3b86a37f84edd6467925d9d9e05d9fa5c27917fafa3f2c813af70d`, 426577 bytes, 353 files; integrity then failed only because tracked generated authority predates `build/product-conversion-smoke.ps1`.

## Prior frozen authority
Dev.14 exact main `e0d9a00c3cb832e8109bf7ba7320215302da2177`, tree `ea530b37b25c89cfea8a1d51566e5288c416c389`, run `33328195635`; deterministic workspace SHA-256 `532d1916d33bff2f440e96ab9a3cabc0b0f1898ea5ad2b35bfccb1a9eb63ca44`.

## Authority rule
Do not infer dev.15 finality from this document. Dev.15 is frozen only after version-aligned generated authority is synchronized from one exact CI artifact, a branch qualification run reaches generated-authority zero-diff with managed/static green, `main` is fast-forwarded, and ordinary CI on the exact current `main` has continuity + managed + supply-chain-static all SUCCESS with deterministic workspace verification and verified delivery upload.

## Remaining shipping gates
Headed Win11 UI/screenshots and Explorer failure matrix; broad Audio source-format/malformed-input acceptance; production signed-package B2 requalification; production FFmpeg redistribution approval; signed MSIX/clean-VM lifecycle; final security/fuzz/chaos/release/end-user acceptance.
