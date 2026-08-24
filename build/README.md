# Build commands

- `bootstrap.ps1 -GenerateLockFiles` — first .NET-capable run only; generates NuGet lock files for review/commit.
- `bootstrap.ps1` — subsequent fail-closed locked restore.
- `build.ps1` — warnings-as-errors C# build after restore.
- `test.ps1` — xUnit plus executable Python static/schema gates.
- `verify.ps1` — full managed verification sequence.
- `native-smoke.ps1` — configures/builds the target-free B0 CMake topology; B3 adds the actual Explorer target.
