# Workspace versioning and assistant tranche protocol

Every implementation tranche increments the development version and ships a complete ZIP of the workspace, not a delta archive.

## Current series
`0.1.0-dev.N`

## Required at each tranche boundary
1. Update `VERSION`, `eng/toolchain.json`, README, CHANGELOG, implementation status, backlog, handover, and machine-readable state.
2. Re-run all executable gates available in the current environment.
3. Report unavailable gates as `NOT_RUN`, never as inferred passes.
4. Generate `machine-readable/package_manifest.json` and `SHA256SUMS.txt` after all content has stabilized.
5. Create deterministic full-workspace ZIP and compute SHA-256.
6. Reopen ZIP and verify CRC, embedded SHA-256 entries, package manifest entries, expected version, and absence of excluded transient/secret-like paths.
7. Provide a copy-paste continuation prompt naming the next required version.

## Naming
Workspace root: `Converty_<VERSION>`

Archive: `Converty_<VERSION>_full_workspace.zip`
