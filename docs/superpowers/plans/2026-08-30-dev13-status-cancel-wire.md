# Converty dev.13 Status/Cancel Wire Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add strict authenticated one-shot Host job status lookup and queued-job cancellation without changing the existing conversion-admission request/response or the Explorer -> Bridge -> strict worker/provider -> FFmpeg product path.

**Architecture:** Reuse the existing current-user named pipe, Host queue, journal, bounded framing, and Bridge connected-server identity verification. Add engine-independent typed control contracts in `Converty.Contracts`, strict V1 JSON adapters in `Converty.Serialization`, dispatch/control semantics in `HostRequestHandler`, and a separate `IBridgeJobControlClient` surface implemented by the existing `BridgeClient`. Every control call uses a fresh pipe connection and authenticates the connected Host before any application frame is written.

**Tech Stack:** C# 14 / .NET 10.0.400, `System.IO.Pipes`, `System.Text.Json`, xUnit v3 + Microsoft Testing Platform, Python 3.13 static/fuzz gates, GitHub Actions `windows-2025` plus Ubuntu static/continuity jobs.

**Spec:** `docs/superpowers/specs/2026-08-30-dev13-status-cancel-wire-design.md`

## Global Constraints

- Start from the live `main` authority and re-fetch `main` before every write and every completion/freeze claim.
- Preserve the existing conversion admission JSON shape and `IBridgeRequestClient.SubmitAsync` behavior.
- Preserve `HostPipeServer`; do not add a second named pipe or persistent session protocol.
- Preserve Host/Bridge media neutrality: no FFmpeg/ffprobe/media parsing/provider loading in Host or Bridge.
- Preserve the existing connected-server authentication order: connect -> verify Host identity -> write first application frame.
- Preserve current-user Host peer authorization before Host reads application semantics.
- Preserve bounded 1 MiB application requests, bounded protocol framing, finite 30-second transport timeouts, strict UTF-8, strict schema versioning, duplicate rejection, unknown-member rejection, no comments/trailing commas, and case-sensitive wire members.
- New control `jobId` parsing is canonical `D` format only. Do not tighten legacy `ConversionRequest` GUID parsing in this tranche.
- Status uses the existing `JobStatusSnapshot`; do not create a parallel status model.
- Cancellation is queued-only and transactional through the existing `HostJobQueue.TryCancel`; do not terminate running workers as a cancellation mechanism.
- Keep Gyan FFmpeg development-only. Do not claim production redistribution approval, signed MSIX qualification, clean-VM qualification, or headed Windows 11 Explorer acceptance.
- Preserve failed RED workflow evidence and never call a side-branch or stale generated-authority run final repository authority.

---

### Task 1: Typed job-control contracts and strict V1 JSON

**Files:**
- Create: `src/Converty.Contracts/Jobs/JobControlOperation.cs`
- Create: `src/Converty.Contracts/Jobs/JobControlFailureReason.cs`
- Create: `src/Converty.Contracts/Jobs/JobControlRequest.cs`
- Create: `src/Converty.Contracts/Jobs/JobControlResponse.cs`
- Create: `tests/Converty.Contracts.Tests/Jobs/JobControlContractTests.cs`
- Create: `tests/Converty.Serialization.Tests/JobControlJsonTests.cs`
- Modify: `src/Converty.Serialization/V1/WireModels.cs`
- Modify: `src/Converty.Serialization/V1/WireEnumText.cs`
- Modify: `src/Converty.Serialization/ContractJson.cs`

**Interfaces:**
- `JobControlOperation.Status`, `JobControlOperation.Cancel`.
- `JobControlFailureReason.JobNotFound`, `NotCancellable`, `PersistenceFailure`.
- `new JobControlRequest(int schemaVersion, JobControlOperation operation, Guid jobId)`.
- `new JobControlResponse(int schemaVersion, JobControlOperation operation, Guid jobId, bool succeeded, JobStatusSnapshot? status, JobControlFailureReason? reason)`.
- `ContractJson.Serialize(JobControlRequest)` / `DeserializeJobControlRequest(string)`.
- `ContractJson.Serialize(JobControlResponse)` / `DeserializeJobControlResponse(string)`.

- [ ] **Step 1: Add domain RED tests before production types exist.**

Create focused tests proving current schema/defined enum/non-empty job ID requirements and response cross-field invariants. The essential assertions are:

