# Converty dev.13 Status/Cancel Wire Design

## Status

Approved in chat on 2026-08-30 as **Approach A: backward-compatible job-management operations on the existing authenticated Host pipe**.

This document is the implementation authority for the dev.13 status/cancel wire tranche. It is intentionally limited to one-shot authenticated status lookup and queued-job cancellation. It does not add a UI, polling service, persistent session protocol, second pipe, or a new conversion execution path.

## Base authority

Implementation starts from qualified `main` commit:

`8240b23a881362c624eb7c539fd74c0be0c7c159`

The existing product path remains authoritative:

Windows 11 Explorer -> native `IExplorerCommand` -> `Converty.Bridge.exe` -> authenticated Host admission -> strict disposable `Converty.EngineWorker.exe` -> typed preset/provider -> fixed app-local FFmpeg -> private staging -> transactional numbered publication.

Nothing in dev.13 may reroute normal Explorer conversion through a new transport or weaken the existing package/peer/authentication boundaries.

## Goals

1. Let an authenticated Bridge client query the current `JobStatusSnapshot` for a known Host job ID.
2. Let an authenticated Bridge client cancel a job only while its authoritative Host state is `Queued`.
3. Reuse the existing current-user named-pipe endpoint and connected-server identity verification.
4. Preserve the existing conversion-admission request and response shape unchanged.
5. Preserve strict JSON parsing, bounded frames, strict UTF-8, duplicate-member rejection, exact schema-version checks, and current-user peer authorization.
6. Provide deterministic typed failure semantics for unknown jobs, non-cancellable jobs, and cancellation persistence failures.
7. Add test-first coverage proving the new wire behavior and proving that Bridge still authenticates the connected Host before writing any application frame.

## Non-goals

- No second management pipe.
- No replacement universal envelope for legacy conversion admission.
- No persistent connection or subscription protocol.
- No polling loop, background status service, tray UI, Explorer progress UI, or notifications.
- No cancellation of jobs that have entered probing/planning/staging/converting/validating/committing or any terminal state.
- No process termination as a cancellation mechanism.
- No changes to FFmpeg/provider selection, staging, publication, collision handling, or source/destination preservation.
- No change to production FFmpeg redistribution/signing policy.
- No headed Windows 11 Explorer acceptance claim.

## Existing primitives to reuse

### `JobStatusSnapshot`

The existing bounded job status contract remains the single status payload. It already validates:

- current `schemaVersion`;
- non-empty `jobId` and `requestId`;
- defined `ConversionJobState`;
- optional finite `progress` in `[0, 1]`;
- optional trimmed non-whitespace `message` bounded to 1024 characters.

No parallel status model will be introduced.

### `HostJobQueue`

The existing queue methods remain authoritative:

- `TryGet(Guid jobId, out JobStatusSnapshot? status)`;
- `TryCancel(Guid jobId, out JobStatusSnapshot? status)`.

Current cancellation semantics are retained:

- unknown/empty job -> failure with no status;
- existing non-`Queued` job -> failure with current status;
- `Queued` job + journal commit succeeds -> persisted replacement snapshot with state `Cancelled` and message `Cancelled before execution.`;
- `Queued` job + journal commit fails -> failure with the unchanged current queued status.

The wire layer must classify these outcomes without changing queue behavior.

## Typed control contracts

Add small engine-independent contracts under `Converty.Contracts.Jobs`:

- `JobControlOperation`: `Status`, `Cancel`;
- `JobControlFailureReason`: `JobNotFound`, `NotCancellable`, `PersistenceFailure`;
- `JobControlRequest`;
- `JobControlResponse`.

`JobControlRequest` contains:

- current `schemaVersion`;
- one defined operation;
- one non-empty `jobId`.

`JobControlResponse` contains:

- current `schemaVersion`;
- echoed operation;
- echoed non-empty `jobId`;
- `succeeded`;
- optional `JobStatusSnapshot`;
- optional `JobControlFailureReason`.

Cross-field invariants are enforced by the domain contract, not left to callers:

- success requires a status, forbids a failure reason, and requires `status.JobId == jobId`;
- failure requires a failure reason;
- `JobNotFound` requires no status;
- `NotCancellable` and `PersistenceFailure` require a current status whose `JobId` matches `jobId`.

Operation-specific legality is checked at the Host/Bridge boundary: status may fail only with `JobNotFound`; cancel may fail with `JobNotFound`, `NotCancellable`, or `PersistenceFailure`.

## Wire request

Canonical request examples:

```json
{"schemaVersion":1,"operation":"status","jobId":"00000000-0000-0000-0000-000000000001"}
```

