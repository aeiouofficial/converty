# Converty Universal Windows Right-Click Converter
## Architecture, Security, Full Build Plan, QA Gates, and Handover Baseline

**Planning baseline:** 2026-08-24
**Target:** Windows 11 first
**Implementation status:** **NOT STARTED — this package is a design/build authority, not proof that code exists.**
**Primary stack:** Native C++ shell integration + .NET 10 LTS coordinator/core + isolated worker processes + signed/pinned conversion engines + MSIX/sparse-package identity.

> **Reliability statement:** no non-trivial software can be truthfully guaranteed to “never bug, glitch, leak, or fail.” This plan converts that requirement into enforceable engineering invariants: fail closed, isolate untrusted parsing, bound resources, never overwrite transactionally, minimize ambient authority, validate every boundary, make failures recoverable, and require adversarial/chaos/fuzz testing before release.

## 1. Product mission

Create a native-feeling Windows 11 File Explorer conversion platform where a user can right-click supported files and convert them quickly to a chosen format or a per-family default. Audio, image, and video are first-class launch families; additional file families are added as isolated providers without redesigning Explorer integration or the coordinator.

### Required UX

- Right-click one or many files in Windows 11 Explorer.
- `Convert` appears in the modern context menu via `IExplorerCommand` and package identity [MS-1].
- Homogeneous selections expose valid/pinned targets for their family.
- Mixed selections expose `Convert using defaults` plus `More options…`.
- Defaults are stored per family: Audio, Image, Video, and later Document/Archive/etc.
- Standard fast conversion uses same-folder output with safe collision handling; policy can be changed in Settings.
- No console window; progress and result are handled by the app/notifications.
- Explorer never runs codecs, probes media, accesses the network, or performs long-running work.

## 2. Non-negotiable architectural invariants

1. **Explorer is a thin trigger, never a worker.** Menu construction is bounded and cheap [MS-1].
2. **Coordinator never parses untrusted media.** Full probing and decoding occur in disposable worker processes.
3. **No untrusted codec/plugin code is loaded into Explorer or the coordinator.** Provider/engine binaries run only in worker context.
4. **Untrusted parsing has no network in the strict profile.** Standard local conversions require no network capability.
5. **Workers receive least authority.** Narrow staging directories, bounded handles/capabilities, Job Object limits, and sandbox profile.
6. **Output publication is transactional.** Worker writes private temp → validate → atomically commit to final destination.
7. **IPC is explicit, versioned, bounded, authenticated to expected peer/user, and reject-by-default.**
8. **Presets contain structured typed values, never command lines or arbitrary pass-through engine arguments.**
9. **Providers are signed/pinned and capability-declared.** Adding a file family never grants arbitrary host loading.
10. **No silent isolation downgrade.** If strict isolation cannot be established, fail the job or require a visible user-approved compatibility profile.

## 3. Trust/process architecture

See `reference-images/01_system_architecture.png`, `02_runtime_conversion_flow.png`, and `03_security_boundaries.png`.

```text
Explorer.exe
  └─ Converty.ShellExtension.dll (native C++; tiny, no media parsing)
       └─ launches Converty.Bridge.exe (signed standalone executable)
            └─ authenticated/bounded named-pipe request
                 └─ Converty.Host.exe (.NET 10 coordinator; no media parsers)
                      ├─ queue/journal/policy/settings/capability registry
                      ├─ private staging manager
                      └─ launches disposable restricted workers
                           ├─ Converty.ProbeWorker.exe
                           └─ Converty.EngineWorker.exe
                                └─ provider adapter → pinned FFmpeg/WIC/future engine
```

### Why a Bridge process

A Bridge keeps the Explorer-loaded component trivial. Explorer does not need a long-lived IPC client, parser, settings database, or error-handling framework. If the Host is absent, the shell returns quickly after launching the Bridge. The Bridge owns startup/retry/IPC timeouts and exits.

## 4. Component contracts

### Converty.ShellExtension
**Language:** C++20 / Win32 COM.

May:
- implement `IExplorerCommand` and child commands;
- cheaply inspect selection metadata needed for command visibility;
- collect filesystem paths/shell items;
- launch only the signed Bridge with a short request token or secure rendezvous reference.

