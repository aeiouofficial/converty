# Implementation status — 0.1.0-dev.17

## Dev.17 Audio batch failure isolation — 2026-08-31
- Kept the native same-family multi-selection topology: one `Converty.Bridge.exe` process receives the selected paths and one fixed typed preset ID.
- Fixed `ConversionBatchRunner` so an ordinary per-file `ConversionFailedException` no longer aborts before later selected files. Each file retains its own private staging/cleanup; successful files already published are retained; the first conversion failure is rethrown after all items have been attempted.
- Cancellation, programmer/contract faults and global infrastructure errors remain fail-fast. No second batch service, IPC protocol or media execution path was introduced.
- Added `build/audio-batch-isolation-smoke.ps1`, wired into ordinary Windows CI. It executes one five-file selection twice: valid WAV, malformed WAV, valid FLAC, truncated FLAC, valid WAV.
- Each attempt requires aggregate Bridge exit code 4 after processing, three ffprobe-verified numbered MP3 successes, no output for invalid inputs, byte-identical sources and pre-existing destinations, `(1)` then `(2)` collision numbering, bounded wait and zero partial residue.

## TDD evidence
- RED managed `053e086fab6fcea1da83ab109e1a986379e0b82a`, run `33346968020`, managed `99352825485`: all earlier gates green; 254 managed total with exactly one new failure, worker calls expected 3 / actual 2.
- RED static product contract `285585107795045a41d85199c22fd971b1ed6191`, run `33346976504`: dedicated mixed-batch product smoke/CI wiring absent as expected.
- Production fix `dc48000696429df5f1d2c57e4a42310d8345c541`; acceptance component `b50601f5836cc4d0a6962b3423693f40c1f02310`; CI wiring `6ee217e63de006bea31f4047d35a01e2de912721`.
- The first Windows integration exposed a PowerShell parser defect in the acceptance logger. RED `fe2886897dc03eec3942c046973e04558acaf860`, fixed by `355e5fdc47ad6d7090678a8b32461fb177a0db63` using `${attempt}:`.
- The next integration exposed PowerShell parameter binding of inline array concatenation before Bridge startup. RED `6fd23d346ddf5b2acecc34fef5974b559df31289`, fixed at behavior head `5829c868c5d192c70f21ea0da9337250a8d9c961` by constructing `$fixtureArguments` before invocation.
- GREEN behavior run `33347652162`: managed `99354775361`, static `99354775208`; all behavior gates green before the expected generated-authority/workspace freshness boundary.

## Observed behavior qualification
- Windows Server 2025 / `windows-2025-vs2026` / .NET SDK 10.0.400.
- 18/18 locked restore; dependency audit PASS across 18 projects/18 frameworks with 0 vulnerable-result packages.
- Release build PASS with 0 warnings / 0 errors.
- Native Explorer, unsigned development MakeAppx package, direct staged class-factory Invoke and loose-package COM activation/Invoke PASS.
- Existing target smoke plus dev.16 36/36 Audio source/action matrix and repeated malformed/truncated single-file acceptance remain PASS.
- New mixed batch PASS on two complete attempts with aggregate exit 4, later-success isolation, numbered collision safety, preservation and no-partial guarantees.
- Managed tests 254/254 PASS, 0 skipped; Python static tests 91/91 PASS; contract vectors 5/5 PASS.
- Pre-authority deterministic A/B workspace SHA-256 `4af24ae6f866c6389a3010642504aea13952ecb17d717c9974d05161fb8f6ba0`, 447903 bytes, 364 entries; integrity then failed only because tracked generated authority still described the pre-dev.17 tree.

## Prior frozen authority
Dev.16 exact main `dca3cbcba326a35801bc442ec93f16d84f58a692`, tree `475e3f55ada62c6e5ae3b16fdba8098734fcef65`, run `33346588907`; jobs managed `99351773826`, static `99351774016`, continuity `99351774057`; workspace SHA-256 `75d7e25eec2f0164540812726151388911560455edea9a7fc0eaf1d9d3e927b5`, 442090 bytes, 361 entries; generated artifact `9742181454`, verified delivery `9742227375`.

## Authority rule
Dev.17 is frozen only after version-aligned generated authority is synchronized from one exact CI artifact, branch qualification reaches generated-authority zero-diff with managed/static behavior green, `main` is non-force fast-forwarded, and ordinary CI on exact current `main` has continuity + managed + supply-chain-static all SUCCESS with deterministic workspace verification and verified delivery upload.

## Remaining shipping gates
Headed Windows 11 UI/screenshots and Explorer crash/hang/failure matrix; production signed-package B2 requalification; production FFmpeg redistribution approval; signed production MSIX/clean-VM lifecycle; final security/fuzz/chaos/release/end-user acceptance.
