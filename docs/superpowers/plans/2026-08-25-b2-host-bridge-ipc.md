# B2 Host + Bridge + Hardened IPC Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Deliver the first B2 tranche where a same-user Converty Bridge can submit bounded fake conversion requests to a single-user Host over versioned authenticated named-pipe IPC without any media parsing or engine execution.

**Architecture:** `Converty.Ipc` owns protocol limits, checked length-prefixed framing, handshake/envelope types, and transport-independent validation. `Converty.Security` owns Windows current-user SID policy, explicit pipe DACL construction, endpoint naming, and connected-peer validation. `Converty.Host` owns bounded in-memory job admission/status/cancellation and the named-pipe server loop. `Converty.Bridge` owns bounded request submission, Host endpoint connection, handshake timeouts, and fast failure; it never waits for conversion completion.

**Tech Stack:** C# 14 / .NET 10, `System.IO.Pipes`, `System.IO.Pipes.AccessControl` 5.0.0, Windows identity/access-control APIs, xUnit v3 + Microsoft Testing Platform.

**Spec:** `docs/Converty_Master_Build_Plan.md` sections 2, 3, 4, 8, 12, and B2.

## Global Constraints

- Explorer/Bridge/Host must never parse or decode untrusted media.
- No provider/codec/plugin loading in Bridge or Host.
- IPC is explicit, versioned, bounded, authenticated to the expected peer/user, and reject-by-default.
- Named-pipe DACL must not grant Everyone, Anonymous, broad Users, or unrelated app-container groups.
- No raw engine arguments, shell commands, scripts, or arbitrary environment values in IPC.
- Strict request-size/count ceilings and checked arithmetic are mandatory before allocation.
- Unauthorized, malformed, or oversized IPC must not enqueue jobs or crash Host.
- This tranche does not execute FFmpeg, probe media, launch workers, integrate Explorer, or claim release signing/sandbox qualification.

---

### Task 1: RED project topology and protocol contract tests

**Files:**
- Create: `src/Converty.Ipc/Converty.Ipc.csproj`
- Create: `tests/Converty.Ipc.Tests/Converty.Ipc.Tests.csproj`
- Create: `tests/Converty.Ipc.Tests/Protocol/FrameCodecTests.cs`
- Modify: `Converty.slnx`

**Interfaces:**
- Produces expected API `ProtocolLimits`, `ProtocolFrame`, `ProtocolFrameCodec.ReadAsync/WriteAsync`.

- [ ] **Step 1: Add tests first** for exact round-trip, unsupported version, zero/negative/oversized lengths, truncated payloads, and cancellation.
- [ ] **Step 2: Push RED commit and verify Windows CI fails because the production protocol types do not exist.**
- [ ] **Step 3: Implement minimal framing types/code using a fixed 12-byte header (`magic`, `version`, `payloadLength`) and `BinaryPrimitives` checked bounds.**
- [ ] **Step 4: Verify managed tests pass with no warnings.**

### Task 2: Explicit same-user Windows pipe security

**Files:**
- Create: `src/Converty.Security/Converty.Security.csproj`
- Create: `src/Converty.Security/Ipc/CurrentUserPipeSecurity.cs`
- Create: `src/Converty.Security/Ipc/PipeEndpointName.cs`
- Create: `src/Converty.Security/Ipc/ConnectedPeerValidator.cs`
- Create: `tests/Converty.Security.Tests/Converty.Security.Tests.csproj`
- Create: `tests/Converty.Security.Tests/Ipc/CurrentUserPipeSecurityTests.cs`
- Create: `tests/Converty.Security.Tests/Ipc/PipeEndpointNameTests.cs`
- Create: `tests/Converty.Security.Tests/Ipc/ConnectedPeerValidatorTests.cs`
- Modify: `Directory.Packages.props`
- Modify: `Converty.slnx`

**Interfaces:**
- Produces `CurrentUserPipeSecurity.Create(SecurityIdentifier) -> PipeSecurity`.
- Produces `PipeEndpointName.ForUser(SecurityIdentifier) -> string`.
- Produces `ConnectedPeerValidator.IsExpectedUser(NamedPipeServerStream, SecurityIdentifier) -> bool`.

- [ ] **Step 1: Add tests asserting a protected DACL with exactly the expected user allow rule and no broad identities.**
- [ ] **Step 2: Add deterministic SID-qualified endpoint-name tests that reject malformed/oversized SID text.**
- [ ] **Step 3: Add peer-validation tests using an injectable identity-reader seam so authorization behavior is testable without weakening the production pipe check.**
- [ ] **Step 4: Implement the minimal Windows-specific security code; use `NamedPipeServerStreamAcl.Create` with supplied `PipeSecurity`, not `PipeOptions.CurrentUserOnly` because that option ignores supplied pipe security.**
- [ ] **Step 5: Verify all security tests and analyzers are green.**

