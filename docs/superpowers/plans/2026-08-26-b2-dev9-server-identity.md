# B2 dev.9 reciprocal Host identity implementation plan

> **Scope:** Converty `0.1.0-dev.9`, B2 only. Do not begin B3 Explorer implementation and do not add FFmpeg/WIC/media execution.

## Goal

Close the reciprocal side of the Bridge↔Host trust boundary far enough that every newly connected Bridge pipe session fails closed before the first application frame unless the server is the expected Converty Host process from the selected Windows package/install authority.

Dev.8 already constrains Host activation to the trusted installed `Converty.Host.exe` path and bounds startup/retry. Dev.9 must authenticate the *server behind the connected pipe*; fixed path selection before launch, same-user ACLs, endpoint naming and client-SID validation are necessary but are not server authentication.

## Packaging/trust decision used by this plan

Production Converty will use a **full-trust MSIX package** as the Windows package-identity authority. The native Explorer command and managed Host/Bridge payload belong to the same package family. The package publisher is tied to the package-signing certificate; production signing verification remains a separate release gate and CI must not call unsigned test binaries production-signed.

The reciprocal check therefore binds a connected pipe session to all of the following:

1. the server process ID returned by Windows for the already-connected pipe handle;
2. the exact canonical trusted `Converty.Host.exe` path selected from installation/package authority;
3. the same Windows package-family identity as the packaged Bridge;
4. a stable server PID across the identity query so a failed/raced lookup is rejected rather than guessed through.

No shared secret, caller-provided executable, command string, preset field or IPC field participates in this trust decision.

Relevant Microsoft platform authority:
- `IExplorerCommand` modern context-menu registration uses package identity and package-manifest `windows.comServer` / `windows.fileExplorerContextMenus` extensions: https://learn.microsoft.com/windows/apps/desktop/modernize/integrate-packaged-app-with-file-explorer
- Full MSIX keeps payload in the package rather than an external sparse-package location: https://learn.microsoft.com/windows/apps/package-and-deploy/packaging/
- Packaged desktop install files are read-only/OS-protected and Windows prevents launch after tampering: https://learn.microsoft.com/windows/msix/desktop/desktop-to-uwp-behind-the-scenes
- Package Publisher must match the package-signing certificate subject: https://learn.microsoft.com/windows/msix/package/sign-msix-package-guide
- `GetNamedPipeServerProcessId`: https://learn.microsoft.com/windows/win32/api/winbase/nf-winbase-getnamedpipeserverprocessid
- `GetPackageFamilyName`: https://learn.microsoft.com/windows/win32/api/appmodel/nf-appmodel-getpackagefamilyname
- `GetCurrentPackageFamilyName`: https://learn.microsoft.com/windows/win32/api/appmodel/nf-appmodel-getcurrentpackagefamilyname

## Task 1 — lock the production package identity decision

**Files**
- Add: `docs/adr/ADR-011-full-msix-identity.md`
- Update as needed later: `docs/security/B2_SERVER_AUTH_GATE.md`
- Update as needed later: `docs/SECURITY_THREAT_MODEL.md`

**Acceptance**
- ADR says full-trust MSIX is the production identity model.
- Sparse/external-location packaging is not the production default because it splits package identity from executable location.
- Production signing remains externally provisioned and separately verified; no private key enters the repository.
- No claim that unsigned CI artifacts satisfy production signer acceptance.

## Task 2 — RED tests for pre-frame server authentication

**Files**
- Modify: `tests/Converty.Bridge.Tests/Ipc/BridgeClientTests.cs`
- Add: `tests/Converty.Bridge.Tests/Ipc/ConnectedServerIdentityVerifierTests.cs` (Windows/native boundary tests where practical)

**Required RED behavior**
1. identity verifier rejection occurs after connect but before any application request frame is written;
2. a valid verifier allows the existing request/acknowledgement path;
3. verifier is invoked once for every new pipe session;
4. identity failure is not translated into Host-unavailable/startup retry;
5. wrong path fails closed;
6. wrong/missing package family fails closed;
7. server PID lookup/process-open/image/package query failure fails closed;
8. server-PID race/mismatch fails closed.

