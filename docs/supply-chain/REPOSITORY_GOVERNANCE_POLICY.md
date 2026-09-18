# Repository Governance Policy

## Desired future main ruleset

This file defines the desired freeze policy for `main`. It **does not claim live configuration**. Live GitHub ruleset and branch-protection state must be read from GitHub and recorded as operational evidence before a freeze.

The future freeze ruleset must:

- **block force pushes** to `main`;
- **block deletions** of `main`;
- **restrict updates** to the approved release path and authorized maintainers or release automation;
- require **linear history** so an already-qualified exact candidate is not replaced by a merge-generated post-qualification SHA;
- require the exact named checks `main-authority-continuity`, `supply-chain-static`, and `managed` before an update is accepted;
- require **verified signatures** for future freeze commits once the release-signing path is qualified.

The signature rule is prospective. It does not retroactively invalidate the **historical unsigned dev.20** authority, and it must not rewrite historical commits to manufacture signatures.

## Exact-candidate promotion invariant

The promotion model is an **exact candidate** model: qualify the candidate SHA, preserve that SHA, and move `main` only when repository governance can accept that same qualified commit. Do not create a merge commit, squash commit, or other post-qualification SHA merely to satisfy governance.

If verified-signature enforcement would require changing the already-qualified candidate SHA, stop and qualify the signed candidate before promotion instead of weakening the rule.

## Live-state evidence

Source files describe policy, not repository reality. A release/freeze record must include a fresh GitHub read of rulesets, branch state, required checks, and signature enforcement. If GitHub reports no applicable ruleset, the release gate remains open.
