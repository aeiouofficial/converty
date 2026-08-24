# FileConvert Foundation dev.3 Implementation Plan

> **For agentic workers:** This tranche is the evidence-safe fallback defined by the dev.2 handover because .NET SDK 10.0.400 could not be provisioned in the current sandbox. B2 remains blocked.

**Goal:** Strengthen B0 supply-chain/release evidence and B1 static adversarial verification without inventing managed-runtime proof, then deliver `0.1.0-dev.3`.

**Architecture:** Keep runtime architecture unchanged. Add deterministic source-SBOM and release-preflight tooling outside production assemblies, keep release signing keys external to the repository/workspace, and add raw contract vectors that independently exercise the v1 fail-closed JSON/schema policy.

**Tech Stack:** Python 3.13 standard library + existing `jsonschema`/`pytest` static-test dependencies, SPDX JSON 2.3, existing CMake topology smoke. No new production dependency.

**Spec:** `docs/superpowers/specs/2026-08-24-foundation-design.md`

## Global constraints
- SDK remains exactly `10.0.400`; managed evidence is NOT_RUN unless that SDK actually executes.
- No B2/IPC/Host implementation while B0/B1 managed gates remain blocked.
- SBOM generation never fabricates dependency versions and never contacts package feeds.
- Release-mode SBOM/preflight fail if any managed project lacks `packages.lock.json`.
- Signing private keys/secrets never enter source control or workspace ZIPs.
- Release integrity uses SHA-256 or stronger; MD5/SHA-1 are forbidden for release integrity.
- Source-only SBOM is development inventory, not release dependency/license/vulnerability evidence.

### Task 1 — Establish RED supply-chain gates
- Add static tests for dev.3 version, SBOM/release policy authority files, deterministic source SBOM, fail-closed release mode, secret exclusion, and adversarial vector manifest.
- Run the focused suite and retain the expected failures before production/tooling files exist.

### Task 2 — Add deterministic SBOM tooling
- Add `scripts/generate_sbom.py` with explicit `source` and `release` modes.
- Source mode inventories first-party managed projects in deterministic SPDX 2.3 JSON.
- Release mode requires every managed project lock file and derives NuGet package versions only from committed lock data.
- Never fetch packages or infer/fabricate versions.

### Task 3 — Add signing/release preflight policy
- Add Markdown and machine-readable release/signing policy.
- Add `scripts/verify_release_inputs.py` to fail closed on missing managed locks, invalid release policy, or private-key/secret-like workspace material.
- Extend workspace packaging policy to exclude `.pfx`, `.p12`, `.key`, `.pem`, and `.env`.

### Task 4 — Add raw contract-vector verification
- Add checksum-pinned v1 vectors covering valid request, duplicate member, unknown member, unknown schema version, and executable-command injection member.
- Add strict Python vector runner: reject duplicate keys before JSON Schema, validate all remaining cases against v1 request schema, and fail on manifest expectation drift.

### Task 5 — Integrate static/CI gates and version authority
- Update repository verifier/package tests/current docs and machine-readable state to dev.3.
- Add supply-chain static CI execution without adding signing secrets or relaxing the managed lock gate.
- Preserve historical dev.1/dev.2 plan/changelog text as history.

### Task 6 — Verify/freeze/package
- Run repository verifier, all Python static tests, contract vector runner, deterministic source SBOM double-generation, CMake configure/build smoke, JSON parse sweep, whitespace/source scan, and release preflight expected-fail check.
- Keep .NET restore/build/xUnit and NuGet lock generation explicitly NOT_RUN if SDK remains unavailable.
- Generate source/package/hash manifests after final stabilization; create deterministic full-workspace ZIP twice; compare hashes; reopen and verify CRC and embedded SHA-256 entries.
