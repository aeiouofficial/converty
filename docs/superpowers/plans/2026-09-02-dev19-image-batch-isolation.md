# Dev.19 implementation plan

1. Establish RED with `tests/static/test_dev19_image_batch_isolation.py` and a focused core-runner Image batch test.
2. Implement the smallest acceptance surface: `build/image-batch-isolation-smoke.ps1` plus the ordinary CI managed step. Reuse the dev.17 runner isolation behavior; avoid duplicate production architecture.
3. Run the full Windows qualification path: locked restore, dependency audit, Release build, native Explorer/package/COM/product smoke, all Audio acceptance, all 24 Image single-file conversions, Image malformed/truncated rejection, Image mixed batch twice, managed tests, deterministic workspace ZIP and delivery evidence.
4. Stage `0.1.0-dev.19` documentation/evidence while preserving all historical granular evidence keys. Never hand-edit generated SBOM/package/hash authority.
5. Generate authority from CI, independently verify artifact digest/member set/version, synchronize with a guarded exact-parent/self-deleting workflow, and require branch zero-diff qualification.
6. Re-fetch live `main`; fast-forward only if its base is unchanged. Run ordinary exact-main CI and independently verify the final delivery artifact/workspace.
7. Curate the repository: README, backlog, implementation status, changelog, machine-readable evidence/handover and recursive handover prompt must all agree with the final exact-main authority.
8. Next tranche after freeze: Video fixed typed actions/source matrix, still under the same strict worker/provider architecture.
9. Remaining launch work is separate: headed Windows 11 Explorer UI/crash matrix, production signing/B2, production FFmpeg redistribution approval, signed MSIX lifecycle, final fuzz/chaos/security/release/end-user acceptance, UX/settings and plugin SDK.