Must not:
- load FFmpeg/WIC/plugins;
- decode/probe content;
- make network requests;
- block on conversion;
- write outputs;
- accept untrusted preset command text.

### Converty.Bridge
**Language:** small native or .NET self-contained executable; choose during B2 based on startup/packaging measurement.

Responsibilities:
- validate shell request shape;
- ensure the request count/size limits;
- start Host if necessary;
- connect only to expected same-user Host endpoint;
- bounded handshake and write timeout;
- exit without waiting for conversion completion.

### Converty.Host
**Language:** C# / .NET 10 LTS.

Responsibilities:
- single-instance coordinator per interactive user;
- authenticated/bounded IPC;
- format/capability/preset registry;
- job planning, queue, journal, cancellation;
- staging-directory ownership;
- sandboxed worker launch;
- progress aggregation;
- post-worker validation and output commit;
- settings/notifications/diagnostics.

Host must not decode untrusted media or dynamically load arbitrary codec/plugin assemblies.

### ProbeWorker
Disposably identifies format/streams/metadata under strict resource limits. For launch families it calls approved probe backends in worker context (for example bundled `ffprobe` for A/V). Output is a strict structured `ProbeResult`, never executable text.

### EngineWorker
Runs one planned conversion under worker-specific sandbox/resource policy. Receives a typed conversion plan, input handles/paths within policy, a private output staging location, and an explicit provider ID. It never chooses the final user-visible filename.

### Providers
Provider package defines:
- provider ID and API version;
- supported source/target graph;
- typed option schema;
- sandbox requirements;
- binary/signature/hash manifest;
- license/redistribution metadata;
- worker adapter.

Provider executable code is loaded only inside worker processes.

## 5. Capability graph and future file families

The core does not contain extension chains such as `if .wav … else if .png`. It resolves:

```text
File type / probe result
 → family
 → source FormatId
 → compatible provider(s)
 → allowed target FormatId(s)
 → presets
 → execution mode: Copy | Remux | Transcode | Transform
```

Interfaces:

```text
IFileProbe
IConverterProvider
IConversionEngine
IPresetProvider
IOutputPathResolver
IConversionPolicy
IProgressReporter
IPluginCatalog
```

Domain objects:

```text
FileDescriptor
FormatDescriptor
ConversionRequest
ConversionPlan
ConversionPreset
ConversionJob
ConversionResult
EngineCapability
SecurityDecision
```

Launch families:
- Audio: FFmpeg worker provider.
- Video: FFmpeg worker provider with remux/stream-copy when valid.
- Image: WIC worker provider for appropriate Windows-native codecs plus an optional separately approved image provider for formats/features outside WIC.

Later providers: document, archive, subtitle, ebook, font, 3D/CAD, data/developer formats. Each provider must pass the same security contract and release gates.

## 6. Explorer UX

### Homogeneous audio selection
```text
Convert
  ├─ Convert to MP3              ← category default / fastest command
  ├─ MP3 — High Quality
  ├─ FLAC — Lossless
  ├─ Opus — Music
  └─ More conversion options…
```

### Homogeneous image selection
```text
Convert
  ├─ Convert to PNG
  ├─ PNG — Lossless
  ├─ JPEG — High Quality
  ├─ WebP
  └─ More conversion options…
```

### Homogeneous video selection
```text
Convert
  ├─ Convert to MP4
  ├─ MP4 — Compatible
  ├─ MP4 — High Quality
  ├─ WebM
  └─ More conversion options…
```

### Mixed selection
```text
Convert
  ├─ Convert using defaults
  └─ More conversion options…
```

Default is per family; never invent nonsensical common targets across mixed families.

## 7. Preset model

Factory/user presets use versioned schemas and typed values, for example:

```json
{
  "schemaVersion": 1,
  "id": "audio.mp3.high",
  "family": "audio",
  "outputFormat": "audio.mp3",
  "provider": "builtin.ffmpeg",
  "settings": {
    "codecProfile": "mp3.lame",
    "bitrateBps": 320000,
    "metadataPolicy": "preserve"
  }
}
```

The provider translates validated option IDs to engine arguments. Users cannot inject raw FFmpeg arguments through the preset schema.

## 8. IPC security design

