# Converty 0.1.0-dev.8 — Next-Agent Handover

## Current authority
- Delivered workspace: `0.1.0-dev.8`.
- Next workspace: `0.1.0-dev.9`.
- Next implementation batch: **finish B2; do not start B3 yet**.
- B0/B1 regression authority remains green.
- Read `machine-readable/handover_state.json` and `machine-readable/build_evidence.json` before changing source.

## Dev.8 qualification
The B2 behavior qualification used .NET SDK `10.0.400` on GitHub Actions Windows Server 2025 and produced:
- 15/15 managed projects restored from committed lock files;
- dependency audit PASS with zero vulnerable-result packages;
- Release build PASS with zero warnings and zero errors;
- 129/129 managed tests PASS;
- raw contract vectors and repository/static boundary gates PASS;
- native topology smoke PASS.

Exact immutable behavior run IDs and SHA are recorded in `machine-readable/build_evidence.json`. Final authority/package closure is recorded separately by the tranche closure workflow.

## B2 implemented through dev.8
- `Converty.Ipc`: fixed 12-byte versioned framing, checked exact reads, 1 MiB payload ceiling, cancellation/truncation/future-version rejection.
- `Converty.Security`: protected current-user pipe DACL, SID-hashed endpoint identity, and connected-client SID validation before application-frame parsing.
- `Converty.Host`: bounded admission/status/cancellation queue; strict request admission; ACL-backed pipe sessions; per-user single-instance WinExe runtime loop; persistent bounded crash-recovery journal.
- Host journal: schema v1, 4096-entry / 8 MiB limits, strict unknown/duplicate rejection, write-through + disk flush before atomic publication, orphan-temp removal, duplicate ID rejection, and in-flight restart recovery to `Failed`.
- Queue mutations using a journal persist before publishing in-memory state; journal write failure leaves queue state unchanged.
- `Converty.Bridge`: strict one-session bounded request client and connect-stage `BridgeHostUnavailableException` classification.
- `TrustedHostPath`: derives only fixed `Converty.Host.exe` from an absolute existing non-reparse installation directory; callers do not choose an executable name or command line.
- `InstalledHostProcessLauncher`: the only approved Bridge process-start site; no shell, no caller arguments, hidden/no-console startup, trusted working directory.
- `BridgeSubmissionCoordinator`: tries the existing Host first, launches the trusted Host at most once only after connect-stage unavailability, retries inside a maximum 30-second startup deadline with delay capped at one second, propagates protocol/application failures without starting Host, and honors caller cancellation.
- Static/repository gates preserve the process/media/network boundaries and confine `Process.Start` to the dedicated Bridge startup launcher.
- Checked-in IPC adversarial corpus plus dev.7 journal/runtime and dev.8 startup/retry tests.

## B2 is still not fully closed
Do **not** begin B3 until these remaining B2 acceptance gaps are resolved with executable evidence:
1. connected-server identity / anti-squatting verification before Bridge trusts a connected session, aligned to the actual installer/signing/package authority; fixed path, pipe name, and same-user ownership alone are not sufficient server authentication;
2. decide and test whether status/cancel require dedicated final wire operations, or explicitly document/qualify why the existing surface is sufficient;
3. complete replay/disconnect/reconnect/session-confusion adversarial acceptance;
4. explicitly close B2 only when malformed, unauthorized, oversized, replayed, disconnected, wrong-server, and restart/crash paths cannot corrupt or incorrectly enqueue state.

## Non-negotiable boundaries
1. Explorer is trigger-only; no parsing, conversion, network, settings database, or engine/plugin load.
2. Host/coordinator never parses untrusted media and never dynamically loads codec/plugin code.
3. Probe and conversion belong to disposable restricted workers.
4. Ordinary local conversion has no network requirement; strict worker profile denies network.
5. IPC uses explicit same-user ACL plus peer validation and bounded/versioned framing.
6. Presets/IPC never carry raw executable command strings or pass-through engine argument vectors.
7. Provider options are typed/whitelisted before argument construction.
8. Workers later write only private staging; Host validates and atomically commits final output.
9. Strict isolation never silently falls back to compatibility mode.
10. Numbered copy remains the safe default collision policy.
11. Signing private keys never enter the repository/workspace.
12. Bridge process creation remains confined to the fixed trusted Host launcher; no request/preset data may select executable paths or process arguments.

## Immediate next work: B2 / 0.1.0-dev.9
Continue test-first from the existing B2 runtime. Preferred order:
1. select and implement the connected-server identity rule that matches the actual Windows packaging/signing model and validate it before Bridge sends the application request frame;
2. add wrong/unverifiable-server fail-closed tests and revalidation for every new session without claiming production signing evidence unless real signed artifacts exist;
3. finalize the status/cancel wire-surface decision and tests;
4. expand replay/disconnect/reconnect/session adversarial coverage;
5. close B2 only when the complete acceptance matrix is executable and green; only then begin B3 Explorer work.

Do not add FFmpeg/WIC/media-provider execution in B2. B4 containment must exist before hostile media parsing is introduced.

Only claim evidence actually executed. At tranche end synchronize version/docs/handover/machine-readable evidence and regenerate/verify the complete workspace package again.
