# Dev.18 Image input acceptance implementation plan

1. Preserve frozen dev.17 authority and create an isolated dev.18 branch from its exact main SHA.
2. Add static RED assertions requiring one dedicated Image product acceptance component, full advertised source/action coverage and transactional invariants.
3. Prove RED while all pre-existing static tests remain green.
4. Add the smallest dedicated Windows smoke needed to generate representative fixtures and invoke every conversion through packaged Bridge/Strict Worker/provider.
5. Wire that smoke into ordinary managed CI after Audio gates and before the full managed regression suite.
6. Require 24 successful Image conversions with expected codec/dimensions plus repeated malformed/truncated deterministic failure and preservation/no-partial checks.
7. Require all pre-existing Audio, managed, static, contract, dependency, native/package/COM and build gates to remain green.
8. Build the deterministic workspace twice; accept only the expected stale-authority failure before version freeze.
9. Stage dev.18 version/documentation/evidence atomically without hand-editing generated SBOM/package/hash authority.
10. Regenerate authority in CI, independently verify exact artifact digest/member/version, then synchronize only the four generated paths with a guarded self-deleting workflow.
11. Require branch generated-authority zero-diff/full qualification.
12. Re-fetch live `main`; fast-forward non-force only if it is still exact frozen dev.17.
13. Require a new exact-main ordinary CI run with continuity, managed and supply-chain-static all SUCCESS and independently verified deterministic delivery.
14. Freeze dev.18 and hand over exactly one next task: dev.19 Image mixed-valid/invalid multi-file isolation/matrix closure before Video expansion.