```csharp
[Fact]
public void SuccessfulResponseRequiresMatchingStatusAndNoReason()
{
    Guid jobId = Guid.NewGuid();
    var status = new JobStatusSnapshot(
        SchemaVersions.Current,
        jobId,
        Guid.NewGuid(),
        ConversionJobState.Queued,
        progress: null,
        message: null);

    var result = new JobControlResponse(
        SchemaVersions.Current,
        JobControlOperation.Status,
        jobId,
        succeeded: true,
        status,
        reason: null);

    Assert.True(result.Succeeded);
    Assert.Same(status, result.Status);
}

[Fact]
public void JobNotFoundFailureForbidsStatus()
{
    Guid jobId = Guid.NewGuid();
    var status = new JobStatusSnapshot(
        SchemaVersions.Current,
        jobId,
        Guid.NewGuid(),
        ConversionJobState.Queued,
        null,
        null);

    Assert.Throws<ArgumentException>(() => new JobControlResponse(
        SchemaVersions.Current,
        JobControlOperation.Status,
        jobId,
        succeeded: false,
        status,
        JobControlFailureReason.JobNotFound));
}
```

Also test undefined operation/reason values, empty job ID, success without status, success with reason, failure without reason, mismatched status job ID, and `NotCancellable`/`PersistenceFailure` without status.

- [ ] **Step 2: Add serialization RED tests.**

Required canonical cases:

```csharp
[Theory]
[InlineData(JobControlOperation.Status, "status")]
[InlineData(JobControlOperation.Cancel, "cancel")]
public void JobControlRequestRoundTripsCanonicalWireText(
    JobControlOperation operation,
    string wireOperation)
{
    Guid jobId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    var request = new JobControlRequest(SchemaVersions.Current, operation, jobId);

    string json = ContractJson.Serialize(request);
    JobControlRequest result = ContractJson.DeserializeJobControlRequest(json);

    Assert.Equal(operation, result.Operation);
    Assert.Equal(jobId, result.JobId);
    Assert.Contains($"\"operation\":\"{wireOperation}\"", json, StringComparison.Ordinal);
    Assert.Contains("\"jobId\":\"11111111-1111-1111-1111-111111111111\"", json, StringComparison.Ordinal);
}
```

Add success and all three failure response round trips. Add strict rejection tests for missing/unknown/duplicate/case-variant members, unsupported schema, unsupported operation/reason, empty job ID, invalid job ID, non-`D` job ID such as `{11111111-1111-1111-1111-111111111111}`, invalid nested status, and contradictory response fields. Verify omitted optionals are absent, not JSON `null`.

- [ ] **Step 3: Commit/push the RED tests and preserve the failing ordinary CI run.**

Expected RED: managed build/test fails because `JobControl*` production types and serializers do not yet exist. The static generated-authority gate may also become stale because tracked test bytes changed; record both facts without treating generated-authority staleness as the behavioral RED.

- [ ] **Step 4: Add the minimal domain contracts.**

`JobControlOperation.cs`:

```csharp
namespace Converty.Contracts.Jobs;

public enum JobControlOperation
{
    Status = 0,
    Cancel = 1,
}
```

`JobControlFailureReason.cs`:

```csharp
namespace Converty.Contracts.Jobs;

public enum JobControlFailureReason
{
    JobNotFound = 0,
    NotCancellable = 1,
    PersistenceFailure = 2,
}
```

`JobControlRequest` validates current schema, `Enum.IsDefined(operation)`, and non-empty job ID.

`JobControlResponse` validates current schema, defined operation/reason values, non-empty job ID, and these exact cross-field rules:

```csharp
if (succeeded)
{
    if (status is null || reason is not null || status.JobId != jobId)
        throw new ArgumentException("Successful job-control responses require matching status and no reason.");
}
else
{
    if (reason is null || !Enum.IsDefined(reason.Value))
        throw new ArgumentException("Failed job-control responses require a defined reason.");

    bool statusRequired = reason is JobControlFailureReason.NotCancellable
        or JobControlFailureReason.PersistenceFailure;
    if ((statusRequired && status is null) ||
        (!statusRequired && status is not null) ||
        (status is not null && status.JobId != jobId))
        throw new ArgumentException("Job-control failure status does not match its reason/job ID.");
}
```

- [ ] **Step 5: Extend strict V1 wire DTOs and enum text.**