### Task 3: Host admission queue, status, and cancellation

**Files:**
- Create: `src/Converty.Host/Converty.Host.csproj`
- Create: `src/Converty.Host/Jobs/HostJobQueue.cs`
- Create: `src/Converty.Host/Jobs/JobAdmissionResult.cs`
- Create: `src/Converty.Host/Ipc/HostRequestHandler.cs`
- Create: `tests/Converty.Host.Tests/Converty.Host.Tests.csproj`
- Create: `tests/Converty.Host.Tests/Jobs/HostJobQueueTests.cs`
- Create: `tests/Converty.Host.Tests/Ipc/HostRequestHandlerTests.cs`
- Modify: `Converty.slnx`

**Interfaces:**
- `HostJobQueue.TryEnqueue(ConversionRequest) -> JobAdmissionResult`.
- `HostJobQueue.TryGet(Guid, out JobStatusSnapshot)`.
- `HostJobQueue.TryCancel(Guid) -> bool`.
- `HostRequestHandler.HandleAsync(ReadOnlyMemory<byte>, PeerAuthorization, CancellationToken) -> byte[]`.

- [ ] **Step 1: Add tests proving duplicate request IDs, unauthorized peers, oversized file selections, malformed JSON, and unsupported protocol versions cannot enqueue.**
- [ ] **Step 2: Add tests for accepted fake jobs, status lookup, and queued-job cancellation.**
- [ ] **Step 3: Implement bounded in-memory queue semantics only; no persistence and no worker execution in this tranche.**
- [ ] **Step 4: Verify rejection paths do not mutate queue count.**

### Task 4: Named-pipe Host server and Bridge client

**Files:**
- Create: `src/Converty.Host/Ipc/HostPipeServer.cs`
- Create: `src/Converty.Bridge/Converty.Bridge.csproj`
- Create: `src/Converty.Bridge/Ipc/BridgeClient.cs`
- Create: `src/Converty.Bridge/Ipc/BridgeSubmissionResult.cs`
- Create: `tests/Converty.Bridge.Tests/Converty.Bridge.Tests.csproj`
- Create: `tests/Converty.Bridge.Tests/Ipc/BridgeClientTests.cs`
- Create: `tests/Converty.Host.Tests/Ipc/HostPipeServerTests.cs`
- Modify: `Converty.slnx`

**Interfaces:**
- `HostPipeServer.RunSingleConnectionAsync(CancellationToken)` creates the ACL-protected server, validates the connected user before reading an application frame, enforces handshake/read/write timeouts, and delegates a bounded payload to `HostRequestHandler`.
- `BridgeClient.SubmitAsync(ConversionRequest, CancellationToken) -> BridgeSubmissionResult` connects to the SID-qualified local endpoint with a short timeout, writes one bounded request frame, reads one bounded acknowledgement, then returns without waiting for conversion.

- [ ] **Step 1: Add integration tests over a local named pipe on Windows for successful same-user submission and fast timeout/failure.**
- [ ] **Step 2: Add malformed/oversized frame cases to ensure the Host rejects before job admission.**
- [ ] **Step 3: Implement server/client with one reader/one writer per pipe instance and finite connect/handshake/read/write deadlines.**
- [ ] **Step 4: Verify no engine/process/network APIs are introduced in Contracts/Core/Serialization and no process execution exists in Bridge/Host.**

### Task 5: dev.6 authority, fuzz corpus, evidence, and packaging

**Files:**
- Create: `tests/fuzz/ipc/v1/` corpus for bad magic, future version, negative/oversized/truncated lengths, malformed request JSON, and unknown members.
- Create: `tests/static/test_dev6_b2_ipc.py`
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
- Regenerate: `machine-readable/source_sbom.spdx.json`
- Regenerate: `machine-readable/release_sbom.spdx.json`
- Regenerate: `SHA256SUMS.txt`

**Interfaces:**
- Workspace version becomes `0.1.0-dev.6`; next workspace version becomes `0.1.0-dev.7`.

- [ ] **Step 1: Add static gate requiring all four B2 projects/tests, explicit ACL API usage, protocol bounds, and absence of FFmpeg/process execution.**
- [ ] **Step 2: Bump version/authority only after managed B2 behavior is green.**
- [ ] **Step 3: Generate lock files for all new managed projects and run vulnerability audit.**
- [ ] **Step 4: Run Release build, full managed tests, fuzz/static corpus, release preflight/SBOM, native topology smoke, and generated-authority zero-diff.**
- [ ] **Step 5: Record only observed evidence and open the dev.6 PR; do not claim B2 fully closed unless every B2 acceptance item is actually exercised.**
