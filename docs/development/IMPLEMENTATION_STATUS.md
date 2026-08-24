# Implementation status — 0.1.0-dev.4

## Scope of this tranche
`0.1.0-dev.4` remains B0/B1 only. The required .NET SDK `10.0.400` is still not installed in this execution container, and the official Linux x64 archive could not be fetched through the sandbox download path. B2 therefore remains deliberately unstarted.

Dev.4 advances only evidence-safe B0 work: immutable GitHub Action provenance, explicit NuGet vulnerability-audit policy, machine-readable audit report verification, CI integration, and tighter release-preflight authority. Runtime architecture is unchanged.

## Implemented source/tooling
- All dev.3 engine-independent Contracts/Core/Serialization/FakeProviders source and tests remain present unchanged in architectural role.
- Every external GitHub Action in `.github/workflows` is pinned to a reviewed full 40-character commit SHA; `scripts/verify_ci_actions.py` rejects mutable/unknown/drifted references.
- CI checkout uses `persist-credentials: false`, workflow permissions remain `contents: read`, and static/managed jobs are bounded to 15/30 minutes.
- `Directory.Build.props` explicitly requires `NuGetAudit=true`, `NuGetAuditMode=all`, and `NuGetAuditLevel=low`.
- `NuGet.Config` uses `https://data.nuget.org/v3/index.json` as a vulnerability-only audit source.
- `build/dependency-audit.ps1` is prepared to run `.NET 10` `dotnet package list --include-transitive --vulnerable --format json --output-version 1` after locked restore.
- `scripts/verify_dependency_audit.py` fails closed on malformed report shape/version or any vulnerability and has clean/vulnerable/malformed fixtures exercised by Python tests.
- Existing deterministic SBOM/release-preflight/signing-key-custody/package-secret controls remain active.

## Managed verification not executed here
The container still has no `dotnet`, and the official SDK archive fetch is blocked. Therefore:
- NuGet lock generation/review: NOT RUN.
- NuGet restore vulnerability audit: NOT RUN.
- machine-readable real dependency vulnerability report: NOT RUN.
- managed Release build: NOT RUN.
- xUnit/Microsoft Testing Platform: NOT RUN.
- B1 managed-runtime correctness remains unqualified.

## B0 blockers before closure
- Execute with .NET SDK exactly `10.0.400`.
- Run `./build/bootstrap.ps1 -GenerateLockFiles`, review/commit all lock graphs, then run locked restore.
- Run `./build/dependency-audit.ps1` and retain the actual machine-readable vulnerability evidence.
- Execute Release build and all managed tests with warnings-as-errors.
- Generate release-mode SBOM from reviewed locks and perform dependency/license/vulnerability review.
- Add qualified Windows x64 Debug/Release evidence and actual release-signing plumbing/evidence on approved release infrastructure.

## B1 blockers before closure
- Compile and execute every managed test suite.
- Fix compiler/analyzer/runtime defects without weakening strict contracts or security boundaries.
- Keep schema migration work version-driven; v1 correctly rejects unknown versions and no v2 exists yet.

## Next work order
1. Deliver `0.1.0-dev.5` from this complete dev.4 workspace.
2. Finish the .NET-capable B0/B1 gate first.
3. Only after that gate is green, write the B2 design/plan and implement Host/Bridge authenticated IPC test-first.
4. Do not add FFmpeg/WIC execution before B2/B4 containment foundations exist.