Add DTOs with nullable `Succeeded` so a missing boolean is distinguishable from explicit `false`:

```csharp
internal sealed class JobControlRequestWire
{
    public int SchemaVersion { get; set; }
    public string? Operation { get; set; }
    public string? JobId { get; set; }
}

internal sealed class JobControlResponseWire
{
    public int SchemaVersion { get; set; }
    public string? Operation { get; set; }
    public string? JobId { get; set; }
    public bool? Succeeded { get; set; }
    public JobStatusSnapshotWire? Status { get; set; }
    public string? Reason { get; set; }
}
```

Add explicit mappings in `WireEnumText`:

```csharp
internal static string ToWire(JobControlOperation value) => value switch
{
    JobControlOperation.Status => "status",
    JobControlOperation.Cancel => "cancel",
    _ => throw new ArgumentOutOfRangeException(nameof(value), "Unsupported job control operation."),
};

internal static JobControlOperation ParseJobControlOperation(string? value) => value switch
{
    "status" => JobControlOperation.Status,
    "cancel" => JobControlOperation.Cancel,
    _ => throw new JsonException("Invalid job control operation wire value."),
};
```

Do the same for `jobNotFound`, `notCancellable`, and `persistenceFailure`.

- [ ] **Step 6: Extend `ContractJson` without changing global serializer options or legacy GUID parsing.**

Add a control-only GUID parser:

```csharp
private static Guid ParseCanonicalGuid(string? value, string propertyName)
{
    if (!Guid.TryParseExact(value, "D", out Guid parsed) || parsed == Guid.Empty)
        throw new JsonException($"Property {propertyName} must be a non-empty canonical UUID.");
    return parsed;
}
```

Keep existing `ParseGuid` unchanged. Add request/response serialize/deserialize methods using existing `Dispatch`, `RejectDuplicateProperties`, `DeserializeWire<TWire>`, and domain mapping. Refactor status wire conversion only as needed into private `ToWire(JobStatusSnapshot)` / `FromWire(JobStatusSnapshotWire)` helpers while preserving existing serialized `JobStatusSnapshot` bytes.

- [ ] **Step 7: Run the focused GREEN checks, then commit/push.**

On Windows/.NET 10.0.400 after locked restore:

```powershell
./build/bootstrap.ps1
./build/build.ps1 -Configuration Release
dotnet test tests/Converty.Contracts.Tests/Converty.Contracts.Tests.csproj --configuration Release --no-build --no-restore
dotnet test tests/Converty.Serialization.Tests/Converty.Serialization.Tests.csproj --configuration Release --no-build --no-restore
```

Expected: no warnings/errors and all Contracts/Serialization tests green. Existing conversion serialization tests remain unchanged and green.

---

### Task 2: Host request dispatch, status lookup, and transactional queued cancellation

**Files:**
- Create: `tests/Converty.Host.Tests/Ipc/HostJobControlHandlerTests.cs`
- Modify: `src/Converty.Host/Ipc/HostRequestHandler.cs`
- Do not modify: `src/Converty.Host/Jobs/HostJobQueue.cs` unless a test proves an existing queue contract defect.
- Do not modify: `src/Converty.Host/Ipc/HostPipeServer.cs`.

**Interfaces:**
- Existing `HostRequestHandler.HandleAsync(...)` accepts both legacy admission requests and explicit job-control requests.
- Existing legacy admission response remains `{schemaVersion,accepted,jobId?,reason?}`.
- Valid control requests return serialized `JobControlResponse`.

- [ ] **Step 1: Add Host RED tests for all control outcomes.**

Cover:
1. status found -> success/current snapshot;
2. status unknown -> `JobNotFound` and no status;
3. queued cancel -> success/`Cancelled`, queue now stores Cancelled;
4. cancel unknown -> `JobNotFound`;
5. cancel non-queued -> `NotCancellable` with current status;
6. cancellation journal write failure -> `PersistenceFailure` with unchanged `Queued` status;
7. hybrid control+conversion JSON -> existing generic admission rejection `invalidRequest` and no queue mutation;
8. malformed/unsupported control JSON -> generic `invalidRequest` and no queue mutation;
9. legacy conversion admission test remains unchanged.

Use a deterministic journal double for the non-queued and persistence cases:

```csharp
private sealed class TestJournal(
    IReadOnlyList<JobStatusSnapshot> recovered,
    int failOnCommit = -1) : IHostJobJournal
{
    private int _commitCount;

    public IReadOnlyList<JobStatusSnapshot> LoadForRecovery() => recovered;

    public void Commit(IReadOnlyCollection<JobStatusSnapshot> snapshots)
    {
        _commitCount++;
        if (_commitCount == failOnCommit)
            throw new IOException("Injected journal failure.");
    }
}
```

For persistence failure, initialize with no recovered jobs, admit one request successfully on commit 1, then configure failure on commit 2 so `TryCancel` returns false with the same queued snapshot.

- [ ] **Step 2: Commit/push the Host RED and preserve its failing CI run.**

Expected behavioral RED: current `HostRequestHandler` tries to deserialize the control object as `ConversionRequest` and returns the legacy `invalidRequest` admission rejection instead of typed status/cancel results.

- [ ] **Step 3: Add strict request classification after authorization/size/UTF-8 validation.**

Keep authorization first. After strict UTF-8 decode, parse only enough JSON to route:

```csharp
private static bool ContainsControlOperation(string json)
{
    using JsonDocument document = JsonDocument.Parse(json, new JsonDocumentOptions
    {
        AllowTrailingCommas = false,
        CommentHandling = JsonCommentHandling.Disallow,
        MaxDepth = 32,
    });

    return document.RootElement.ValueKind == JsonValueKind.Object &&
        document.RootElement.TryGetProperty("operation", out _);
}
```

If `operation` is present, call `ContractJson.DeserializeJobControlRequest`; otherwise call existing `DeserializeConversionRequest`. Duplicate/unknown/hybrid members are still rejected by the strict contract parser. Any JSON/UTF-8 parse error returns the existing generic `invalidRequest` admission response.

- [ ] **Step 4: Implement typed status handling by delegating only to `HostJobQueue.TryGet`.**

```csharp
private byte[] HandleStatus(JobControlRequest request)
{
    if (_queue.TryGet(request.JobId, out JobStatusSnapshot? status) && status is not null)
    {
        return Encoding.UTF8.GetBytes(ContractJson.Serialize(new JobControlResponse(
            SchemaVersions.Current,
            JobControlOperation.Status,
            request.JobId,
            succeeded: true,
            status,
            reason: null)));
    }

    return Encoding.UTF8.GetBytes(ContractJson.Serialize(new JobControlResponse(
        SchemaVersions.Current,
        JobControlOperation.Status,
        request.JobId,
        succeeded: false,
        status: null,
        JobControlFailureReason.JobNotFound)));
}
```

Use an equivalent direct UTF-8 serialization approach consistent with existing code; do not add transport concerns to the handler.

- [ ] **Step 5: Implement typed cancel classification by delegating only to `HostJobQueue.TryCancel`.**

Required mapping:

```csharp
bool cancelled = _queue.TryCancel(request.JobId, out JobStatusSnapshot? status);
if (cancelled)
{
    if (status is null || status.State != ConversionJobState.Cancelled)
        throw new InvalidOperationException("Successful cancellation did not return Cancelled status.");
    return SerializeControlSuccess(request, status);
}

JobControlFailureReason reason = status switch
{
    null => JobControlFailureReason.JobNotFound,
    { State: ConversionJobState.Queued } => JobControlFailureReason.PersistenceFailure,
    _ => JobControlFailureReason.NotCancellable,
};
return SerializeControlFailure(request, status, reason);
```

Do not alter `HostJobQueue` state transitions. A persistence failure must leave the queued snapshot unchanged.

- [ ] **Step 6: Run focused Host GREEN checks, then commit/push.**

```powershell
./build/bootstrap.ps1
./build/build.ps1 -Configuration Release
dotnet test tests/Converty.Host.Tests/Converty.Host.Tests.csproj --configuration Release --no-build --no-restore
```

Expected: all Host tests green, including existing admission, pipe server, journal, queue, and fuzz tests.

---

### Task 3: Authenticated Bridge status/cancel client and correlated response validation

**Files:**
- Create: `src/Converty.Bridge/Ipc/IBridgeJobControlClient.cs`
- Create: `tests/Converty.Bridge.Tests/Ipc/BridgeJobControlClientTests.cs`
- Modify: `src/Converty.Bridge/Ipc/BridgeClient.cs`
- Keep unchanged: `src/Converty.Bridge/Ipc/IBridgeRequestClient.cs`
- Keep unchanged: all existing callers of `IBridgeRequestClient.SubmitAsync`.