Microsoft notes that a default named-pipe security descriptor grants read access to Everyone and the anonymous account [MS-2]. Converty therefore creates explicit security descriptors.

### Host pipe
- Per-user unpredictable or user-SID-qualified endpoint name.
- Explicit DACL restricted to the current interactive user SID plus only identities demonstrably required by the final design.
- Do not grant `Everyone`, `Anonymous`, broad `Users`, or unrelated app-container groups.
- Server verifies connecting process/token/SID/integrity/session where available and appropriate.
- Client verifies it is talking to expected signed Host where technically practical; prevent pipe-squatting with endpoint creation/handshake strategy.
- Versioned handshake.
- Fixed upper bound on frame size, files/request, paths, options, strings, nested depth, and queue submissions.
- Length-prefix parsing uses checked arithmetic; malformed/unknown versions reject.
- Read/write/handshake timeouts.
- No command-line fragments, raw engine arguments, scripts, or arbitrary environment values in IPC.

### Request shape
```text
ProtocolVersion
RequestId (random GUID)
Action
PresetId? / TargetFormatId?
FileCount
Canonical request items
Caller metadata required for authentication/routing
```

Paths remain untrusted data throughout planning. Extension is a hint, never the authority for media format.

## 9. Worker isolation and containment

### Strict target profile
Use the strongest production-supported combination qualified during implementation:
- dedicated disposable worker;
- AppContainer or equivalent restricted-token isolation [MS-4][MS-5];
- **no network capability** for local conversions;
- private per-job staging directory;
- only required file/directory access;
- Windows Job Object with kill-on-close and memory/CPU/process/time constraints [MS-3];
- process mitigations applied where compatible;
- explicit child-process policy;
- bounded pipe stdout/stderr/progress;
- environment allowlist;
- no inherited handles except an explicit allowlist;
- working directory inside private staging;
- child executables loaded from trusted install paths only.

Experimental/newer Windows sandbox APIs may be investigated, but are not made a production dependency until their support/lifecycle is proven [MS-6].

### Compatibility profile
Some engines/codecs may prove incompatible with the strictest container. If a compatibility profile is necessary:
- it is a distinct security profile;
- the worker still uses Job Object/resource/handle/network/file-scope controls where enforceable;
- no automatic fallback;
- user/policy decides whether the less-isolated operation is allowed;
- telemetry/log explicitly records the profile, without leaking media contents.

## 10. FFmpeg hardening

FFmpeg runs only in a worker context.

Rules:
- bundle or install a specifically pinned build;
- verify code signature/hash manifest before execution;
- never search `%PATH%` for the engine;
- direct process creation; no `cmd.exe`, PowerShell, shell expansion, or concatenated command line controlled by input;
- provider maps typed options to known argument tokens;
- set `protocol_whitelist` to the minimum needed for local conversion; FFmpeg documents that protocol allowlisting constrains usable protocols [FF-1];
- ordinary conversion disables network protocols;
- use explicit input/output and overwrite policy;
- bound threads/resources through engine options plus OS containment;
- progress uses machine-readable channel;
- stdout/stderr length bounded;
- probe/convert timeouts enforced externally by Host/Job Object;
- validate output independently before publish.

## 11. File/path/output transaction security

### Input validation
- Do not trust extensions.
- Reject/handle unsupported shell namespaces and non-filesystem items explicitly.
- Normalize/validate path handling without destroying valid long/Unicode Windows paths.
- Define reparse-point policy and test junction/symlink races.
- Open/revalidate inputs at the correct boundary to reduce path substitution races.
- Avoid exposing arbitrary filesystem scope to workers.

### Output lifecycle

```text
RESOLVE destination directory and collision policy
CREATE private job staging
WORKER writes job-id.partial inside staging
WORKER exits success/failure
HOST validates staged output
HOST revalidates final destination/collision
HOST commits with safe move/replace strategy
HOST publishes success
CLEAN staging
```

Default collision policy: **numbered copy**.

Never overwrite input. If overwrite is later enabled, make backup/replace semantics transactional and crash-tested; do not truncate the final file before conversion succeeds.

### Recovery journal
Persist only the minimum job state needed to identify abandoned private temp files after Host crash/reboot. On startup, clean only paths tied to valid Converty job metadata beneath Converty-owned staging roots.

