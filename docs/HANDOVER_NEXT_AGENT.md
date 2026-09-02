# Converty continuation handover — dev.19 Image batch isolation

Repository: `https://github.com/aeiouofficial/converty`  
Default branch: `main`

Read `docs/HANDOVER_PROMPT.txt` first; it is the canonical context-free recursive continuation prompt. Re-fetch live refs before every write or completion claim.

## Frozen baseline
Dev.18 exact-main authority: commit `ef079f7e7923e399624067c4d54b9ce7577bf090`, tree `0af729f150897d170eac9f9aebfd5bc7d5d4083a`, run `33390111824`. Managed `99481546832`, static `99481546572`, continuity `99481546655` all SUCCESS. Workspace SHA-256 `4be8d5a2f503a8a885347b647bbd0aa0b61ce6d56b3ac39d9af7b11fb801628a`.

## Dev.19 current line
Branch: `dev/0.1.0-dev.19-image-batch-isolation`.
Current work adds real Image mixed-batch acceptance without changing the trust architecture. One Bridge process receives valid → malformed → valid → truncated → valid Image members. Ordinary per-file failures do not suppress later valid members; aggregate failure is reported after the selection; valid outputs publish numbered/no-overwrite; invalid members publish nothing; sources and pre-existing destinations remain unchanged; partial staging and converter processes must be cleaned.

## Evidence components
- `build/image-batch-isolation-smoke.ps1`
- `tests/static/test_dev19_image_batch_isolation.py`
- `tests/Converty.Core.Tests/Execution/ImageBatchIsolationTests.cs`
- `.github/workflows/ci.yml`
- `docs/development/DEV19_IMAGE_BATCH_ISOLATION_TDD_EVIDENCE.md`
- `docs/superpowers/specs/2026-09-02-dev19-image-batch-isolation-design.md`
- `docs/superpowers/plans/2026-09-02-dev19-image-batch-isolation.md`

## Current status
The branch has passed the static contract on the corrected CI run and has an active Windows managed qualification. Do not call dev.19 frozen until the latest managed run completes and the authority sequence succeeds.

## Required freeze sequence
1. Finish the latest Windows managed qualification.
2. Preserve complete historical machine-readable evidence vocabulary while adding dev.19 fields.
3. Generate source/release SBOM, package manifest and SHA256SUMS only in CI.
4. Independently verify generated artifact digest, exact four-member set, CRC and workspace version.
5. Guarded exact-parent sync, self-delete temporary workflow.
6. Branch zero-diff qualification.
7. Re-fetch live main and fast-forward only if base unchanged.
8. Fresh exact-main three-job SUCCESS.
9. Independently verify final deterministic workspace and verified delivery.
10. Re-read both refs and curate all status/handover metadata to the exact final state.

## Next after freeze
Create the next branch from exact frozen main and begin Video foundation test-first. Preserve all Audio 36-case + malformed/truncated + mixed-batch and Image 24-case + malformed/truncated + mixed-batch regression gates.

## Explicitly open launch gates
Headed Windows 11 modern Explorer exact-build UI/screenshots and crash/hang/failure matrix; production signed-package B2 identity/authentication; production FFmpeg/ffprobe redistribution/license/notices/signature/hash approval; signed production MSIX install/update/uninstall; final fuzz/chaos/security/release/end-user acceptance; UX/settings; plugin SDK manifest/API/signature/hash gate.