**Interfaces:**

```csharp
public interface IBridgeJobControlClient
{
    Task<JobControlResponse> GetStatusAsync(Guid jobId, CancellationToken cancellationToken = default);
    Task<JobControlResponse> CancelAsync(Guid jobId, CancellationToken cancellationToken = default);
}
```

`BridgeClient` implements both `IBridgeRequestClient` and `IBridgeJobControlClient`.

- [ ] **Step 1: Add Bridge RED tests for status/cancel before production methods exist.**

Required tests:
1. `GetStatusAsync` sends a strict `status` request and accepts a correlated status response;
2. `CancelAsync` sends `cancel` and accepts correlated Cancelled response;
3. status identity rejection closes with zero application bytes;
4. cancel identity rejection closes with zero application bytes;
5. response operation mismatch -> `InvalidDataException`;
6. response job-ID mismatch -> `InvalidDataException`;
7. status operation receiving `NotCancellable`/`PersistenceFailure` -> `InvalidDataException`;
8. cancel success whose status is not `Cancelled` -> `InvalidDataException`;
9. cancel `PersistenceFailure` requires queued status;
10. cancel `NotCancellable` requires non-queued status;
11. malformed/unknown/duplicate response members and invalid nested status -> `InvalidDataException`;
12. existing `SubmitAsyncVerifiesConnectedServerBeforeFirstApplicationFrame` and `IdentityRejectionWritesNoApplicationFrame` remain green.

A server helper should deserialize the frame as `JobControlRequest` and verify expected operation/job ID before writing the supplied response.

- [ ] **Step 2: Commit/push Bridge RED and preserve its failing ordinary CI run.**

Expected behavioral RED: `IBridgeJobControlClient`, `GetStatusAsync`, and `CancelAsync` do not yet exist.

- [ ] **Step 3: Add the separate interface and a shared authenticated one-request exchange in `BridgeClient`.**

The helper may eliminate connection boilerplate, but the security order is non-negotiable:

```csharp
private async Task<ReadOnlyMemory<byte>> ExchangeAuthenticatedAsync(
    byte[] requestPayload,
    CancellationToken cancellationToken)
{
    await using var pipe = new NamedPipeClientStream(
        ".",
        PipeName,
        PipeDirection.InOut,
        PipeOptions.Asynchronous,
        TokenImpersonationLevel.Impersonation);

    try
    {
        await pipe.ConnectAsync(_connectTimeout, cancellationToken);
    }
    catch (TimeoutException error)
    {
        throw new BridgeHostUnavailableException(
            "Converty Host did not accept the pipe connection before the connect timeout.", error);
    }
    catch (IOException error)
    {
        throw new BridgeHostUnavailableException("Converty Host pipe connection is unavailable.", error);
    }

    _serverIdentityVerifier.VerifyConnectedServer(pipe);
    await BoundedProtocolFrameIo.WriteAndFlushAsync(pipe, requestPayload, _connectTimeout, cancellationToken);
    ProtocolFrame response = await BoundedProtocolFrameIo.ReadAsync(pipe, _connectTimeout, cancellationToken);
    return response.Payload;
}
```

If `SubmitAsync` is refactored to use this helper, preserve the same exception mapping and admission parser behavior. Authentication failure must escape before the write call.

- [ ] **Step 4: Add `GetStatusAsync` and `CancelAsync`.**

Both construct a validated `JobControlRequest`, serialize through `ContractJson`, perform one authenticated exchange, strictly decode UTF-8, deserialize a `JobControlResponse`, and call a correlation validator.

```csharp
public Task<JobControlResponse> GetStatusAsync(Guid jobId, CancellationToken cancellationToken = default) =>
    SendJobControlAsync(JobControlOperation.Status, jobId, cancellationToken);

public Task<JobControlResponse> CancelAsync(Guid jobId, CancellationToken cancellationToken = default) =>
    SendJobControlAsync(JobControlOperation.Cancel, jobId, cancellationToken);
```

- [ ] **Step 5: Enforce operation/job/state correlation after strict contract parsing.**

