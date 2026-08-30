# Dev.14 session replay acceptance implementation plan

1. Preserve exact dev.13 main authority and create a temporary dev.14 side branch for RED evidence.
2. Add real named-pipe acceptance tests for ambiguous disconnect/replay and fresh-connection status/cancel lifecycle.
3. Run full CI and require the replay test to fail for the intended current behavior while build/product gates remain healthy.
4. Add the smallest queue API needed to resolve an existing job by request ID under the existing queue lock.
5. Change only admission duplicate handling so authenticated replay returns the existing job ID; keep duplicate prevention in the queue.
6. Re-run full CI and require all managed/static/product gates green before the expected generated-authority freshness boundary.
7. Version the workspace as `0.1.0-dev.14`, update human/machine-readable authority and keep the four generated authority files runner-owned.
8. Qualify the versioned candidate, synchronize exactly the runner-generated source SBOM, release SBOM, package manifest and SHA256 manifest from one exact artifact, then requalify zero-diff.
9. Fast-forward `main` only after branch qualification; run ordinary CI on the exact main SHA and require continuity + managed + supply-chain-static all successful with deterministic verified delivery.
10. Record final commit/tree/run/job/artifact/workspace evidence and preserve the headed/signing/redistribution limitations explicitly.