## 12. Resource-exhaustion policy

Treat decompression bombs, giant dimensions/durations, malformed metadata, recursive containers, fork bombs, log floods, and enormous selection counts as hostile.

Controls:
- request count/size ceilings;
- pre-probe wall-clock/CPU/memory limits;
- decoded pixel/duration/frame/sample policy before expensive work where measurable;
- worker memory/CPU/process ceilings;
- staging quota/free-space preflight;
- output maximum and progress/log maximum;
- global and per-provider concurrency ceilings;
- cancellation always terminates the worker Job Object;
- no retry storms.

## 13. Supply-chain and plugin security

### Built-in engines/providers
Manifest includes:
- provider ID + API version;
- publisher;
- package/binary version;
- SHA-256 hashes;
- signature/publisher identity policy;
- supported architecture;
- licensing/notices;
- source URL/build provenance where distributable;
- capability declaration.

Host verifies package trust before selecting provider. Worker independently receives an exact provider path/ID selected by Host.

### Future plugins
Do not scan a writable `plugins` folder and load arbitrary DLLs.

Plugin path:
```text
Discover manifest
 → schema/version check
 → signature/publisher policy
 → hash/integrity check
 → capability registration as data
 → worker launch
 → plugin code loaded only in worker
```

Third-party plugins should ultimately use a separate worker process with a versioned protocol rather than in-process Host extension.

## 14. Logging/privacy

Structured events:
```text
job.accepted
job.rejected
probe.started/completed
plan.created/rejected
worker.started/terminated
output.validated/committed
job.completed/failed/cancelled
security.policy_denied
security.provider_integrity_failed
```

Rules:
- no media contents;
- no command strings containing uncontrolled data;
- secrets never logged;
- path logging minimized/redactable/hashable in diagnostic export;
- rotate and bound log size;
- user can clear diagnostics;
- crashes do not upload automatically unless a future explicit opt-in flow is designed.

## 15. Versioned schemas and persisted state

At minimum:
```text
conversion-request/1
probe-result/1
conversion-plan/1
job-journal/1
settings/1
preset/1
provider-manifest/1
format-registry/1
```

Rules:
- schema version mandatory;
- reject unsupported future version;
- migration functions are explicit and tested;
- backup before destructive settings migration;
- no polymorphic deserialization from arbitrary type names;
- persisted data is untrusted at load.

## 16. Settings application

Settings owns:
- default preset by family;
- pinned Explorer presets;
- same directory/subdirectory/custom output policy;
- collision policy;
- metadata policy;
- concurrency/resource defaults;
- strict/compatibility isolation permission;
- provider inventory/status;
- diagnostic export/clear.

Settings never edits raw engine command lines for built-in providers.

## 17. Repository topology

See `reference-images/08_workspace_structure.png`.

```text
Converty/
├─ src/
│  ├─ Converty.Contracts/
│  ├─ Converty.Core/
│  ├─ Converty.Ipc/
│  ├─ Converty.Security/
│  ├─ Converty.Host/
│  ├─ Converty.Bridge/
│  ├─ Converty.ProbeWorker/
│  ├─ Converty.EngineWorker/
│  ├─ Converty.Settings/
│  └─ Converty.Serialization/
├─ native/
│  └─ Converty.ShellExtension/
├─ providers/
│  ├─ Converty.Provider.FFmpeg/
│  └─ Converty.Provider.Wic/
├─ schemas/
├─ packaging/
│  └─ Converty.Package/
├─ tests/
│  ├─ Unit/
│  ├─ Integration/
│  ├─ IpcSecurity/
│  ├─ SandboxSecurity/
│  ├─ Explorer/
│  ├─ Fuzz/
│  ├─ MediaCorpus/
│  ├─ TransactionalOutput/
│  └─ Chaos/
├─ docs/
└─ reference-images/
```

Keep files small and single-purpose. Do not create parallel/duplicate capability registries, queues, settings stores, or security policy systems.

## 18. Build plan / execution order

