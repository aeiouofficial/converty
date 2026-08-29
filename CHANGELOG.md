# Changelog

## 0.1.0-dev.12 — 2026-08-29
- Eliminated the historical `StrictWorkerIsTerminatedWhenStagingGrowthExceedsOutputBudget` scheduler-dependent test assumption without changing production output limits or containment.
- Replaced the arbitrary 512 KiB post-kill ceiling with a canary that writes exactly 64 KiB + 4 KiB and then holds until the existing launcher detects the breach and terminates the Job Object.
- Preserved RED evidence at `ad223384400be1c5749e0b09e301f7ddd5565eda` / run `33271012596` and failed intermediate experiments rather than rewriting history.
- Behavior head `f4c241b0895d06d2e44d72f31e07f141cdc74577` / run `33271379504` passed 192/192 managed tests, 72/72 static tests, 5/5 vectors, Release 0 warnings/errors, native/package/COM/product conversion gates; only generated-authority freshness remained intentionally open.
- Production `WindowsWorkerProcessLauncher`, resource limits, AppContainer/Job Object isolation, output polling and normal Bridge→Worker→FFmpeg path are unchanged.

## 0.1.0-dev.11 — 2026-08-29
- Closed development B2 connected-server identity/authentication without rerouting normal conversion through Host.
- Development package staging now includes exact sibling `Converty.Host.exe`.
- Proved real registered package COM shell `CreateProcessW` gives exact Bridge PFN `Converty.Dev_yr4ybytcyx7nj` (run `33218030168`, job `99005949641`).
- Proved package-identified parent→exact Host `Process.Start` preserves PFN (run `33211928010`, job `98986920905`).
- Proved packaged Bridge authenticates connected Host PID/path/PFN/stable PID before first application frame; Host accepted job `5bd48925-8c88-48d2-bbd7-a62c2ba03e3e` (run `33218498644`, job `99007347897`).
- Removed temporary diagnostics/invalid unpackaged-PowerShell positive smoke while retaining immutable Actions evidence.
- Pre-version exact tree `0d37afdba33abcd9ca31f3e59d0d6dc8a1bb7e5d` passed run `33260905467`: 192/192 managed, 66/66 static, 5/5 vectors, product/package/COM smokes, zero-diff authority, deterministic workspace and verified delivery.
- Source/version authority now identifies dev.11; generated dev.11 SBOM/package/hash authority regeneration and exact-head freeze remain next.

## 0.1.0-dev.10 — 2026-08-27
- Moved conversion execution out of the dev.9 Core/Bridge spike into fixed app-local `Converty.EngineWorker` and `Converty.Provider.FFmpeg`; Core now coordinates only a typed worker-client contract.
- Added unique private per-job staging so workers receive staged input/output paths rather than source/final publication destinations; validated output is published using the existing race-safe numbered no-overwrite transaction and owned staging is cleaned in `finally`.
- Added explicit `Strict` and `Compatibility` worker profiles with no automatic fallback. The product Bridge now requests `Strict`.
- Added suspended native worker creation with an explicit inherited-handle list and Job Object assignment before resume, including kill-on-close, active-process, process/job-memory, CPU and wall-clock limits plus bounded stderr.
- Added zero-capability per-launch AppContainer isolation, application read/execute and private-staging read/write ACL grants, reparse-point rejection, and cleanup of temporary isolation authority.
- Qualified strict executable canaries for staging write allowed, sibling/outside-scope write denied, loopback network denied, and descendant termination/resource containment.
- Added a finite output-growth ceiling: 8 GiB conversion default, 16 GiB hard configuration maximum, 25 ms staging-growth monitoring, final post-exit check, fail-closed reparse handling, and typed `WorkerOutputLimitExceededException`. A Windows strict canary proves termination after crossing a 64 KiB budget.
- Fixed two analyzer findings in the output monitor without changing containment semantics: explicit `CancellationToken.None` for the intentional polling delay and concrete `Dictionary` return type for the file-length snapshot helper.
- Behavior-qualified at `f221563c790057344a94b4e60c309d4512a77c38`, GitHub Actions run `33028554361` (managed `98375493893`, static `98375494099`): 18/18 locked restore, zero-vulnerability audit, Release 0 warnings/errors, native/package/direct+packaged COM smokes PASS, strict Bridge→EngineWorker→FFmpeg product smoke PASS, MP3 exactly 320000 bit/s, 190/190 managed tests, 66/66 static tests, 5/5 contract vectors.
- The behavior run produced byte-identical workspace ZIPs (`a0edd6e15a63d71cc2ef493ef33f6bb6e3f0b16ee0d8f484ebc981b800f749de`, 369035 bytes, 328 files) but final embedded-manifest verification intentionally remained blocked by stale tracked generated authority. dev.10 is not frozen until authority regeneration and exact-head requalification complete.
- Headed Windows 11 Explorer acceptance, remaining B2 final acceptance, production FFmpeg redistribution/signature/notices approval, signed production MSIX, clean-VM lifecycle, and final release audit remain open.

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