```csharp
private static void ValidateControlResponse(
    JobControlRequest request,
    JobControlResponse response)
{
    if (response.Operation != request.Operation || response.JobId != request.JobId)
        throw new InvalidDataException("Job-control response does not match the request.");

    if (request.Operation == JobControlOperation.Status)
    {
        if (!response.Succeeded && response.Reason != JobControlFailureReason.JobNotFound)
            throw new InvalidDataException("Status response used an invalid failure reason.");
        return;
    }

    if (response.Succeeded)
    {
        if (response.Status?.State != ConversionJobState.Cancelled)
            throw new InvalidDataException("Successful cancel response must be Cancelled.");
        return;
    }

    if (response.Reason == JobControlFailureReason.PersistenceFailure &&
        response.Status?.State != ConversionJobState.Queued)
        throw new InvalidDataException("Persistence failure must return unchanged queued status.");

    if (response.Reason == JobControlFailureReason.NotCancellable &&
        response.Status?.State == ConversionJobState.Queued)
        throw new InvalidDataException("Queued cancel failure must be persistenceFailure.");
}
```

Strictly decode response UTF-8 with `new UTF8Encoding(false, true)` before `ContractJson.DeserializeJobControlResponse`.

- [ ] **Step 6: Run focused Bridge GREEN checks, then commit/push.**

```powershell
./build/bootstrap.ps1
./build/build.ps1 -Configuration Release
dotnet test tests/Converty.Bridge.Tests/Converty.Bridge.Tests.csproj --configuration Release --no-build --no-restore
```

Expected: all Bridge tests green, including existing submission authentication ordering.

---

### Task 4: Adversarial corpus and static architectural gates

**Files:**
- Modify: `tests/fuzz/ipc/v1/corpus.json`
- Modify: `tests/Converty.Host.Tests/Ipc/IpcFuzzCorpusTests.cs`
- Modify: `tests/static/test_dev6_b2_ipc.py`
- Create: `tests/static/test_dev13_status_cancel_wire.py`

**Interfaces:**
- Existing frame fuzz cases remain unchanged.
- Invalid job-control request corpus entries must return generic `invalidRequest` and leave queue count zero.
- Static gates encode the approved single-pipe/authentication/product-neutrality architecture.

- [ ] **Step 1: Add malformed job-control corpus cases.**

Extend the existing seven-case corpus with exactly these five request-text IDs:

- `control-unknown-member`
- `control-duplicate-operation`
- `control-noncanonical-job-id`
- `control-future-schema`
- `control-hybrid-conversion-members`

Examples:

```json
{"id":"control-unknown-member","kind":"requestText","data":"{\"schemaVersion\":1,\"operation\":\"status\",\"jobId\":\"11111111-1111-1111-1111-111111111111\",\"command\":\"forbidden\"}","expectReason":"invalidRequest"}
```

and the non-canonical GUID must use braces so it is parseable as a GUID generally but rejected by the new `D`-only control parser.

Update `IpcFuzzCorpusTests` expected count from 7 to 12. Update the exact ID set in `test_dev6_b2_ipc.py` so the historical B2 gate now recognizes the expanded corpus rather than falsely failing because it was deliberately strengthened.

- [ ] **Step 2: Add dev.13 static tests before relying on implementation.**

`test_dev13_status_cancel_wire.py` must assert at least:

```python
def test_dev13_uses_existing_pipe_and_separate_bridge_control_interface() -> None:
    bridge = text("src/Converty.Bridge/Ipc/BridgeClient.cs")
    interface = text("src/Converty.Bridge/Ipc/IBridgeJobControlClient.cs")
    host = text("src/Converty.Host/Ipc/HostRequestHandler.cs")

    assert "IBridgeJobControlClient" in bridge
    assert "GetStatusAsync" in interface and "CancelAsync" in interface
    assert "TryGet" in host and "TryCancel" in host
    assert bridge.index("VerifyConnectedServer") < bridge.index("BoundedProtocolFrameIo.WriteAndFlushAsync")
```

Also assert:
- `IBridgeRequestClient.cs` still exposes only `SubmitAsync`;
- no second Host pipe class/file was added for control operations;
- `HostPipeServer.cs` still validates `_peerValidator.IsExpectedUser` before `BoundedProtocolFrameIo.ReadAsync`;
- Host/Bridge remain free of FFmpeg/ffprobe and unapproved process execution outside existing Bridge startup boundary;
- `JobControlRequest`/`JobControlResponse` files and serializers exist;
- `JobStatusSnapshot` remains the status type used by the control response.

