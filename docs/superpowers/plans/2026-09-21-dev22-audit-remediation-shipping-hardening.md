# Dev.22 audit remediation and shipping-readiness hardening

**Date:** 2026-09-21  
**Repository:** `aeiouofficial/converty`  
**Branch:** `dev/0.1.0-dev.22-shipping-readiness`  
**Base:** `1bfb6a409976abdbf781ba027f556818fb791fa7`  
**Status:** IN PROGRESS  
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
