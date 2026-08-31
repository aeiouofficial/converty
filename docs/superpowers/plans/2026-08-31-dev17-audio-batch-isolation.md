# Dev.17 Audio batch failure-isolation implementation plan

1. Re-resolve frozen dev.16 `main`; branch from it without force.
2. RED managed test: valid → failure → valid must attempt all three files, preserve successful publications and clean per-file staging.
3. RED static contract: require a dedicated real packaged mixed-batch smoke and CI wiring while preserving one native Bridge process for a same-family selection.
4. Minimal production fix in existing `ConversionBatchRunner`: isolate only `ConversionFailedException`, continue, then rethrow the first after iteration; preserve fail-fast behavior for other exceptions/cancellation.
5. Implement Windows smoke: valid WAV → malformed WAV → valid FLAC → truncated FLAC → valid WAV; one Bridge process; run twice; verify aggregate exit 4, numbered MP3 successes, no invalid publication, source/destination hashes, ffprobe codec and zero partials.
6. Wire into ordinary Windows CI after dev.16 Audio acceptance and before managed tests.
7. For every harness failure, diagnose root cause before touching production; add a regression assertion before the harness fix.
8. Require behavior GREEN: native/package/COM/product gates, dev.16 36-case matrix, dev.17 mixed batch, managed/static/vector suites, deterministic workspace A/B build up to the expected stale generated-authority boundary.
9. Stage `0.1.0-dev.17` version/docs/evidence atomically; do not hand-edit generated SBOM/package/hash authority.
10. Let versioned CI generate authority; independently retrieve/verify exact artifact; synchronize only the four generated authority files through the guarded temporary authority workflow/process; remove temporary workflow afterward.
11. Require same-tree branch qualification with generated-authority zero-diff and all applicable behavior gates green; side-branch continuity failure is expected.
12. Re-fetch `main`. If and only if it still equals frozen dev.16 and compare is pure fast-forward, update `main` with `force:false`.
13. Require a fresh exact-main ordinary CI run where continuity, managed and supply-chain-static all succeed. Independently verify generated-authority and verified-delivery artifact metadata/bytes/workspace integrity.
14. Record final dev.17 SHA/tree/run/jobs/artifacts/workspace hash/counts and update recursive handover. Only then call dev.17 frozen and begin dev.18 Image work.
