# B2 dev.7 Persistent Journal + Host Runtime Plan

**Goal:** advance B2 with a versioned crash-recoverable Host job journal and a bounded Host runtime loop wired to the existing per-user single-instance lease. Do not add media parsing, providers, worker execution, or Explorer integration.

## Journal invariants
- Persist only bounded typed job state: schema version, job ID, request ID, state, progress, message.
- No paths, command strings, engine arguments, environment values, provider code, or media-derived content are required by the journal.
- Maximum journal size and entry count are explicit before expensive allocation.
- JSON rejects unknown schema versions, unknown members, duplicate members, malformed enum/state values, duplicate job IDs, and duplicate request IDs.
- Commit writes a same-directory temporary file, flushes it to disk, then atomically replaces/moves the committed journal path.
- A failed commit must not publish the corresponding queue mutation in memory.
- Orphan temporary files never override a valid committed journal.
- On restart, `Queued` and terminal states can be restored; any in-flight state from `Probing` through `Committing` becomes `Failed` with an interruption message. No conversion automatically resumes after a crash.

## Runtime invariants
- One Host runtime per user, using `HostSingleInstanceLease`.
- Runtime restores journal before accepting IPC.
- Runtime owns one bounded server loop and stops on cancellation.
- Unauthorized/malformed/oversized sessions remain non-mutating.
- No process launch or media execution is introduced in this tranche.

## TDD sequence
1. RED journal tests: atomic replacement, strict load, duplicate rejection, temp-orphan behavior, interrupted-state recovery, mutation rollback on persistence failure.
2. GREEN `HostJobJournal` and queue integration.
3. RED Host runtime tests: second same-user runtime rejected, journal restored before sessions, cancellation exits loop.
4. GREEN `HostRuntime` loop around the existing `HostPipeServer` factory/lease.
5. Add dev.7 static/adversarial coverage, then run locked restore, vulnerability audit, Release build, managed tests, static gates, release preflight/SBOM, native topology smoke.
6. Synchronize version/status/backlog/evidence/handover and package deterministic `Converty_0.1.0-dev.7_full_workspace.zip` twice with CRC/hash verification.

B2 remains open after dev.7 unless Bridge trusted Host startup/retry and remaining anti-squatting/session/signature acceptance are also qualified.