### B0 — Repository, toolchain, reproducibility
**Deliverable:** empty-but-building solution with CI and release metadata discipline.
- Create solution topology and module contracts.
- Pin .NET SDK/toolchain and native compiler/CMake version policy.
- Enable warnings-as-errors, analyzers, nullable, native `/GS`, CFG, ASLR/DEP-compatible settings, SDL checks where appropriate.
- Dependency lock files.
- CI x64 Debug + Release.
- SBOM generation and dependency/static-analysis scan.
- Code-signing plan and dev/test certificate workflow that never commits private keys.
- Architecture/reference docs copied into repository.

**Gate:** clean clone builds reproducibly; dependency graph and SBOM reviewed.

### B1 — Core contracts + capability graph + fake engines
**Deliverable:** no OS integration yet; deterministic testable planning core.
- Versioned IDs/schemas.
- Format registry/capability graph.
- Structured preset validation.
- Per-family defaults.
- Output-path/collision resolver.
- Fake Audio/Image/Video providers.
- Property tests for filenames, Unicode, extension spoofing, collisions, huge selections.

**Gate:** all unit/property tests green; no external process execution.

### B2 — Host + Bridge + hardened IPC
**Deliverable:** same-user single-instance Host accepts fake conversion jobs from Bridge.
- Explicit pipe security descriptor [MS-2].
- Peer identity validation and anti-squatting strategy.
- Versioned bounded framing.
- Queue/journal/cancellation.
- Strict request-size/count limits.
- IPC fuzz target/corpus.
- Bridge startup and fast-failure behavior.

**Gate:** unauthorized/malformed/oversized IPC cannot enqueue jobs, crash Host, or allocate unbounded memory.

### B3 — Modern Explorer integration + package identity
**Deliverable:** actual Windows 11 default right-click command [MS-1].
- Native `IExplorerCommand` DLL.
- COM/package manifest registration.
- Sparse/MSIX packaging choice finalized by installer architecture.
- `GetState`/title/icon/subcommands bounded.
- Multi-select.
- Shell launches Bridge only.
- Headed Explorer tests on clean Windows 11 VM.

**Gate:** menu remains responsive under Host missing/crashed/slow scenarios; Explorer never hosts conversion code.

### B4 — Worker launcher + sandbox + Job Objects
**Deliverable:** fake worker runs with verified least privilege and private staging.
- Per-job staging directory/ACL.
- Restricted worker profile implementation.
- Job Object kill-on-close + limits [MS-3].
- AppContainer/restricted-token qualification [MS-4][MS-5].
- No-network canary test.
- File-scope canary test.
- inherited-handle audit.
- child-process audit.
- strict vs compatibility profile policy; no silent downgrade.

**Gate:** worker cannot network or modify canary file outside scope in strict profile; process tree dies on cancel/Host close.

### B5 — FFmpeg pinned provider + Audio MVP
**Deliverable:** original primary use case: right-click WAV/FLAC/etc. → MP3 320k.
- Verify/bundle pinned FFmpeg licensing/distribution strategy.
- Signed/hash manifest.
- Worker-only FFmpeg adapter.
- Local protocol allowlist [FF-1].
- WAV→MP3 320k preset.
- ffprobe under ProbeWorker.
- machine progress.
- Unicode/long-path/malformed audio corpus.
- numbered-copy transaction.

**Gate:** no console; no network; malformed inputs contained; original preserved across forced crash.

### B6 — Audio matrix + presets/defaults
- MP3, FLAC, WAV PCM, AAC/M4A, Opus, OGG, AIFF/ALAC only when verified by actual pinned build.
- Metadata policy.
- per-family default.
- pinned Explorer presets.
- batch queue/concurrency.

### B7 — Image provider
- WIC provider in worker [plus separately approved engine only if needed].
- PNG/JPEG/TIFF/BMP and verified formats.
- alpha flatten/reject policy.
- ICC/EXIF/XMP/GPS policy.
- dimension/pixel/decompression bomb limits.
- animated-image policy explicit.

### B8 — Video + remux optimization
- FFmpeg video provider.
- MP4/MOV/MKV/WebM matrix based on tested build.
- mode planner: Copy/Remux/Transcode.
- compatibility pixel-format/audio defaults.
- subtitles/HDR/metadata policy.
- hardware acceleration disabled by default until separately qualified; device-driver attack surface/per-machine variability requires its own tranche.