```json
{"schemaVersion":1,"operation":"cancel","jobId":"00000000-0000-0000-0000-000000000001"}
```

Requirements:

- `schemaVersion` must equal `SchemaVersions.Current`;
- `operation` is exactly lowercase `status` or `cancel` on the wire;
- serializers emit `jobId` in canonical `D` format;
- the control parser accepts only canonical `D`-format non-empty job IDs;
- unknown, duplicate, missing, case-variant, or extra members are rejected;
- trailing commas/comments remain rejected;
- normal conversion requests keep their current JSON shape and serializer unchanged.

The stricter canonical GUID rule applies only to the new control wire and must not silently change existing conversion contract parsing.

## Request dispatch

`HostRequestHandler` remains the one application request dispatcher on the existing pipe.

After current peer authorization, byte-bound checks, and strict UTF-8 decoding, it inspects the top-level JSON object only far enough to determine whether the explicit `operation` member is present.

Routing rule:

- top-level `operation` present -> parse strictly as `JobControlRequest`;
- top-level `operation` absent -> parse strictly using the existing `ConversionRequest` path.

A hybrid object containing `operation` plus conversion-admission members is routed to the control parser and rejected because the control contract disallows unknown members. A conversion object with unknown members continues to be rejected by the existing conversion parser.

Malformed JSON that cannot be classified is rejected as `invalidRequest`; no attempt is made to infer an operation from damaged input.

## Wire response

The existing conversion admission response remains schema-compatible and unchanged:

- `schemaVersion`;
- `accepted`;
- optional `jobId`;
- optional `reason`.

Job-control operations use the separate strict `JobControlResponse` shape.

Status success example:

```json
{
  "schemaVersion":1,
  "operation":"status",
  "succeeded":true,
  "jobId":"00000000-0000-0000-0000-000000000001",
  "status":{
    "schemaVersion":1,
    "jobId":"00000000-0000-0000-0000-000000000001",
    "requestId":"00000000-0000-0000-0000-000000000002",
    "state":"queued"
  }
}
```

Cancel failure example:

```json
{
  "schemaVersion":1,
  "operation":"cancel",
  "succeeded":false,
  "jobId":"00000000-0000-0000-0000-000000000001",
  "reason":"notCancellable",
  "status":{
    "schemaVersion":1,
    "jobId":"00000000-0000-0000-0000-000000000001",
    "requestId":"00000000-0000-0000-0000-000000000002",
    "state":"converting",
    "progress":0.5
  }
}
```

Null optional members follow the existing `ContractJson` rule and are omitted rather than serialized as JSON `null`.

Response invariants:

- `schemaVersion` must be current;
- `operation` must echo the requested operation;
- `jobId` must echo the requested non-empty job ID;
- successful responses contain one valid status and no reason;
- returned `status.JobId` must equal the echoed/requested `jobId`;
- failed responses contain one defined reason and either the current status when required by that reason or no status for `jobNotFound`;
- duplicate, unknown, case-variant, contradictory, or extra response members are rejected;
- Bridge verifies operation and job-ID correlation, preventing a valid response for one request from being accepted for another.

The nested status object is the existing `JobStatusSnapshot` wire representation, not a duplicate status schema.

## Operation semantics

### `status`

Host calls `HostJobQueue.TryGet(jobId, out status)`.

- found -> success with current status;
- not found -> failure `jobNotFound`, no status.

### `cancel`

Host calls `HostJobQueue.TryCancel(jobId, out status)`.

- returns `true` -> success; returned status must be `Cancelled`;
- returns `false`, `status == null` -> `jobNotFound`;
- returns `false`, current status exists and state is not `Queued` -> `notCancellable`, current status included;
- returns `false`, current status exists and state is `Queued` -> `persistenceFailure`, unchanged queued status included.

No other failure reason is inferred from queue state.

## Authentication and transport

Every status or cancel operation uses a fresh `NamedPipeClientStream`, matching current submission behavior.

The Bridge sequence is mandatory:

1. connect to the existing current-user pipe;
2. call `IConnectedServerIdentityVerifier.VerifyConnectedServer(pipe)`;
3. only after successful verification, serialize/write the first application frame;
4. read one bounded response frame;
5. strictly validate schema, operation, job ID, success/failure invariants, and nested status.

If server identity verification fails, zero application request bytes may be written.

The Host continues to enforce expected-user peer authorization before accepting application semantics. No status/cancel method may bypass either side of this authentication model.

## Bridge API shape

Keep `IBridgeRequestClient.SubmitAsync` and all existing callers unchanged.

Add an independently testable job-control interface implemented by the existing concrete `BridgeClient`, with one-shot methods equivalent to:

