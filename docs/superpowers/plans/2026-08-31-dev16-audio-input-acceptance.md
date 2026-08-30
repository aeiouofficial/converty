# Dev.16 Audio source and malformed-input acceptance implementation plan

1. RED: add static contract requiring a dedicated Audio input acceptance smoke and ordinary-CI wiring.
2. Implement deterministic source fixtures and 6×6 fixed Audio action product-path matrix under excluded `artifacts/`.
3. Add malformed/truncated repeated negative cases with bounded process deadline and transactional preservation checks.
4. Run ordinary Windows CI and treat any hang/failure as a product defect rather than weakening the matrix.
5. If a defect appears, use systematic root-cause debugging and add a second RED regression contract before fixing it.
6. Require restore/audit/Release/native/package/COM/existing product smoke/new matrix/managed/static/vector gates to pass before versioning.
7. Version as `0.1.0-dev.16`, update all human and machine-readable authority, then synchronize only CI-generated SBOM/package/hash files from one exact artifact.
8. Require branch generated-authority zero-diff qualification.
9. Re-read live `main`; if still the frozen dev.15 SHA, non-force fast-forward only. Never overwrite concurrent work.
10. Require exact-current-main continuity + managed + supply-chain-static SUCCESS, deterministic verified workspace and verified-delivery artifact before calling dev.16 frozen.
