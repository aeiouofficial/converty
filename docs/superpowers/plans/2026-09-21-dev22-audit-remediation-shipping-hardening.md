# Dev.22 audit remediation and shipping-readiness hardening

**Date:** 2026-09-21  
**Repository:** `aeiouofficial/converty`  
**Branch:** `dev/0.1.0-dev.22-shipping-readiness`  
**Base:** `1bfb6a409976abdbf781ba027f556818fb791fa7`  
**Status:** TASKS 1-8 QUALIFIED ON SUBJECT 1aea099a; POST-QUALIFICATION DOCUMENTATION RESYNC REQUIRED  
**Release status:** NOT CUSTOMER SHIP-READY

## Purpose
Close the concrete defects confirmed by the 2026-09-21 critical audit without weakening security, evidence, or exact-candidate invariants. Repository logic may become complete while external production evidence remains legitimately OPEN.

## Non-negotiable invariants
- Frozen `main` remains `8a1f46603aa842728247bc11b34fcccf121858fd` until real release gates are satisfied.
- Dev.21 candidate `c3bc042aea3154e30ce2720db4722cbda27bcb34` remains immutable.
- No private signing key enters the workspace or ordinary CI.
- Production FFmpeg/MSIX/Windows-11/end-user evidence stays OPEN unless tied to real immutable artifacts and exact candidate authority.
- No generated SBOM/package/hash authority is hand-edited.
- Behavior changes follow RED -> GREEN -> regression verification.
- Correct code is left unchanged unless a concrete audited failure path requires modification.

## Work packages

### Task 1 — Release-gate state machine and exact-candidate authority
Replace tests that permanently require tracked OPEN state with state-machine fixtures; require live governance for authoritative readiness; bind approvals to repository/candidate/tree/artifact/workflow evidence; reject stale evidence.

### Task 2 — Governance/readiness integration
Remove the dev-only manual-workflow bootstrap dependency; expose readiness as a normal named CI check; validate the exact active main-applicable ruleset; separate pre-promotion ancestry from post-promotion continuity.

### Task 3 — Production FFmpeg evidence hardening
Validate hashes and structured evidence; reject development FFmpeg authority as production approval; require immutable production provenance bound to the exact candidate.

### Task 4 — Production MSIX/B2 evidence hardening
Validate package/signer hashes and structured signature/timestamp/PFN/B2/lifecycle evidence; tie approval to one exact signed production artifact; keep development identity/unsigned packages ineligible; add reproducible verification without storing keys.

### Task 5 — Workspace secret-content gate
Add reproducible content-level secret/private-key scanning to release preflight/CI while retaining filename/suffix exclusions as defense in depth.

### Task 6 — Batch outcome and failure-isolation repair
Expand the per-member boundary across validate -> resolve -> stage -> execute -> validate -> publish; keep cancellation/global faults fail-fast; return structured per-file outcomes and report partial success.

### Task 7 — Transactional publication and staging recovery
Publish through an owned destination-volume temporary file and same-volume no-overwrite rename; preserve collision-race handling; add safe stale owned-staging cleanup.

### Task 8 — Dev.22 authority closure
Bump coherent non-generated authority to `0.1.0-dev.22`; reconcile docs/evidence; regenerate generated authority only through ordinary CI/guarded sync; exact-candidate qualify; do not promote while genuine external gates remain OPEN.

## Acceptance / review focus
Final review must prove that release evidence cannot be satisfied by arbitrary strings, approvals are candidate/artifact-bound, live governance is inspected, promotion checks are not circular, partial batch failures remain observable, publication is crash-safe across volumes, secret scanning covers content, and no external evidence is fabricated.


## Ruling — approval binding without self-referential Git SHAs
A tracked approval manifest cannot embed the SHA/tree of the commit that contains that manifest: changing the manifest changes the tree and commit SHA. Dev.22 therefore binds external production approvals to an exact `subjectCommitSha` + `subjectTreeSha` + artifact SHA-256 + qualification workflow evidence. Final readiness must prove that the current candidate descends from that subject and that every post-subject change is restricted to an explicit evidence/generated-authority allowlist, then require fresh exact-current-candidate CI. Any product/code/workflow change after the subject invalidates the approval and requires requalification.


## Pre-authority qualification evidence — 2026-09-21
Exact behavior head `7e50fa9a1df4d0a8336d16596af488335424b792` / tree `a3a6deec5d753c6176fb40368a56d263b3beaa62`; CI `35664296931`: 395/395 managed, 162/162 static, 5/5 vectors, dependency audit PASS, Release 0 warnings/errors, all packaged product matrices/mixed batches/mode witnesses PASS. Deterministic workspace SHA-256 `526588af945a4eeb04984dc17792af8dd10fc1d7f2a9e9006ab3976d448b2b0a`, 642060 bytes / 462 files, double-build PASS. Semantic packaging stops only on stale tracked generated authority. Tasks 1-7 are complete; Task 8 is metadata → CI authority generation → independent verification → guarded sync → exact-candidate qualification.

## Exact subject qualification — 2026-09-24

`1aea099a2878f72f8c95a5cd3bd1dee98a1f98b3` / tree `8369b929f3e8b48f76f720ae3a954818acf2a25a` / CI `35937520600`: managed 395/395, static 162/162, vectors 5/5, full packaged product regressions PASS, tracked generated authority zero-diff PASS. Independent delivery artifact 10783368416 has GitHub SHA-256 `2697a85aa3b2a5f51f1446afb37e348bb242177dc29b11dbf3b8416b422ba207`; nested 462-entry workspace ZIP SHA-256 `337bdb018274866803e8b6e15f7b9ea0fad4436ac3d2b912d61f340cfc76cf47`, CRC and all 460 package-manifest/461 SHA256SUMS rows match. Generated artifact 10783376753 has SHA-256 `c3b79e9db86c52585c9c3db00d05504d43a0b6991f6c323c9f6cdf302eb1df59`.

Ruling: the user's local Converty project must not allocate scratch/cache/logs on C:. The local verified migration and D: bootstrap are documented in AGENTS.md; the repository bootstrap and regression guard are added in the post-qualification documentation amendment. The amendment changes the tracked tree, so repeat ordinary CI generation, independently verify the new artifact, synchronize only the four CI-generated authority files and qualify the resulting exact PR head again. Keep the external shipping gates in GitHub issue #13 OPEN and draft PR #14 unmerged.

## Local qualification addendum — 2026-09-24
GitHub Actions minutes are exhausted; current user scope is local CI on `D:\converty` plus durable GitHub commits and in-place repository docs. The test-only reparse fixture correction and deterministic LF generator fix have RED/GREEN regression evidence. Local managed 395/395, static 168/168, vectors 5/5, native MSVC and dependency audit PASS; the pinned development FFmpeg/package matrix and deterministic final archive must be rechecked on the exact amended tree before qualifying it. Regenerate the four deterministic authority files with their generators, never manually edit their values, and verify repeatable output and complete archive-member hashes. No earlier GitHub Actions run covers the amended tree.