- `GetStatusAsync(Guid jobId, CancellationToken)`;
- `CancelAsync(Guid jobId, CancellationToken)`.

Both return the typed `JobControlResponse` after verifying that its operation and job ID match the request that was sent.

No retry loop or background polling is added in this tranche.

## Serialization

Extend `Converty.Serialization.ContractJson` with strict serialization/deserialization for both `JobControlRequest` and `JobControlResponse`, using the same global rules already used by conversion and status contracts:

- camel-case wire names;
- case-sensitive property names;
- `JsonUnmappedMemberHandling.Disallow`;
- duplicate-property rejection;
- no comments/trailing commas;
- bounded max depth;
- exact schema dispatch;
- domain validation mapped to `JsonException`;
- null optional members omitted.

Add V1 wire DTOs and explicit enum-to-wire mappings for control operations/failure reasons. Reuse the existing `JobStatusSnapshot` mapping for nested status data; do not create a second status representation or weaken global serializer options.

## Error handling

- Authorization failure remains fail-closed and is not used to reveal job existence.
- Oversized/empty/invalid UTF-8/malformed requests remain `invalidRequest` at the Host boundary.
- If malformed input cannot be reliably classified as control versus legacy admission, Host may use the existing generic invalid-request rejection; authenticated Bridge-generated control requests are always classifiable, so Bridge treats a non-control response to a control request as a protocol error.
- Bridge treats malformed, oversized, schema-mismatched, operation-mismatched, job-ID-mismatched, duplicate-member, or internally contradictory control responses as protocol errors rather than fabricating a status.
- Transport connect/read/write timeouts remain bounded by the existing 30-second maxima.
- Cancellation persistence failure must not mutate the in-memory job to `Cancelled`.
- Unexpected internal exceptions are not translated into successful control results.

## Test-first implementation requirements

Add RED tests before production changes for at least:

1. strict request round trip for `status` and `cancel`;
2. strict response round trip for success and each defined failure;
3. request rejection for missing/unknown/duplicate/case-variant members, unsupported operation, empty/non-canonical/invalid job ID, and unsupported schema;
4. response rejection for malformed cross-field combinations, operation mismatch, job-ID mismatch, duplicate/unknown members, and invalid nested status;
5. Host status success;
6. Host status unknown -> `jobNotFound`;
7. Host queued cancellation success -> persisted `Cancelled` snapshot;
8. Host cancel unknown -> `jobNotFound`;
9. Host cancel existing non-queued -> `notCancellable` with current status;
10. Host cancel journal failure -> `persistenceFailure` with unchanged queued status;
11. Bridge server-identity verification occurs before any status application frame is written;
12. Bridge server-identity verification occurs before any cancel application frame is written;
13. existing conversion `SubmitAsync` admission behavior remains unchanged and green;
14. existing product conversion, package/COM activation, strict-worker isolation, FFmpeg, collision, Unicode/metacharacter, source-preservation, and static/security gates remain green.

Where the repository already has fuzz/static contract corpora covering wire contracts, add the new grammar without relaxing existing vectors.

## Versioning and qualification

The tranche becomes `0.1.0-dev.13` only as part of the actual status/cancel implementation. The design-only commits do not claim dev.13 runtime qualification.

Qualification sequence:

1. preserve RED evidence for the new tests;
2. implement the minimal production changes;
3. run ordinary Windows CI on the exact behavior SHA;
4. require all managed/native/package/COM/product/static/security gates to pass;
5. synchronize deterministic runner-generated authority only if source changes make tracked authority stale;
6. if generated authority is synchronized by a bot commit that cannot recursively trigger CI, create an empty same-tree qualification commit following the established repository pattern;
7. final completion requires ordinary CI success on the exact current `main` HEAD plus current/zero-diff generated authority;
8. record run/job IDs, artifacts, digests, workspace ZIP evidence, and exact SHAs in the handover.

## Shipping blockers deliberately left open

This tranche does not close:

- headed modern Windows 11 Explorer visual acceptance;
- exact-build screenshots;
- Explorer crash/hang/failure matrix;
- replay/disconnect/reconnect/session acceptance beyond the one-shot operations specified here;
- production FFmpeg redistribution/license/notices/signature/hash approval;
- signed MSIX;
- clean Windows 11 VM install/update/uninstall;
- final security/fuzz/chaos/release audit;
- end-user acceptance.

## Acceptance statement

Dev.13 status/cancel wire work is complete only when the exact current `main` authority proves, in ordinary Windows CI, that authenticated one-shot status and queued-job cancellation work with strict correlated responses while the existing Explorer conversion path and all previously qualified product/security invariants remain green.
