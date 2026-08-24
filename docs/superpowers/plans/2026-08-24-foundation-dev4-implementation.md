# FileConvert Foundation dev.4 Implementation Plan

> **For agentic workers:** This tranche is the evidence-safe fallback defined by the dev.3 handover because .NET SDK 10.0.400 still cannot execute in the current sandbox. B2 remains blocked.

**Goal:** Strengthen B0 CI/release provenance and NuGet vulnerability-audit enforcement without inventing managed-runtime evidence, then deliver `0.1.0-dev.4`.

**Architecture:** Keep runtime architecture unchanged. Pin every external GitHub Action to an immutable 40-character commit, make those pins machine-readable and statically verified, explicitly configure NuGet audit sources/level/mode, and add a deterministic dependency-audit report verifier that can be exercised against fixtures now and real `.NET 10.0.400` output later.

**Tech Stack:** GitHub Actions YAML, Python 3.13 standard library + pytest, NuGet 7/.NET 10 audit features, PowerShell build orchestration, existing CMake topology smoke. No new production dependency.

**Spec:** `docs/superpowers/specs/2026-08-24-foundation-design.md`

## Global constraints
- SDK remains exactly `10.0.400`; managed evidence is NOT_RUN unless that SDK actually executes.
- No B2/IPC/Host implementation while B0/B1 managed gates remain blocked.
- External GitHub Actions must use full immutable commit SHAs; mutable `@vN`/branch refs are forbidden in workflows.
- NuGet audit remains enabled in `all` mode at `low` threshold, with a dedicated vulnerability-only audit source.
- Dependency-audit JSON verification fails closed on malformed output or any reported vulnerability.
- CI and release provenance metadata must be versioned and machine-readable.
- No signing secret/private key enters the repository or workspace package.

### Task 1 — Establish RED dev.4 provenance/audit gates
- Add tests for dev.4 version synchronization, required provenance files, immutable Action SHA policy, explicit NuGet audit source/level, dependency-audit fixture parsing, and CI integration.
- Run the focused suite and retain expected failures before implementation files/config changes exist.

### Task 2 — Pin CI actions and record provenance
- Upgrade/pin checkout, setup-python, and setup-dotnet to reviewed immutable 40-character SHAs with human-readable release comments.
- Add `machine-readable/ci_action_pins.json` containing owner/repo, semantic release, full SHA, release URL, and review date.
- Add `scripts/verify_ci_actions.py` that scans every workflow and fails on mutable/unapproved external action refs.

### Task 3 — Strengthen NuGet vulnerability auditing
- Set `NuGetAuditLevel=low` explicitly while retaining `NuGetAudit=true` and `NuGetAuditMode=all`.
- Add vulnerability-only `auditSources` in `NuGet.Config` using `https://data.nuget.org/v3/index.json`.
- Add `build/dependency-audit.ps1` to run `.NET 10` machine-readable transitive vulnerability listing after locked restore and then invoke the verifier.
- Integrate the audit step into verification/CI without pretending it ran in this sandbox.

### Task 4 — Add fail-closed dependency-audit report verification
- Add `scripts/verify_dependency_audit.py` for NuGet JSON output version 1.
- Require a projects list; count projects/frameworks/packages; fail if any top-level or transitive package has a non-empty `vulnerabilities` array.
- Add clean, vulnerable, malformed-version, and malformed-shape fixtures plus focused static tests.

### Task 5 — Synchronize authority/evidence
- Add `docs/supply-chain/CI_PROVENANCE_POLICY.md` and update release/SBOM/handover/backlog/status/toolchain/release-policy authority for dev.4.
- Extend `scripts/verify_repository.py` and static package checks to require the new controls.
- Keep managed build/xUnit/locks/dependency audit explicitly NOT_RUN/BLOCKED if SDK remains unavailable.

### Task 6 — Verify/freeze/package
- Run CI-action verifier, repository verifier, full Python static suite, contract vectors, deterministic source SBOM twice, CMake configure/build smoke, JSON/XML parse sweeps, whitespace/source scans, and release preflight expected-fail check.
- Generate source/package/hash manifests after final stabilization; create deterministic full-workspace ZIP twice; compare hashes; reopen and verify CRC and embedded SHA-256 entries.
