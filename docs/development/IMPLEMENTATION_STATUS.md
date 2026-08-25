# Implementation status — 0.1.0-dev.8

## Tranche result
`0.1.0-dev.8` advances B2 by adding a trusted fixed installed-Host startup boundary and a bounded one-launch Bridge startup/retry coordinator. It does not add media parsing, FFmpeg/WIC execution, provider loading, or worker execution to Host or Bridge.

## Qualified behavior evidence
The dev.8 behavior qualification used GitHub Actions Windows Server 2025 with .NET SDK exactly `10.0.400`:
- 15 managed projects restored from committed lock files.
- Restored-graph NuGet vulnerability audit: PASS, zero vulnerable-result packages.
- Release build: PASS, zero warnings, zero errors.
- Microsoft Testing Platform/xUnit: 129 total, 129 succeeded, 0 failed, 0 skipped.
- Raw contract vectors and repository/static boundary verification: PASS on the behavior workflow.
- Native CMake topology smoke: PASS.

The immutable behavior run is recorded in `machine-readable/build_evidence.json`. Final generated-authority/package synchronization is performed separately by the self-cleaning tranche closure workflow.

## B2 implemented through this tranche
Existing dev.6/dev.7 controls remain intact:
- fixed 12-byte v1 framing with checked exact reads and a 1 MiB payload ceiling;
- protected current-user named-pipe DACL;
- SID-hashed endpoint identity;
- connected-client SID validation before application-frame parsing;
- bounded Host admission/status/cancellation queue;
- strict persistent 4096-entry / 8 MiB crash-recovery journal with persist-before-publish mutation ordering;
- per-user Host single-instance WinExe server loop;
- strict Bridge request/acknowledgement transport with a maximum 30-second connect timeout;
- checked-in IPC adversarial corpus.

Dev.8 adds:
- `TrustedHostPath`, which accepts only an absolute existing installation directory and derives the fixed `Converty.Host.exe` filename;
- rejection of reparse-point install directories or Host executables at trust-path construction time;
- `InstalledHostProcessLauncher`, the only approved Bridge `Process.Start` site, with no shell execution, no caller arguments, no console window, and a trusted working directory;
- a narrow `BridgeHostUnavailableException` emitted only by the connection stage, not by frame/response parsing;
- `IBridgeRequestClient` so startup orchestration is independently testable without weakening `BridgeClient`;
- `BridgeSubmissionCoordinator`: first tries the existing Host, starts the fixed Host at most once only for connect-stage unavailability, then retries inside a maximum 30-second startup deadline with delay capped at one second;
- protocol failures and application rejections that never trigger process startup;
- cancellation that terminates startup/retry rather than being translated into Host-unavailable behavior;
- static/repository gates that still forbid process APIs in Host and everywhere in Bridge except the dedicated startup module, while media/network execution remains forbidden across both modules.

## B2 intentionally still open
B2 is **not fully closed**. B3 remains blocked until these items have executable acceptance evidence:
1. connected-server identity / anti-squatting validation before Bridge trusts application data, aligned to the actual installer/signing/package authority;
2. final decision and coverage for dedicated status/cancel wire operations if required by the runtime UX;
3. remaining replay/disconnect/reconnect/session-confusion acceptance cases;
4. explicit final B2 closure proving malformed, unauthorized, oversized, replayed, disconnected, wrong-server, and Host restart/crash paths cannot corrupt or incorrectly enqueue state.

Fixed-path startup, pipe naming, or same-user ownership alone are **not** recorded as complete server authentication. Production signing private keys remain outside the repository/workspace.

## Boundary status
Contracts/Core/Serialization remain free of process/network/engine/native-loading logic. Host still contains no process-launch/media-engine path. Bridge process creation is restricted to the dedicated fixed-Host startup module; all other Bridge code remains process-free. Host/Bridge do not execute conversion engines or parse hostile media. Worker containment remains B4 authority; FFmpeg/WIC execution remains prohibited until those foundations are qualified.