- [ ] **Step 3: Commit/push the adversarial/static RED if the new static gate is written before all production tokens exist; otherwise preserve the earlier behavioral REDs and keep this commit purely strengthening.**

Do not manufacture a failing run after the behavior is already proven. The mandatory RED evidence is Tasks 1-3.

- [ ] **Step 4: Run full managed/static regression locally where available.**

```powershell
./build/bootstrap.ps1
./build/dependency-audit.ps1
./build/build.ps1 -Configuration Release
./build/test.ps1 -Configuration Release
```

And independently:

```bash
python scripts/verify_contract_vectors.py
python scripts/verify_repository.py
python -m pytest -q tests/static
```

Expected: all managed tests, existing 5 conversion-request contract vectors, repository verifier, expanded fuzz test, and all Python static gates pass. Tracked generated authority is allowed to be stale at this behavior-development point and must be recorded separately.

---

### Task 5: Dev.13 version/authority docs and exact behavior qualification

**Files:**
- Modify: `VERSION`
- Modify: `README.md`
- Modify: `CHANGELOG.md`
- Modify: `eng/toolchain.json`
- Modify: `docs/development/IMPLEMENTATION_STATUS.md`
- Modify: `docs/TASK_BACKLOG.md`
- Modify: `docs/HANDOVER_NEXT_AGENT.md`
- Modify: `docs/HANDOVER_PROMPT.txt`
- Modify: `machine-readable/handover_state.json`
- Modify: `machine-readable/build_evidence.json`
- Do not hand-edit generated authority files in this task.

**Version:** `0.1.0-dev.13`.

- [ ] **Step 1: Re-fetch live `main`; stop and audit if another actor advanced it.**

The implementation commits must form a fast-forward line from the approved design/plan authority. Do not overwrite or force-update concurrent work.

- [ ] **Step 2: Bump version and human/machine-readable status documents to observed dev.13 behavior only.**

Update `VERSION` and `eng/toolchain.json.workspaceVersion` to `0.1.0-dev.13`. Document:
- typed status/cancel wire on the existing authenticated pipe;
- queued-only transactional cancellation;
- Bridge connected-server verification before first control frame;
- conversion admission unchanged;
- exact RED commits/runs observed so far;
- exact remaining blockers.

Do not pre-fill future workflow IDs, artifact IDs, hashes, test totals, or claims that have not yet been observed. Evidence files should state that final exact-main generated-authority closure is pending until Task 6.

- [ ] **Step 3: Commit/push the dev.13 behavior candidate and let ordinary CI qualify that exact SHA.**

The managed job must prove on Windows Server 2025 / `windows-2025`:
- locked restore;
- NuGet vulnerability audit;
- Release build with zero warnings/errors;
- native Explorer DLL smoke;
- development package / MakeAppx validation;
- direct and registered Explorer COM activation/invocation;
- product Bridge -> strict worker -> FFmpeg smoke;
- Unicode/metacharacter path behavior;
- source/existing destination preservation;
- numbered collision publication;
- MP3 exactly 320000 bit/s;
- all managed tests including new Contracts/Serialization/Host/Bridge control tests.

The static job must prove repository/static/fuzz/vector gates up to the expected generated-authority freshness check.

- [ ] **Step 4: Inspect the workflow logs and record the behavior SHA/run/job IDs and actual counts.**

Do not use “should pass.” If any behavior gate fails, preserve the failing SHA/run and return to the smallest relevant task before changing production code.

- [ ] **Step 5: Treat generated-authority/workspace-integrity failure as expected only if every earlier behavior/static gate is green and the diff is limited to deterministic authority files.**

The four generated authority files are:
- `machine-readable/source_sbom.spdx.json`
- `machine-readable/release_sbom.spdx.json`
- `machine-readable/package_manifest.json`
- `SHA256SUMS.txt`

Do not conflate their expected freshness failure with runtime/test failure.

---

### Task 6: Deterministic generated-authority sync and exact-current-main freeze

**Files:**
- Runner-generated only: `machine-readable/source_sbom.spdx.json`
- Runner-generated only: `machine-readable/release_sbom.spdx.json`
- Runner-generated only: `machine-readable/package_manifest.json`
- Runner-generated only: `SHA256SUMS.txt`
- Temporary one-shot workflow only if required by the established repository pattern: `.github/workflows/dev13-generated-authority-sync.yml`
- Final evidence/handover corrections only if exact observed values changed: `machine-readable/build_evidence.json`, `machine-readable/handover_state.json`, `docs/HANDOVER_NEXT_AGENT.md`, `docs/HANDOVER_PROMPT.txt`, `README.md`, `CHANGELOG.md`, `eng/toolchain.json`, `docs/development/IMPLEMENTATION_STATUS.md`, `docs/TASK_BACKLOG.md`