### B9 — Settings + notifications + queue UI
- Per-family defaults.
- custom/pinned presets.
- output/collision/metadata/concurrency/isolation settings.
- provider inventory and health.
- progress/error/cancel UI.
- accessibility/mobile-like responsive settings layout where applicable.

### B10 — Provider/plugin SDK proof
- Signed manifest/API contract.
- out-of-process plugin worker.
- one non-media sample provider (e.g. subtitle SRT↔VTT) proves no shell/core architecture changes.
- incompatible/unsigned/tampered plugin rejection tests.

### B11 — Security/fuzz/chaos qualification
- IPC structure-aware fuzzing.
- file/path/junction/symlink race tests.
- provider fuzz corpora.
- worker escape canaries.
- crash every transaction stage.
- disk-full/access-denied/long-path/Unicode/concurrent collisions.
- power-loss/reboot recovery simulation where practical.
- dependency and binary scanning.

### B12 — Installer/release candidate
- MSIX/sparse-package + conventional installer path finalized.
- code-sign binaries/package.
- clean Windows 11 VM install/update/uninstall.
- modern-menu headed acceptance.
- release SBOM/licenses/notices.
- release hash manifest.
- complete handover and operational docs.

## 19. Test matrix

### Unit/property
- ID/schema validation.
- capability resolution determinism.
- target filtering.
- preset validation.
- filename/collision rules.
- metadata-policy mapping.
- engine argument token generation.
- migration round trips.

### IPC/security
- unauthorized SID/session.
- fake server / pipe squatting.
- invalid version.
- truncated frame.
- huge length prefix.
- huge file count.
- duplicate/replay ID.
- disconnect at each handshake stage.

### Filesystem/transaction
- output exists races.
- input==output attempts.
- readonly destination.
- destination disappears.
- reparse points.
- path replacement between plan/commit.
- FAT/NTFS/network-drive policy differences if supported.
- disk full.
- crash/reboot cleanup.

### Media/adversarial
- random bytes with valid extension.
- polyglot files.
- corrupt container tables.
- giant dimensions/durations.
- deep/recursive metadata.
- decompression bombs.
- zero-length input.
- huge but valid input.

### Explorer
- single/multi/mixed select.
- 1,000+ selected items behavior/visibility limit.
- Host absent/crash/restart.
- Bridge absent/corrupt.
- extension unloaded/reloaded/update.
- no Explorer hang/crash/leak under repeated menu opens.

## 20. Release gates

Release candidate is blocked unless all are evidenced:

1. Clean Windows 11 x64 install/update/uninstall.
2. Modern context-menu command appears directly.
3. Explorer crash/hang matrix passes.
4. Same-user IPC auth and malformed-frame tests pass.
5. Strict worker cannot network or write outside allowed scope.
6. Job Object kills full process tree on cancel/crash.
7. Tampered/unsigned provider rejected.
8. All supported launch-family conversions pass corpus matrix.
9. Original files survive every injected failure stage.
10. No unbounded temp/log/staging accumulation after recovery.
11. SBOM/license/third-party notices complete.
12. Signed binaries/package and release hashes verified.
13. No open Critical/High security finding; Medium exceptions require written risk acceptance and compensating controls.

## 21. Next-agent startup procedure

1. Read this document completely.
2. Read `docs/SECURITY_THREAT_MODEL.md`, `docs/ARCHITECTURE_DECISIONS.md`, `docs/TASK_BACKLOG.md`, and `machine-readable/handover_state.json`.
3. Inspect the exact workspace hashes/version and repository status; do not assume code exists because this plan exists.
4. Start at the first incomplete batch only.
5. Test-first for each contract/bug/feature.
6. Never weaken shell/Host/worker boundaries to make a feature easier.
7. Update task backlog and machine-readable handover on every batch.
8. Do not claim a security or release gate without executable evidence.

## 22. What “done” means

Done is not “FFmpeg command works.” Done means a signed Windows 11 product whose Explorer integration remains cheap and safe, whose coordinator remains outside the parser trust boundary, whose worker blast radius is demonstrably bounded, whose outputs are transactionally published, and whose extension model does not turn future file families into arbitrary code execution inside Explorer/Host.
