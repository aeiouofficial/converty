# dev.13 Status/Cancel TDD Evidence

This file preserves development evidence for the dev.13 authenticated status/cancel wire tranche. It is not release qualification authority; final authority must be an ordinary successful CI run on the exact current `main` HEAD with generated authority current.

## Task 1 RED

- Branch: `dev/0.1.0-dev.13-status-cancel-red`
- RED HEAD: `beabdc7fe0fec0e9d2e0f9f6add4fefa9eaa593b`
- Workflow run: `33285343578`
- Managed job: `99187464826`
- Supply-chain/static job: `99187464721`
- Main-authority-continuity job: `99187464792`

Observed Windows behavior:

- locked restore: PASS
- NuGet vulnerability audit: PASS
- Release build: FAIL after the new job-control contract/serialization tests were added and before production `JobControl*` contracts/serializers existed
- native/package/COM/product/test/package-delivery steps: skipped after the build failure

Observed static behavior:

- immutable CI action pins: PASS
- source/release SBOM generation: PASS
- release-input verification: PASS
- package/hash generation: PASS
- raw contract vectors: PASS
- Python static gates: PASS
- tracked generated-authority zero-diff gate: FAIL because the RED test bytes changed tracked workspace authority

The side-branch main-authority-continuity job also failed as designed because side branches are not repository authority. Neither that continuity failure nor generated-authority staleness is the Task 1 behavioral RED.

## Task 1 GREEN candidate

- Behavior candidate commit: `86116777076bd66374bc7559617315d80c8d699f`
- Tree: `5616081392a63875149af80a366aafa76c00659e`
- Parent: RED HEAD `beabdc7fe0fec0e9d2e0f9f6add4fefa9eaa593b`
- Branch: `dev/0.1.0-dev.13-status-cancel-green1`

The candidate adds only the typed job-control domain contracts and strict V1 JSON adapters required by the Task 1 RED tests. Qualification results must be appended or transferred into the final tranche handover after CI completes; this section makes no green claim by itself.
