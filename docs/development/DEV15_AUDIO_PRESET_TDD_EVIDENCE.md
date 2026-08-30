# Dev.15 Audio preset matrix TDD evidence

## RED
Head `f729f821c98bc8841e585bad7764d8f7446d1c65`, run `33330926712`.

- managed `99309222229`: Release/native/package/COM/legacy MP3 product smoke passed; 253 managed tests ran, 249 passed and exactly four new preset expectations failed because M4A/AAC, Opus and Ogg Vorbis did not yet exist.
- static `99309222365`: 78 existing tests passed; exactly three new dev.15 assertions failed for the absent managed/native/product-matrix feature.
- continuity `99309222527`: expected side-branch failure.

## Implementation
- registry `3b68e76f02b8e502ec0e37737ac366627336b41d`
- real product matrix `75276340d71a79ee21053506d6624ef5a1494095`
- native Explorer submenu `8f5ffccb31b8fa17205fcc29ffccb327fd8df33d`
- static boundary correction `ba3e7930ef3c9754f181263992239f695cf0f01f`
- structured legacy smoke assertion correction / GREEN behavior head `335754a7d99c99f918fa7f2bc29a89f691f0fd2a`

## GREEN behavior
Run `33331186761`, managed `99309902055`, static `99309901887`.

- 18/18 locked restore PASS; 18 projects / 18 frameworks / 0 vulnerable-result packages.
- Release build PASS, 0 warnings, 0 errors.
- native Explorer, unsigned package/MakeAppx, direct shell DLL Invoke, registered package COM activation/Invoke PASS.
- real packaged Bridge→Strict Worker→FFmpeg matrix PASS for `audio.mp3`, `audio.m4a.aac`, `audio.opus`, `audio.ogg.vorbis`.
- source and pre-existing destinations preserved for every target; numbered `(1)` outputs; no partial output; ffprobe codec identity PASS; MP3 exactly 320000 bit/s.
- managed 253/253 PASS; static 81/81 PASS; vectors 5/5 PASS.
- deterministic pre-authority A/B ZIP SHA-256 `7430cc7aaf3b86a37f84edd6467925d9d9e05d9fa5c27917fafa3f2c813af70d`, 426577 bytes, 353 files.
- workspace integrity then failed only on stale tracked generated authority at `build/product-conversion-smoke.ps1`, as expected before versioned authority synchronization.
