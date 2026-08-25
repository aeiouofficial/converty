# Implementation status — 0.1.0-dev.7

## Tranche result
`0.1.0-dev.7` advances B2 from hardened IPC primitives into crash-recoverable Host runtime behavior. It adds a persistent bounded Host job journal and wires the per-user single-instance lease into a real no-console Host WinExe server loop. No media parsing, FFmpeg/WIC execution, provider loading, or worker execution was introduced into Host or Bridge.

## Qualified behavior evidence
The dev.7 B2 behavior qualification used GitHub Actions Windows Server 2025 with .NET SDK exactly `10.0.400`:
- 15 managed projects restored from committed lock files.
- Restored-graph NuGet vulnerability audit: PASS, zero vulnerable-result packages.
- Release build: PASS, zero warnings, zero errors.
- Microsoft Testing Platform/xUnit: 120 total, 120 succeeded, 0 failed, 0 skipped.
- Raw contract vectors and repository/static boundary verification: PASS.
- Native CMake topology smoke: PASS.

The immutable behavior run is recorded in `machine-readable/build_evidence.json`. Final authority/package synchronization is performed separately at tranche closure.

## B2 implemented through this tranche
Existing dev.6 controls remain intact:
- fixed 12-byte v1 framing with checked exact reads and a 1 MiB payload ceiling;
- protected current-user named-pipe DACL;
- SID-hashed endpoint identity;
- connected-client SID validation before application-frame parsing;
- bounded Host admission/status/cancellation queue;
- strict request admission and one-session ACL-backed Host pipe server;
- bounded Bridge client with a maximum 30-second connect timeout;
- executable adversarial IPC corpus.

Dev.7 adds:
- strict Host journal schema v1 with 4096-entry and 8 MiB bounds;
- rejection of unknown/duplicate journal members and duplicate job/request IDs;
- same-directory temporary journal generation using write-through and disk flush before atomic publication;
- orphan temporary-file removal without allowing it to override the committed generation;
- restart recovery that preserves queued/terminal state but converts `Probing` through `Committing` to `Failed` with an interruption reason;
- queue integration where enqueue/cancel journal persistence happens before in-memory publication and persistence failure leaves queue state unchanged;
- journal recovery before new IPC work is admitted;
- per-user `HostSingleInstanceLease` wired into the Host runtime loop;
- `Converty.Host` WinExe entrypoint using LocalAppData state and no console dependency;
- runtime/server-loop tests in addition to journal recovery tests.

## B2 intentionally still open
B2 is **not fully closed**. B3 remains blocked until these items have executable acceptance evidence:
1. Bridge trusted Host startup/retry/fast-failure process-lifetime behavior with finite deadlines and trusted installed executable identity/path policy;
2. server-auth / named-pipe squatting resistance appropriate to the selected packaging/signing model beyond current-user client SID validation;
3. final decision and coverage for dedicated status/cancel wire operations if the runtime UX requires them;
4. additional restart/replay/disconnect/timeout/session-confusion adversarial cases needed by the final B2 matrix;
5. explicit B2 closure demonstrating malformed, unauthorized, oversized, replayed, disconnected, squatted-endpoint, and Host restart/crash paths cannot corrupt or incorrectly enqueue state.

## Boundary status
Contracts/Core/Serialization remain free of process/network/engine/native-loading logic. Host/Bridge contain only trusted coordination/IPC/process-lifetime behavior and do not execute conversion engines or parse hostile media. Worker containment remains B4 authority; FFmpeg/WIC execution remains prohibited until those foundations are qualified.
