# Converty Foundation Design — v0.1.0-dev.1

## Authority
This implementation tranche is approved by the user's request to begin implementing the existing architecture/handover pack. The normative architecture remains `docs/Converty_Master_Build_Plan.md`, `docs/SECURITY_THREAT_MODEL.md`, and the ADRs.

## Scope
Implement B0 repository/bootstrap authority plus the deterministic, engine-independent portion of B1. No Explorer registration, IPC server, sandbox launcher, FFmpeg invocation, WIC conversion, or plugin loading is claimed in this tranche.

## Locked boundaries
- Explorer/native shell work remains a future native C++ component and must not parse media.
- Host/core remains .NET 10 and never accepts executable command strings as presets.
- Formats, capabilities, planning, naming, and schemas are generic across file families.
- Conversion planning operates on trusted probe descriptors supplied by a future isolated probe worker; it does not parse media itself.
- Output naming is deterministic and collision-safe; final transactional commit remains a later tranche.
- Fake providers are test infrastructure only and never execute external code.

## v0.1.0-dev.1 deliverables
1. Reproducible repository metadata and pinned SDK/toolchain policy.
2. .NET solution/project topology for Contracts, Core, FakeProviders, and tests.
3. Versioned domain contracts for request, format, capability, preset, plan, and job state.
4. Strong format/family identifiers with validation.
5. Capability graph and deterministic provider matching.
6. Conversion planner that rejects impossible/ambiguous requests.
7. Output path resolver with numbered-copy collision policy.
8. Fake Audio/Image/Video providers for deterministic tests.
9. Static repository verification that can run even when .NET is unavailable.
10. Updated handover/state/version manifests and a full-workspace ZIP.

## Explicitly deferred
B2 IPC, B3 Explorer, B4 process containment, B5+ real conversion engines, B9 settings UI, B10 plugin SDK, and release signing/package execution.
