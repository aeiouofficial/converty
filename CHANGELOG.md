# Changelog

## 0.1.0-dev.9 — 2026-08-26
- Delivered the first automated functional Windows product path: modern packaged `IExplorerCommand` shell DLL → fixed app-local `Converty.Bridge.exe` → typed preset → fixed app-local FFmpeg → same-folder output.
- Added real C++20/MSVC `IExplorerCommand` root/subcommands, fixed Bridge launch, Release-native hardening, package COM server/context-menu registration, MakeAppx validation, loose registration, direct class-factory activation, and packaged COM activation/invocation smokes.
- Added pinned development FFmpeg/ffprobe 9.0.1 qualification with SHA-256 verification. This remains development-only and is not production redistribution approval.
- Restored the original Audio MVP requirement: WAV/FLAC/etc. → MP3 at 320 kbps; ffprobe now verifies the real product-smoke codec and bitrate.
- Added transactional development output publication using a Converty-owned partial path followed by no-overwrite numbered publication; collision races preserve both source and externally created destinations.
- Hardened Unicode/metacharacter paths, including literal PowerShell path validation for filenames containing brackets.
- Fixed FFmpeg/ffprobe version probing so process exit codes are captured before log truncation.
- Fixed the packaged Explorer verb identifier to the MakeAppx-valid alphanumeric `ConvertyConvert`.
- Fixed WinExe Bridge qualification by starting it through structured `ProcessStartInfo`, waiting with a finite deadline, and reading the process object's exit code.
- Qualified behavior at `b71aa06fb024afe85f64707b05d996e86c37d8c8`, GitHub Actions run `33001019450`: Release build 0 warnings/errors; 176/176 managed tests; 54/54 static tests after in-job authority generation; native build PASS; MakeAppx PASS; direct DLL Invoke PASS; package COM registration/Invoke PASS; Bridge→FFmpeg MP3/320k smoke PASS.
- Added ADR-013 documenting the approved product-first development exception. B2/B4/release hardening remains required before shipping.
- Headed Windows 11 modern-menu acceptance, containment, production FFmpeg licensing/signature policy, signed MSIX, and clean-VM release gates remain open.

## 0.1.0-dev.8 — 2026-08-26
- Added fixed trusted `Converty.Host.exe` startup and bounded one-launch Bridge retry coordination after connect-stage Host unavailability.
- Preserved explicit current-user pipe ACL/SID validation, bounded framing, persistent Host queue/journal recovery, and process-start confinement.
- Qualified 15-project locked restore, zero-vulnerability audit, zero-warning/error Release build, 129/129 managed tests, and native topology smoke.
- Kept B2 server-auth/status-wire/replay-session closure explicitly open.

## 0.1.0-dev.7 — 2026-08-25
- Added a strict persistent Host job journal with schema v1, 4096-entry and 8 MiB hard bounds, duplicate/unknown-member rejection, canonical ID validation, and deterministic ordering.
- Added crash-safe journal publication through a same-directory temporary generation using write-through plus disk flush before atomic replacement; orphan temporary files cannot override the committed generation.
- Added restart recovery: queued/terminal state is preserved while interrupted `Probing` through `Committing` states become `Failed` with an explicit Host-restart reason.
- Integrated journal recovery into `HostJobQueue` before new work is accepted and made enqueue/cancel mutations persist before in-memory publication; persistence failure leaves queue state unchanged.
- Wired the existing per-user single-instance lease into a bounded Host runtime/server loop and added a real no-console `Converty.Host` WinExe entrypoint using LocalAppData state.
- Qualified the dev.7 behavior head with 120/120 managed tests and native topology smoke.

## 0.1.0-dev.6 — 2026-08-25
- Began B2 with bounded/versioned IPC, explicit pipe security, bounded Host admission, and strict Bridge request sessions.
- Added 15 locked managed projects, vulnerability auditing, 108/108 managed tests, and the checked-in IPC adversarial corpus.

## 0.1.0-dev.5 — 2026-08-25
- Completed the Converty product-name migration, lock-file qualification, vulnerability auditing, Release build, 63/63 managed tests, static/provenance gates and native topology smoke.

## 0.1.0-dev.4 — 2026-08-24
- Pinned permanent GitHub Actions to reviewed full SHAs, added NuGet vulnerability auditing, deterministic SBOM/release preflight and CI containment.

## 0.1.0-dev.3 — 2026-08-24
- Added deterministic SPDX source/release SBOM tooling, release-input preflight, release policy, secret/package exclusions and strict raw contract vectors.

## 0.1.0-dev.2 — 2026-08-24
- Added strict serialization adapters, schema/domain alignment, property/adversarial suites, packaging policy and expanded static boundary gates.

## 0.1.0-dev.1 — 2026-08-24
- Added the initial .NET 10/C++20 repository foundation, contracts/core/fake providers, schemas, capability graph/planner, collision-safe output resolver, tests, native topology, CI and handover authority.
