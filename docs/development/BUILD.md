# Build and clean-clone procedure

## Supported development target
Windows 11 x64, .NET SDK 10.0.400, C# 14, CMake 3.28+, MSVC C++20 toolchain.

## First .NET-capable run
1. Verify `dotnet --version` is exactly `10.0.400`.
2. Run `./build/bootstrap.ps1 -GenerateLockFiles`.
3. The bootstrap must regenerate lock files, verify every project received one, and immediately succeed in `--locked-mode`.
4. Review every generated `packages.lock.json` for the expected direct/transitive package graph.
5. Run `./build/dependency-audit.ps1`; retain and review `artifacts/dependency-audit/nuget-vulnerabilities.json`. Investigate every finding instead of suppressing it globally.
6. Run `python scripts/verify_release_inputs.py` and `python scripts/generate_sbom.py --mode release`; these must use the reviewed lock graph rather than inferred dependency versions.
7. Commit the reviewed lock files.
8. Run `./build/verify.ps1`.
9. Run `./build/native-smoke.ps1` on the qualified Windows/MSVC environment in addition to the Linux topology smoke.

## Subsequent clean-clone verification
```powershell
./build/bootstrap.ps1
./build/dependency-audit.ps1
./build/build.ps1 -Configuration Release
./build/test.ps1 -Configuration Release
./build/native-smoke.ps1
```

The default bootstrap uses `--locked-mode` and intentionally fails if a project lacks a lock file.

## Current execution-environment limitation
The ChatGPT Linux container used for `0.1.0-dev.4` does not provide `dotnet`. The official SDK 10.0.400 archive was located, but the sandbox download path could not provision it. No managed compile/xUnit/lock-file/dependency-audit result is claimed from this environment. Source-SBOM generation is allowed without locks, while release-mode SBOM generation and release preflight intentionally fail until the reviewed managed lock graph exists.
