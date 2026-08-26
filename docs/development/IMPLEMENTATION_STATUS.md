# Implementation status — 0.1.0-dev.9

## Tranche result
`0.1.0-dev.9` delivers the first automated functional Converty product path under the explicitly approved ADR-013 product-first exception:

`IExplorerCommand → fixed app-local Bridge → typed preset → fixed app-local FFmpeg → transactional numbered output`

This is a development qualification milestone. It does not claim final worker containment, signed release packaging, production FFmpeg redistribution approval, or headed Windows 11 UI acceptance.

## Behavior qualification
Immutable functional behavior head: `b71aa06fb024afe85f64707b05d996e86c37d8c8`.
Permanent GitHub Actions run: `33001019450`; managed job `98282574626`; static job `98282574403`.
Runner: Windows Server 2025 (`windows-2025-vs2026`) with .NET SDK exactly `10.0.400`.

Executed results:
- 15/15 managed projects locked restore PASS.
- NuGet vulnerability audit PASS; 0 vulnerable-result packages.
- Release managed build PASS; 0 warnings, 0 errors.
- Native C++20/MSVC Explorer Release build PASS.
- Development FFmpeg/ffprobe 9.0.1 archive SHA-256 verification PASS; both executables execute successfully.
- MakeAppx unsigned development-package schema/layout validation PASS.
- Direct staged shell DLL class factory + `IExplorerCommand::Invoke` conversion PASS.
- Loose package registration + packaged COM activation + `IExplorerCommand::Invoke` conversion PASS.
- Direct Bridge→FFmpeg product smoke PASS with Unicode/metacharacter path, source preservation, collision preservation, numbered output, no partial-file leak, MP3 codec and exactly 320000 bit/s verified by ffprobe.
- Microsoft Testing Platform/xUnit: 176 total, 176 succeeded, 0 failed, 0 skipped.
- Static/repository gates at behavior head after in-job generation: 54/54 PASS; contract vectors 5/5 PASS.
- Deterministic workspace ZIP double build produced matching bytes/hash at the behavior head; final embedded-manifest verification was intentionally blocked by stale tracked generated authority and is closed by the subsequent authority-synchronization cycle.

## Implemented product slice
- Native root `IExplorerCommand` with fixed product subcommands and cheap extension-based visibility.
- Fixed app-local `Converty.Bridge.exe` launch from the shell DLL using Win32 process creation, no shell execution.
- Development Bridge mode validates a fixed typed preset ID and selected absolute files, then invokes the dedicated Core conversion runner.
- `ProductPresetRegistry` provides fixed audio/video/image conversions; Explorer never receives raw FFmpeg arguments.
- `audio.mp3` is the original 320 kbps Audio MVP.
- `TrustedFfmpegPath` resolves only `tools/ffmpeg/ffmpeg.exe` beneath the application directory and rejects reparse points.
- `FfmpegProcessLauncher` uses `ProcessStartInfo.ArgumentList`, no shell, hidden process, bounded stderr and finite timeout.
- `ConversionBatchRunner` validates supported inputs and converts through an owned partial output followed by no-overwrite numbered publication.
- Development package identity registers the shell class through `windows.comServer`/SurrogateServer and the modern context-menu verb through `windows.fileExplorerContextMenus`.
- Native/package/product smokes use real Unicode/metacharacter filenames and real FFmpeg conversion.

## Important corrections made during qualification
- FFmpeg/ffprobe version probes now capture process exit before truncating log output.
- Explorer context-menu verb ID changed from schema-invalid `Converty.Convert` to `ConvertyConvert`.
- PowerShell smoke validation uses literal-path semantics for filenames containing `[ ]`.
- Product smoke explicitly waits for the `WinExe` Bridge process and reads its process-object exit code.

## Remaining shipping gates
1. Headed Windows 11 Explorer acceptance: visually prove Converty appears in the modern right-click menu, enumerate the expected submenu, invoke a real conversion through Explorer UI, and capture current-version screenshots/evidence.
2. B4 containment: private staging, restricted disposable workers, Job Object/resources, no-network and outside-scope-write canaries, no silent isolation downgrade.
3. Move FFmpeg execution from the dev.9 Core/Bridge product spike to the final worker/provider architecture without regressing Explorer UX/output behavior.
4. Finish B2 connected-server anti-squatting/final status-wire/replay-session acceptance.
5. Production FFmpeg licensing/redistribution/signature/hash/notices decision; dev Gyan payload is qualification-only.
6. Signed production MSIX and clean Windows 11 VM install/update/uninstall acceptance.
7. Final security/fuzz/chaos/release audit and end-user shipping acceptance.

## Boundary status
Contracts and Serialization remain engine-independent. Host remains non-executing and does not parse hostile media. Explorer remains trigger-only. Dev.9 temporarily permits conversion execution in the dedicated Core launcher invoked by Bridge under ADR-013; this development exception must be removed by migrating engine execution into the restricted worker/provider boundary before release.
