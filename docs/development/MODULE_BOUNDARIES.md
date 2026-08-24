# Module boundaries

## Implemented foundation
### FileConvert.Contracts
Owns versioned immutable identifiers and declarative data contracts. It must stay free of process launch, networking, native loading, codec APIs, filesystem mutation, media parsers, and JSON transport policy.

### FileConvert.Core
Owns deterministic format lookup, capability matching, planning, and output-name selection. It consumes trusted `ProbedFileDescriptor` data from a future isolated probe boundary; it never derives that data by parsing media.

### FileConvert.Serialization
Owns strict versioned JSON ↔ Contracts mapping only. It references Contracts but Core/Contracts do not reference it. It performs no IPC transport, filesystem operations, process creation, native loading, media parsing, provider execution, or networking. Unknown versions/members and duplicate JSON property names are rejected.

### FileConvert.FakeProviders
Owns deterministic capability fixtures only. No process launch or filesystem/media interaction.

## Deferred process modules
These directories contain boundary documents only until their planned batch. A no-op executable would be misleading evidence and is therefore intentionally absent.
- ShellExtension: B3.
- Bridge/Host/Ipc: B2.
- ProbeWorker/EngineWorker/Security: B4.
- FFmpeg: B5.
- WIC: B6.
- Settings: B9.
- Package: B3/B12.

## Dependency direction
`Serialization → Contracts`

`FakeProviders → Contracts`

`Core → Contracts`

`Contracts → nothing project-local`

No production dependency is permitted from Contracts/Core into Serialization, Host, worker, shell, provider, or engine modules.
