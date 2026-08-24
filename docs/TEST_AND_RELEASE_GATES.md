# Test and Release Gates

## Gate rule
A release gate is binary. “Mostly works” is failure. Evidence must be retained with the build artifact.

## Shell gate
- Modern Windows 11 menu entry verified on clean VM.
- No conversion/probe/network in Explorer-loaded DLL.
- Explorer remains responsive if Host/Bridge/worker are absent, crash, or reject request.

## IPC gate
- Default pipe descriptor is not used.
- Unauthorized client is rejected.
- Oversize/malformed frame corpus does not crash or unboundedly allocate Host.
- Timeouts and disconnects leave no stuck jobs.

## Worker isolation gate
- No intended network egress under strict profile.
- Canary file outside job scope cannot be created/modified.
- Child process tree terminates on cancel/Host job-handle close.
- Resource ceilings terminate bombs without corrupting Host state.

## Transaction gate
- Crash at each lifecycle state leaves original unchanged.
- No final output is reported successful before validation+commit.
- Collision policy is race-safe in concurrent jobs.
- Abandoned temp work is recoverable/cleanable after restart.

## Supply-chain gate
- Every external GitHub Action reference is pinned to an approved full 40-character commit SHA and passes `scripts/verify_ci_actions.py`.
- CI checkout credentials are not persisted and every job has an explicit finite timeout.
- Reviewed `packages.lock.json` files exist for every managed project and locked restore succeeds.
- `./build/dependency-audit.ps1` produces a machine-readable transitive vulnerability report and `scripts/verify_dependency_audit.py` accepts it.
- Installed engine/provider binaries match signed/hash manifest.
- Tampered binary/provider is rejected before execution.
- Release-mode SBOM corresponds to the reviewed managed lock graph and shipping package.
- No unresolved dependency vulnerability is accepted silently; any future exception mechanism requires explicit reviewed policy and retained evidence.

## Provider gate
Every new provider must prove:
- source/target capability matrix;
- malformed corpus handling;
- resource bounds;
- deterministic error mapping;
- metadata/privacy behavior;
- sandbox compatibility level;
- license/redistribution compliance.

## Release-candidate gate
- Clean x64 Windows 11 VM install → use → update → uninstall.
- Headed Explorer acceptance.
- Unit/integration/security/fuzz/chaos suites pass.
- Release package signed.
- Release hash manifest, SBOM, third-party notices, known limitations, and handover are complete.
