# Converty 0.1.0-dev.6 — Next-Agent Handover

## Current authority
- Delivered workspace: `0.1.0-dev.6`.
- Next workspace: `0.1.0-dev.7`.
- Next implementation batch: **continue B2; do not start B3 yet**.
- B0/B1 regression authority remains green.
- Read `machine-readable/handover_state.json` and `machine-readable/build_evidence.json` before changing source.

## Dev.6 qualification
The B2 behavior qualification used .NET SDK `10.0.400` on GitHub Actions Windows Server 2025 and produced:
- 15/15 managed projects restored from committed lock files;
- dependency audit PASS with zero vulnerable-result packages;
- Release build PASS with zero warnings and zero errors;
- 108/108 managed tests PASS;
- seven checked-in IPC adversarial cases executed with their expected rejection behavior;
- 5/5 raw contract vectors PASS;
- repository boundary verification and native topology smoke PASS.

Exact immutable behavior run IDs and SHA are recorded in `machine-readable/build_evidence.json`. Final authority synchronization/package verification is recorded there separately.

## B2 implemented in dev.6
- `Converty.Ipc`: versioned fixed 12-byte framing, checked length handling, 1 MiB payload ceiling, cancellation/truncation/future-version rejection.
- `Converty.Security`: protected current-user pipe DACL, SID-hashed endpoint identity, connected-client SID validation through pipe impersonation.
- `Converty.Host`: bounded in-memory queue, duplicate/capacity rejection, queued status/cancellation, strict admission, ACL-backed one-session pipe server, tested per-user single-instance lease primitive.
- `Converty.Bridge`: bounded same-user request client, maximum 30-second connect timeout, strict one-acknowledgement validation.
- `tests/fuzz/ipc/v1/corpus.json`: seven adversarial protocol/request cases executed by managed tests.

## B2 is not fully closed
Do **not** mark B2 complete or begin B3 until these remaining B2 items are resolved with executable evidence:
1. persistent crash-safe atomic Host job journal and recovery tests;
2. complete Host executable lifetime/server loop wired to `HostSingleInstanceLease`;
3. Bridge Host startup/retry/fast-failure process behavior using trusted install-path/signature policy;
4. status/cancellation wire operations if required by the finalized Host runtime surface;
5. remaining anti-squatting/signed-peer/session validation justified by the packaging model;
6. remaining B2 adversarial/fuzz acceptance cases needed for the final B2 gate.

## Non-negotiable boundaries
1. Explorer is trigger-only; no parsing, conversion, network, settings database, or engine/plugin load.
2. Host/coordinator never parses untrusted media and never dynamically loads codec/plugin code.
3. Probe and conversion belong to disposable restricted workers.
4. Ordinary local conversion has no network requirement; strict worker profile denies network.
5. IPC uses explicit same-user ACL plus peer validation and bounded/versioned framing.
6. Presets/IPC never carry raw executable command strings or pass-through engine argument vectors.
7. Provider options are typed/whitelisted before argument construction.
8. Workers write only private staging; Host validates and atomically commits final output.
9. Strict isolation never silently falls back to compatibility mode.
10. Numbered copy remains the safe default collision policy.
11. Signing private keys never enter the repository/workspace.

## Immediate next work: B2 / 0.1.0-dev.7
Continue test-first from the existing B2 implementation. Prefer this order:
1. design and implement the minimal versioned persistent job-journal contract and atomic recovery semantics without storing executable text;
2. wire Host lifetime to the per-user single-instance lease and a bounded server loop;
3. add Bridge Host-start/retry behavior with strict executable-path/signature policy and finite deadlines;
4. extend integration/adversarial tests for restart, duplicate/replay, disconnects, timeout stages, and journal recovery;
5. close the B2 gate only when malformed/unauthorized/oversized/replayed IPC and Host restart/crash paths cannot corrupt or incorrectly enqueue state.

Do not add FFmpeg/WIC/media-provider execution in B2. B4 containment must exist before hostile media parsing is introduced.

Only claim evidence actually executed. At tranche end synchronize version/docs/handover/machine-readable evidence and regenerate/verify the complete workspace package again.