- [ ] **Step 1: Download the generated-authority artifact from the exact green behavior SHA and verify its artifact digest/ZIP hash before use.**

The sync must be hard-guarded to the exact behavior parent SHA and exact artifact ID/hash. Copy only the four runner-generated files listed above.

- [ ] **Step 2: If repository permissions require it, use the established one-shot guarded workflow pattern.**

The one-shot workflow must:
- verify `HEAD^`/expected parent equals the frozen behavior SHA;
- download exactly the frozen generated-authority artifact;
- verify its exact archive digest/hash;
- replace only the four generated authority files;
- delete itself;
- commit/push the deterministic bytes.

No manual SBOM/hash/package-manifest edits.

- [ ] **Step 3: Because a bot `GITHUB_TOKEN` push may not recursively trigger ordinary CI, create an empty same-tree qualification commit only when necessary.**

The empty qualification commit changes no bytes and exists only to trigger ordinary `ci` on the synchronized tree. Record sync commit SHA, exact tree, empty qualification SHA, and parent relationship.

- [ ] **Step 4: Require ordinary CI success on the exact current `main` HEAD.**

Final gate requires all three jobs SUCCESS on that exact SHA:
- `main-authority-continuity`;
- `supply-chain-static`, including tracked generated-authority zero-diff;
- `managed`, including all product and test gates plus deterministic workspace ZIP double build and verified-delivery artifact upload.

- [ ] **Step 5: Record final artifacts and deterministic workspace evidence.**

Capture from the exact-main run:
- generated-authority artifact ID/name/digest;
- verified-delivery artifact ID/name/digest;
- `Converty_0.1.0-dev.13_full_workspace.zip` SHA-256, bytes, file count, package-manifest entry count, SHA-manifest entry count;
- CRC result;
- deterministic double-build result;
- exclusion-policy result.

- [ ] **Step 6: Re-fetch `main` one last time and compare it to the qualified SHA before any completion claim.**

If `main` moved, the previous run is historical development evidence, not final authority. Audit the intervening commits and requalify the new exact `main` tree.

- [ ] **Step 7: Produce the recursive copy-paste handover.**

The final handover must contain:
- repository/default branch/live SHA/tree/parent;
- immutable prior dev.12 qualified authority and relevant historical behavior heads;
- design and plan commits;
- every dev.13 RED/GREEN/failure commit and workflow run/job ID;
- exact code changes and reasons;
- exact managed/static/security/product outcomes and counts;
- generated-authority artifact/source/hash details;
- final delivery artifact/workspace ZIP hash/counts;
- historical failures preserved;
- shipping blockers still open;
- explicitly unverified claims, especially headed Windows 11 acceptance and production signing/FFmpeg redistribution;
- **ONE precise next task**;
- all product/security invariants;
- the recursive handover rule itself.

## Plan Self-Review Checklist

Before implementation begins, verify this plan against the approved spec:

- [ ] Both `status` and `cancel` are covered end-to-end.
- [ ] Legacy admission shape and `IBridgeRequestClient` remain unchanged.
- [ ] One existing pipe only; no persistent control session/UI/polling.
- [ ] Control request uses canonical `D` job ID without changing legacy parsing.
- [ ] Response cross-field invariants and operation/job correlation are tested.
- [ ] Host unknown/non-cancellable/persistence-failure semantics exactly match existing queue behavior.
- [ ] Queued cancellation persistence failure leaves queued state unchanged.
- [ ] Bridge identity authentication is proven before both status and cancel application frames.
- [ ] Strict malformed/duplicate/unknown/schema/member cases are covered.
- [ ] Expanded fuzz/static gates preserve Host/Bridge media/process neutrality.
- [ ] Version/evidence updates occur only from observed results.
- [ ] Generated authority is runner-generated and synchronized separately from behavior qualification.
- [ ] Final completion is exact-current-`main` ordinary CI, not a side branch or stale SHA.
- [ ] No placeholders such as future run IDs, artifact IDs, hashes, or test totals are invented.
