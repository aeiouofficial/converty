# Converty 0.1.0-dev.5 — Next-Agent Handover

## Current authority
- Delivered workspace: `0.1.0-dev.5`.
- Next workspace: `0.1.0-dev.6`.
- Next implementation batch: **B2 Host/Bridge authenticated IPC**.
- B2 start gate: **PASS**.
- Read `machine-readable/handover_state.json` and `machine-readable/build_evidence.json` before changing source.

## Qualification
The immutable dev.5 qualification run used .NET SDK `10.0.400` on Windows Server 2025 and produced:
- 7/7 managed projects restored from committed lock files;
- dependency audit PASS with zero vulnerable-result packages;
- Release build PASS with zero warnings and zero errors;
- 63/63 managed tests PASS;
- 19/19 static tests PASS on the qualified head;
- 5/5 raw contract vectors PASS;
- native topology smoke PASS.

Exact run IDs and SHA are machine-readable in `machine-readable/build_evidence.json`.

## What exists
- `Converty.Contracts`: versioned domain/wire contracts and bounds.
- `Converty.Core`: format registry, capability graph, conversion planner, output path resolver.
- `Converty.Serialization`: strict v1 JSON adapters.
- `Converty.FakeProviders`: data-only Audio/Image/Video fixtures.
- Seven committed NuGet lock files.
- Fail-closed dependency vulnerability auditing.
- Deterministic source/release SPDX tooling and release preflight.
- Immutable full-SHA GitHub Actions authority.
- Native CMake topology only; no production Explorer target yet.

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

## Immediate next work: B2 / 0.1.0-dev.6
Write the focused B2 design/implementation plan from existing architecture authority, then implement test-first:
- Host single-instance ownership;
- explicit pipe security descriptor/DACL;
- authenticated peer-validation strategy compatible with the packaging model;
- framed protocol version plus message/selection/count/time quotas;
- Bridge client with no media parsing;
- bounded job queue and crash-safe atomic journal;
- status/cancellation;
- malformed/oversized/unauthorized IPC fuzz and adversarial tests.

Do not begin FFmpeg/WIC/media-provider execution in B2. B4 containment must exist before hostile media parsing is introduced.

## Remaining non-B2 release work
- full production MSVC/Explorer Debug+Release matrix;
- signing infrastructure and signed-artifact evidence;
- final dependency license/notices review;
- clean VM installer/update/uninstall gates.

Only claim evidence that was actually executed.
