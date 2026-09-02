# Dev.19 frozen exact-main evidence

## Authority anchor
- Version: `0.1.0-dev.19`
- Exact frozen `main`: `eb0ce66dab646427d5bef1548c12e5cc4765b2f1`
- Tree: `337a4e11fb41bab6b6eeb462c3755381580f06c1`
- Exact-main run: `33597504612`
- `main-authority-continuity` job `100143814059`: SUCCESS
- `supply-chain-static` job `100143814189`: SUCCESS
- `managed` job `100143814261`: SUCCESS

This authority is immutable evidence. Any later metadata-only dev.19 handover commit is not a replacement frozen tree.

## Exact-main tests and product gates
- locked restore: PASS
- dependency audit: 18 projects / 18 frameworks / 0 vulnerable-result packages
- Release build: 0 warnings / 0 errors
- native Explorer topology/build: PASS
- development package validation: PASS; unsigned qualification package only
- direct shell class-factory invoke: PASS
- loose-package COM activation/invoke: PASS
- packaged Bridge → Strict Worker/provider → app-local FFmpeg: PASS
- Audio source/action matrix: 36/36 PASS
- repeated malformed/truncated Audio rejection: PASS, deterministic Bridge exit `4`
- Audio mixed-batch failure isolation: PASS twice
- Image source/action matrix: 24/24 PASS
- repeated malformed/truncated Image rejection: PASS, deterministic Bridge exit `4`
- Image mixed-batch failure isolation: PASS twice
- managed tests: 255/255 PASS
- static tests: 99/99 PASS
- raw contract vectors: 5/5 PASS
- tracked generated-authority zero-diff: PASS

## Final deterministic workspace
- SHA-256: `167b4695cca6810fe0e36e57c45a7bf11483105c0e71b955a948604f2cd9e584`
- bytes: `484507`
- ZIP entries: `378`
- root: `Converty_0.1.0-dev.19/`
- package-manifest entries: `376`
- SHA256SUMS entries: `377`
- CRC: PASS
- deterministic double build: PASS
- exclusion policy: PASS

## Exact-main artifacts
Generated authority:
- artifact ID `9833901138`
- digest `sha256:255e9328231e0951368b468308774f1df97dc05a0b05094f8ea4d1f566749ed6`

Verified delivery:
- artifact ID `9833955082`
- outer digest `sha256:6e7b7e753a4101a3e690ffe558c96e47f9dba831b858bed5f0005ec144837809`
- outer size `504509` bytes
- exact outer members:
  - `Converty_0.1.0-dev.19_HANDOVER_PROMPT.txt`
  - `Converty_0.1.0-dev.19_build_evidence.json`
  - `Converty_0.1.0-dev.19_full_workspace.zip`
  - `package-evidence.json`

Independent download verification confirmed:
- outer digest matches GitHub Actions upload digest;
- outer CRC PASS;
- exact four outer members only;
- nested workspace digest matches `167b4695cca6810fe0e36e57c45a7bf11483105c0e71b955a948604f2cd9e584`;
- nested CRC PASS;
- all 378 entries rooted under `Converty_0.1.0-dev.19/`;
- VERSION and package-manifest workspaceVersion both `0.1.0-dev.19`;
- all 376 package-manifest hashes/byte counts pass;
- all 377 SHA256SUMS entries pass;
- zero obvious excluded `artifacts`, `bin`, `obj`, `.git`, `.pytest_cache`, or `__pycache__` paths.

## Authority synchronization provenance
- behavior anchor: `8a64d0a12ccf47df5df364a1c6c545f876d57d29`, run `33596229372`
- pre-authority curation: `d88412c9cdac13450806b34db7737be95190e314`
- curated generated-authority artifact: `9833673969`, SHA-256 `d14ddad32701283f13dd587b079a151743211c8f5935fa5f1373225600bf6f61`, CRC/member/version verification PASS
- guarded sync staging: `fe3b33fe367741c7b748c2ab9863799d86867b55`
- self-deleting authority sync: `d5a8e10dd43c742660574dbd9b54848df7d82421`
- same-tree qualification: `eb0ce66dab646427d5bef1548c12e5cc4765b2f1`
- branch qualification run: `33597141220`
- pre-promotion main: `2ecf65d6131568c23e6fb4bbfe2371b6bc978407`
- compare: 29 ahead / 0 behind
- promotion: non-force fast-forward only

## Explicitly not proven by this freeze
- headed Windows 11 modern Explorer exact-build UX/screenshots/crash-hang-failure acceptance
- production FFmpeg/ffprobe redistribution/provenance/signature/hash/license/notices approval
- production signed-package B2 identity/authentication
- signed production MSIX clean install/update/uninstall
- final fuzz/chaos/security/release/end-user acceptance

Those remain open and must not be inferred from automated CI.