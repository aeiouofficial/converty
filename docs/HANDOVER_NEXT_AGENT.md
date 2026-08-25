# Converty 0.1.0-dev.7 — Next-Agent Handover

## Current authority
- Delivered workspace: `0.1.0-dev.7`.
- Next workspace: `0.1.0-dev.8`.
- Next implementation batch: **finish B2; do not start B3 yet**.
- B0/B1 regression authority remains green.
- Read `machine-readable/handover_state.json` and `machine-readable/build_evidence.json` before changing source.

## Dev.7 qualification
The B2 behavior qualification used .NET SDK `10.0.400` on GitHub Actions Windows Server 2025 and produced:
- 15/15 managed projects restored from committed lock files;
- dependency audit PASS with zero vulnerable-result packages;
- Release build PASS with zero warnings and zero errors;
- 120/120 managed tests PASS;
- raw contract vectors and repository/static boundary gates PASS;
- native topology smoke PASS.

Exact immutable behavior run IDs and SHA are recorded in `machine-readable/build_evidence.json`. Final authority/package closure is recorded separately by the tranche closure workflow.

## B2 implemented through dev.7
- `Converty.Ipc`: fixed 12-byte versioned framing, checked exact reads, 1 MiB payload ceiling, cancellation/truncation/future-version rejection.
- `Converty.Security`: protected current-user pipe DACL, SID-hashed endpoint identity, and connected-client SID validation through pipe impersonation before application-frame parsing.
- `Converty.Host`: bounded admission/status/cancellation queue; strict request admission; ACL-backed pipe sessions; per-user single-instance lease; WinExe runtime loop; persistent bounded crash-recovery journal.
- Host journal: schema v1, 4096-entry / 8 MiB limits, strict unknown/duplicate rejection, same-directory temporary write with write-through + disk flush before atomic publication, orphan-temp removal, duplicate ID rejection, and in-flight restart recovery to `Failed`.
- Queue mutations using a journal persist before publishing in-memory state; journal write failure leaves queue state unchanged.
- `Converty.Bridge`: bounded same-user request client, maximum 30-second connect timeout, strict one-acknowledgement parsing.
- Checked-in IPC adversarial corpus plus dev.7 journal/runtime tests.

## B2 is still not fully closed
Do **not** begin B3 until the following remaining B2 acceptance gaps are resolved with executable evidence:
1. Bridge trusted Host startup/retry/fast-failure process behavior with finite deadlines and an installed executable identity policy;
2. server-auth / pipe-squatting resistance appropriate to the selected packaging/signing model, beyond same-user client validation;
3. decide and test whether status/cancel require dedicated final wire operations;
4. extend restart/replay/disconnect/timeout/session adversarial coverage as required by the final B2 acceptance matrix;
5. explicitly close B2 only when malformed, unauthorized, oversized, replayed, disconnected, squatted-endpoint, and restart/crash paths cannot corrupt or incorrectly enqueue state.

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

## Immediate next work: B2 / 0.1.0-dev.8
Continue test-first from the existing B2 runtime. Preferred order:
1. define the trusted installed Host identity/path abstraction for Bridge without embedding arbitrary executable command text;
2. implement finite Host-start/retry/fast-failure behavior and prove that wrong/missing/untrusted Host identity fails closed;
3. add server-auth/anti-squatting/session acceptance under the selected package/signing assumptions;
4. finalize status/cancel wire-surface decision and tests;
5. expand B2 adversarial coverage and close the B2 gate only when all remaining acceptance criteria are evidence-backed.

Do not add FFmpeg/WIC/media-provider execution in B2. B4 containment must exist before hostile media parsing is introduced.

Only claim evidence actually executed. At tranche end synchronize version/docs/handover/machine-readable evidence and regenerate/verify the complete workspace package again.
