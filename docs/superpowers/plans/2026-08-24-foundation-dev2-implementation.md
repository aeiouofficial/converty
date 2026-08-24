# FileConvert Foundation dev.2 Implementation Plan

> **For agentic workers:** Continue from the approved foundation design. Keep B2 blocked until a real .NET 10.0.400 managed build/test run succeeds.

**Goal:** Close remaining engine-independent B0/B1 contract gaps, add strict typed/versioned JSON adapters and deterministic adversarial/property coverage, then produce evidence-backed `0.1.0-dev.2`.

**Architecture:** Add `FileConvert.Serialization` as a one-way adapter between versioned JSON and immutable domain contracts. It may reference `FileConvert.Contracts`; Contracts/Core must not reference Serialization. Serialization owns strict JSON shape/version dispatch only and contains no transport, filesystem, process, media-parser, provider-loader, or network behavior.

**Tech Stack:** C# 14 / .NET 10 (`net10.0`), `System.Text.Json`, xUnit v3/Microsoft Testing Platform, Python static/schema tests, CMake native topology smoke.

**Spec:** `docs/superpowers/specs/2026-08-24-foundation-design.md`

## Global constraints
- SDK remains exactly `10.0.400` with `rollForward=latestPatch`.
- No new NuGet runtime dependency is introduced for serialization or property tests.
- Schema v1 remains the only accepted wire version; unknown/missing versions fail closed.
- Unknown JSON members fail closed.
- Stable wire enums use explicit strings, never CLR enum-name inference.
- Schema constraints and domain-constructor constraints must agree.
- Deterministic property/adversarial tests use fixed seeds and bounded iteration counts.
- Do not start B2 until managed Release build + xUnit executes successfully.

### Task 1 — Establish dev.2 RED gates
- Add static tests requiring the serialization module/project topology and dev.2 contract-bound tokens.
- Add C# boundary/property/serialization tests before production implementation.
- Run the executable Python static suite and retain the expected failures caused by missing dev.2 production files.

### Task 2 — Align domain constraints with schemas
- Bound request/probed paths at 32,767 characters.
- Bound display names at 128 characters.
- Bound format extensions at 32 entries.
- Bound preset options at 128 entries.
- Expose schema version on root `FormatDescriptor` and `CapabilityDescriptor` contracts while retaining current-version convenience constructors.
- Preserve immutable snapshots and fail-closed enum validation.

### Task 3 — Implement typed/versioned JSON adapters
- Create `src/FileConvert.Serialization` referencing Contracts only.
- Implement strict `System.Text.Json` options with unmapped members rejected, case-sensitive names, comments/trailing commas rejected, and bounded depth.
- Implement explicit schema-version dispatch before v1 conversion.
- Implement explicit wire text mappings for conversion action/mode/job-state enums.
- Implement v1 mappings for request, preset, capability, format, plan, and job status.
- Sort preset option keys on serialization for deterministic output.

### Task 4 — Add deterministic property/adversarial coverage
- Seeded identifier fuzz/property tests against the canonical identifier grammar.
- Seeded output-name tests across Unicode basenames and collision runs.
- Serialization round-trip tests for every root contract.
- Unknown version/member, invalid enum, over-limit value, and invalid conditional request tests.
- Confirm tests contain bounded loops and no random seed from wall-clock state.

### Task 5 — Strengthen static/CI gates
- Require all B1 projects in `FileConvert.slnx`.
- Verify Contracts/Core do not reference Serialization or execution/network APIs.
- Verify Serialization contains no process/network/FFmpeg/shell execution tokens.
- Make CI fail if committed lock files are missing or drift once generated.
- Record the failed local SDK provisioning attempt rather than inventing managed evidence.

### Task 6 — Verify and package
- Run repository verifier, Python static/schema suite, CMake configure/build smoke, `git diff --check`, JSON parsing, schema example validation, and package integrity checks.
- If .NET remains unavailable, keep managed compile/xUnit and lock-file generation explicitly blocked.
- Update version, changelog, backlog, implementation status, handover, machine-readable evidence/state.
- Generate package manifest and SHA256 manifest after final source stabilization.
- Commit the source snapshot, create deterministic full-workspace ZIP twice, verify identical SHA-256, reopen archive, verify CRC and embedded hashes.
