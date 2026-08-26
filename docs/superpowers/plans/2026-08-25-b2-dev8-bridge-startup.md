# Converty 0.1.0-dev.8 — B2 Bridge Host Startup / Server-Auth Plan

## Scope
Continue B2 only. Do not begin B3 and do not add FFmpeg/WIC/media execution.

## Locked boundaries
- Bridge is a tiny activation client. It never parses media and never accepts executable command text or arbitrary process arguments from IPC/presets/callers.
- Host remains the same-user trusted coordinator and still performs no media parsing/provider execution.
- Existing current-user DACL, connected-client SID validation, bounded/versioned framing, queue/journal limits, strict JSON, and finite timeouts remain unchanged.
- Signing/package identity is not fabricated. Unsigned CI binaries must not be mislabeled as signed-server authentication evidence.

## Task 1 — trusted installed Host path + launcher
RED tests first:
- Host executable path is derived from one trusted install directory and fixed filename `Converty.Host.exe`.
- relative/empty/nonexistent/reparse-point install inputs fail closed.
- launcher uses `UseShellExecute=false`, no arguments, no caller command line, hidden/no-console startup.
- launcher cannot be redirected to an arbitrary executable by request/preset data.

GREEN implementation:
- `TrustedHostPath`
- `IHostProcessLauncher`
- `InstalledHostProcessLauncher`

## Task 2 — bounded startup/retry coordinator
RED tests first:
- first successful submission never launches Host.
- only transport-unavailable failures trigger one Host launch.
- protocol/schema/rejection responses never trigger process startup.
- startup retry has a bounded total deadline and bounded retry delay.
- exactly one launch attempt per submission.
- cancellation aborts startup/retry promptly.

GREEN implementation:
- injectable request-client interface around existing `BridgeClient`.
- `BridgeSubmissionCoordinator` orchestrates connect -> trusted launch -> bounded retry.
- existing `BridgeClient` remains strict one-session transport.

## Task 3 — server-auth / squatting acceptance design
Do not claim this closed with unsigned CI binaries. Add testable policy abstractions/evidence for selected packaging identity and document exactly what remains dependent on signing/package deployment.

## Verification
Use .NET SDK exactly 10.0.400 on Windows Server 2025. Require committed locks, vulnerability audit, zero-warning Release build, all MTP/xUnit tests, static/repository gates, contract vectors, and native topology smoke. At tranche freeze regenerate source/release SBOM, package/hash manifests, deterministic full-workspace ZIP, and dev.9 handoff.