Prefer injectable interfaces for deterministic policy tests. Do not weaken the production verifier to make CI work when the CI process has no package identity.

## Task 3 — implement the minimal connected-server verifier

**Files**
- Add: `src/Converty.Bridge/Ipc/IConnectedServerIdentityVerifier.cs`
- Add: `src/Converty.Bridge/Ipc/BridgeServerIdentityException.cs`
- Add: `src/Converty.Bridge/Ipc/WindowsConnectedServerIdentityVerifier.cs`
- Modify: `src/Converty.Bridge/Ipc/BridgeClient.cs`

**Implementation constraints**
- `BridgeClient` owns the ordering: connect → verify connected server → serialize/write first application frame.
- Production factory derives expected package family from the current packaged Bridge using Windows package identity, not request text.
- Server PID comes from the connected pipe handle.
- Open the server process with query-only rights.
- Resolve the canonical process image path and require exact trusted Host-path equality using Windows path comparison semantics.
- Query server package-family name and require exact equality with the expected Converty package family.
- Re-query/revalidate the server PID before accepting identity; any race/error fails closed.
- No shell, no command line, no executable-text input, no network, no media parser.

## Task 4 — integrate identity with trusted activation composition

**Files**
- Modify only the Bridge startup/composition surface actually needed.
- Extend static boundary tests if a new factory/composition point is introduced.

**Acceptance**
- The trusted Host path used for process activation is the same trusted path supplied to the connected-server identity verifier.
- A server-auth failure never triggers another Host launch.
- Every new retry/session authenticates independently.
- Existing maximum connect/startup deadlines remain unchanged and finite.

## Task 5 — wire-operation decision

After server identity is green, inspect current status/cancel behavior and explicitly decide whether status/cancel become first-class IPC operations in dev.9.

If implemented:
- add a bounded, versioned operation discriminant;
- reject unknown operations, future versions, duplicate JSON members, malformed GUIDs, oversized payloads and executable-text fields;
- keep conversion requests backwards-explicit and schema strict.

If deferred:
- record the decision and blocker in backlog/handover; do not imply B2 closure.

## Task 6 — adversarial B2 expansion

Add executable coverage, reusing existing journal/runtime primitives rather than adding a parallel subsystem:
- Host restart during queued work;
- orphan journal `.tmp`;
- corrupt committed journal;
- request replay after restart;
- duplicate `RequestId`;
- disconnect before request completion;
- disconnect before acknowledgement;
- partial frame/header;
- oversized declared frame;
- timeout during connect/read/write where deterministically testable;
- unauthorized SID;
- fake/squatted server;
- Host launch failure;
- second Host instance;
- queue/journal failure with no partial in-memory mutation.

## Task 7 — B2 closure decision

Do **not** mark B2 complete merely because server identity is implemented. Mark B2 complete only if the complete process-start/server-auth/replay/restart acceptance matrix is executable and green.

Only after that may a later tranche begin B3 native Explorer `IExplorerCommand` implementation.

## Task 8 — dev.9 authority/release closure

At tranche end:
- synchronize `VERSION`, README, CHANGELOG, backlog, implementation status, handover docs/state, toolchain and build evidence to `0.1.0-dev.9`;
- regenerate source SBOM and release SBOM;
- regenerate `machine-readable/package_manifest.json` and `SHA256SUMS.txt`;
- run immutable Action-pin, contract-vector, static, locked restore, dependency audit, zero-warning/zero-error Release build, full managed tests and native smoke on one exact SHA;
- require zero generated-authority diff;
- build the workspace ZIP twice deterministically from committed Git bytes;
- compare byte-for-byte, reopen, CRC-test, verify package-manifest and SHA manifest hashes, and verify exclusions;
- freeze evidence only from real passing runs and independently requalify the evidence-frozen tree;
- produce `Converty_0.1.0-dev.9_full_workspace.zip`, `Converty_0.1.0-dev.9_HANDOVER_PROMPT.txt`, and build/package evidence.
