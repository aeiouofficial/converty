# Dev.14 one-shot replay/disconnect/reconnect acceptance design

## Goal
Qualify recovery from ambiguous client disconnects and repeated one-shot operations on the existing authenticated Host named pipe, without adding a persistent-session protocol or changing normal Explorer conversion routing.

## Existing authority
Dev.13 already uses one request/one response per pipe connection. `HostRuntime` owns one `HostJobQueue` across connections and creates a fresh `HostPipeServer` session for each connection. Bridge status/cancel opens a fresh pipe and authenticates the connected Host before the first application frame.

## Decision
Treat `ConversionRequest.requestId` as the admission idempotency key. `HostJobQueue.TryEnqueue` keeps its duplicate outcome. When `HostRequestHandler` receives that duplicate outcome, it resolves the existing job by request ID and returns `accepted=true` with the existing `jobId`. It never enqueues a second job.

This lets a client that disconnected after sending but before receiving the admission response replay the same request on a fresh authenticated connection and recover the canonical job ID.

## Acceptance matrix
1. Send valid admission, close client before reading response, verify only one queue entry exists.
2. Replay the same admission on a fresh connection, expect the existing job ID and queue count still one.
3. Query status on another fresh connection, expect the same request/job correlation and queued state.
4. Independently qualify admission → fresh status → fresh cancel → fresh status, with queued → cancelled → cancelled and one queue entry.

## Security and architecture invariants
- Same current-user authentication and DACL/peer validation ordering remain unchanged.
- No second pipe, long-lived session token, polling daemon or new broker.
- No media parsing or execution moves into Host/Bridge.
- No shell command construction, raw FFmpeg argument passthrough, PATH lookup, network dependency or silent Strict→Compatibility fallback.
- Queue/journal persistence remains authoritative; restored request-to-job mappings continue to be constructed from restored status snapshots.

## Non-goals
Headed Windows 11 UI acceptance, production signed-package identity requalification, production FFmpeg redistribution approval, and signed-MSIX clean-VM acceptance remain separate gates.
