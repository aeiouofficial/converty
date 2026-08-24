# Dependency policy

1. Direct NuGet versions are centrally pinned in `Directory.Packages.props`.
2. Every project must have a reviewed `packages.lock.json` before B0 can close.
3. CI/default local restore uses locked mode after lock files exist.
4. NuGet audit is enabled with `NuGetAuditMode=all`.
5. New runtime dependencies require a short rationale, license review, vulnerability review, and handover update.
6. Native/converter executables require exact version, download authority, SHA-256/SHA-512 verification, redistribution/license review, and runtime integrity verification before execution code is accepted.
7. GitHub Actions are workflow dependencies and must be SHA-pinned during B10 supply-chain closure; version tags in the current bootstrap workflow are not release authority.
