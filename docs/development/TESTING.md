# Testing policy

## Managed tests
The managed projects use xUnit v3 through `xunit.v3.mtp-v2` and the .NET 10 Microsoft Testing Platform runner.

First qualified run:
```powershell
./build/bootstrap.ps1 -GenerateLockFiles
./build/dependency-audit.ps1
./build/build.ps1 -Configuration Release
./build/test.ps1 -Configuration Release
```

Subsequent runs use `./build/bootstrap.ps1` in locked mode.

## Static/schema gates
The Python suite is intentionally independent of `dotnet` so architecture/security/package invariants remain executable when a managed SDK is absent:
```bash
python scripts/verify_ci_actions.py
python scripts/verify_repository.py
python scripts/verify_contract_vectors.py
python -m pytest -q tests/static
```

These tests do not replace C# compilation/xUnit execution.

## Native topology smoke
```bash
cmake --preset native-smoke
cmake --build --preset native-smoke
```

This verifies only CMake topology on non-MSVC platforms. B3 must add a real MSVC Explorer target and Windows evidence.
