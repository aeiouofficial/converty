# Converty
<img width="1536" height="1024" alt="Converty Architecture Blueprint" src="https://github.com/user-attachments/assets/985f38c5-5c04-4b45-b69f-5deb0cdcc374" />

Windows 11 modern-context-menu file conversion platform. Converty is being built as a modular right-click converter for Audio, Images, Video, and future file families while keeping Explorer, coordinator, worker, media-engine, staging, and publication trust boundaries explicit.

## Workspace version
**0.1.0-dev.10** — B4 disposable-worker containment behavior is qualified. The first generated-authority tree is also fully qualified; one final regeneration/requalification is required because recording that qualification changes repository bytes.

## Current evidence-backed state
The immutable dev.10 B4 behavior head is `f221563c790057344a94b4e60c309d4512a77c38`, qualified by GitHub Actions run `33028554361` (managed `98375493893`, static `98375494099`). The contained product path is:

`IExplorerCommand → fixed Converty.Bridge.exe → Strict Converty.EngineWorker.exe → typed preset/provider → fixed app-local ffmpeg.exe → private staging → validated no-overwrite numbered publication`

Behavior evidence includes 18/18 locked restore, zero vulnerable-result packages, Release build with 0 warnings/errors, native Explorer, development FFmpeg/ffprobe 9.0.1, MakeAppx, direct/package COM invocation, strict Bridge→worker→FFmpeg conversion, Unicode/metacharacter paths, source/external-destination preservation, numbered publication, MP3 exactly 320000 bit/s, 190/190 managed tests, 66/66 static tests, 5/5 vectors, strict filesystem/network canaries, Job Object containment, and the output-growth ceiling canary.

## First generated-authority qualification
Generated authority was frozen on tree `af16e15820985e787e54fb0c659cf6005bd4df89` and qualified at commit `529216b3676b97e7a9e0b78333c2229ed3396794`, run `33035768679`, managed job `98397998679`, static job `98397998510`.

That run was fully green:
- generated source/release SBOM, package manifest and SHA256SUMS regeneration: PASS;
- tracked generated-authority diff: CLEAN;
- Release/product/test gates: PASS; managed tests **190/190**, static tests **66/66**;
- deterministic workspace ZIP double build: PASS;
- `Converty_0.1.0-dev.10_full_workspace.zip`: SHA-256 `ed2fd33e376eef060f9342a77a48cdff40a9e2c95e0c6dc2d0ef98c557197241`, 377093 bytes, 328 files;
- 326 package-manifest entries and 327 SHA256SUMS entries verified;
- ZIP reopen/CRC: PASS;
- exclusion policy: PASS;
- verified delivery artifact upload: PASS, artifact `9631969967`, digest `23de3e391ddb76ef8ddbf70c05f22a3fcc307a621692dc9759001c80741ad119`.

This evidence is now being recorded in source authority. Because that recording changes repository bytes, dev.10 is not yet final: generated package/hash authority must be regenerated from the evidence-frozen tree and the resulting exact tree independently requalified once more.

## What dev.10 still does not claim
Converty is not shipped or production-ready. These gates remain open:
- real headed Windows 11 modern Explorer context-menu visibility/usability and exact-build screenshots;
- Explorer crash/hang/failure headed matrix;
- remaining B2 connected-server anti-squatting, final status/cancel wire decision, and replay/disconnect/session acceptance;
- production FFmpeg redistribution/license/notices/signature/hash approval; the Gyan payload is development qualification input only;
- signed production MSIX and clean Windows 11 VM install/update/uninstall;
- final security/fuzz/chaos/release audit and end-user shipping acceptance.

## Start here
1. `docs/HANDOVER_NEXT_AGENT.md`
2. `machine-readable/handover_state.json`
3. `machine-readable/build_evidence.json`
4. `docs/development/IMPLEMENTATION_STATUS.md`
5. `docs/TASK_BACKLOG.md`
6. `docs/Converty_Master_Build_Plan.md`
7. `docs/adr/ADR-013-dev9-functional-product-spike.md`
8. `docs/SECURITY_THREAT_MODEL.md`
9. `docs/TEST_AND_RELEASE_GATES.md`
10. `docs/security/B2_SERVER_AUTH_GATE.md`
11. `docs/supply-chain/SBOM_POLICY.md`
12. `docs/supply-chain/RELEASE_SIGNING_POLICY.md`

## Verification
On Windows with .NET SDK `10.0.400`:
```powershell
./build/bootstrap.ps1
./build/dependency-audit.ps1
./build/build.ps1 -Configuration Release
./build/native-smoke.ps1
./build/prepare-dev-ffmpeg.ps1
./build/stage-dev-package.ps1 -Configuration Release -FfmpegPath ./artifacts/dev-ffmpeg/ffmpeg.exe
./build/validate-dev-package.ps1
./build/explorer-registration-smoke.ps1
./build/product-conversion-smoke.ps1
./build/test.ps1 -Configuration Release
```

Supply-chain/static verification:
```bash
python scripts/verify_ci_actions.py
python scripts/verify_release_inputs.py
python scripts/generate_sbom.py --mode source
python scripts/generate_sbom.py --mode release
python scripts/generate_package_manifest.py
python scripts/generate_hash_manifest.py
python scripts/verify_repository.py
python scripts/verify_contract_vectors.py
python -m pytest -q tests/static
```

Complete snapshots use `Converty_<VERSION>_full_workspace.zip`; build caches, `.git`, package caches, Python bytecode, `.env`, and common private-key formats are excluded.
