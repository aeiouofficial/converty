# Implementation status — 0.1.0-dev.6

## Tranche result
`0.1.0-dev.6` delivers the first evidence-backed B2 Host/Bridge hardened-IPC tranche while preserving the B0/B1 trust boundaries. No media parsing, FFmpeg/WIC execution, provider loading, or worker execution was introduced into Host or Bridge.

## Qualified evidence
The B2 behavior qualification used GitHub Actions Windows Server 2025 with .NET SDK exactly `10.0.400`:
- 15 managed projects restored from committed lock files.
- Restored-graph NuGet vulnerability audit: PASS, zero vulnerable-result packages.
- Release build: PASS, zero warnings, zero errors.
- Microsoft Testing Platform/xUnit: 108 total, 108 succeeded, 0 failed, 0 skipped.
- Checked-in IPC adversarial corpus: seven cases executed by managed tests against the real protocol codec/request-admission path.
- Raw contract vectors: PASS, 5/5.
- Repository/static boundary verification and native CMake topology smoke: PASS.

The immutable behavior run is recorded in `machine-readable/build_evidence.json`. Final dev.6 authority synchronization is separately verified before packaging.

## B2 implemented in this tranche
- fixed 12-byte v1 framing with checked exact reads and a 1 MiB payload ceiling;
- fail-closed bad magic, unsupported version, negative/oversized length, truncation, and cancellation handling;
- protected current-user named-pipe DACL construction;
- deterministic SID-hashed endpoint naming without exposing the raw SID;
- connected-client SID validation through pipe impersonation with a testable identity-reader seam;
- bounded in-memory Host queue with duplicate and capacity rejection;
- queued status lookup and cancellation;
- strict request admission where unauthorized/malformed requests cannot mutate the queue;
- ACL-backed Host named-pipe single-session server that validates peer identity before reading an application frame;
- bounded Bridge request client with a maximum 30-second connect timeout and strict acknowledgement validation;
- tested per-user Host single-instance lease primitive;
- checked-in and executable IPC adversarial corpus.

## B2 intentionally still open
The B2 batch is **not fully closed**. The next tranche must finish these items before B3 is treated as unblocked:
1. persistent crash-safe/atomic job journal and recovery behavior;
2. complete Host executable lifetime/server loop wired to the single-instance lease;
3. Bridge Host startup/retry/fast-failure process-lifetime behavior;
4. status/cancellation commands over the final wire surface if required by the runtime UX;
5. anti-squatting/signed-peer validation beyond same-user SID where practical under the selected packaging model;
6. any additional structure-aware/fuzz coverage required by the B2 acceptance matrix.

## Boundary status
Contracts/Core/Serialization remain free of process/network/engine/native-loading logic. Host/Bridge contain IPC/coordinator behavior only and do not execute conversion engines. Worker containment remains B4 authority; FFmpeg/WIC execution remains prohibited until those foundations are qualified.
