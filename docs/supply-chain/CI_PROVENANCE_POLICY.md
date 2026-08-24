# Converty CI Provenance Policy

## Purpose
CI is executable supply-chain input. A mutable GitHub Action tag or branch may change what code executes without a Converty source change, so all external Actions are treated as reviewed dependencies.

## Required workflow policy
- Every external `uses:` reference under `.github/workflows/` is pinned to a full 40-character Git commit SHA.
- The reviewed semantic release remains in a trailing comment, for example `# v7.0.1`.
- `machine-readable/ci_action_pins.json` is the authority mapping action name to reviewed release and exact SHA.
- `scripts/verify_ci_actions.py` fails on mutable tags/branches, unknown external actions, SHA drift, missing release comments, or comments that disagree with the pin manifest.
- Local actions (`./...`) and Docker references are not covered by the external Action pin manifest and require their own review if introduced.
- Dependabot may propose Action updates, but an update is not accepted until the new immutable SHA/release pair is reviewed and both workflow and manifest are changed together.

## Current reviewed actions
The dev.4 baseline records `actions/checkout`, `actions/setup-python`, and `actions/setup-dotnet` only. Adding another external Action requires adding it to the machine-readable pin authority and passing the verifier before merge.

## Non-claims
A pinned SHA prevents tag retargeting from silently changing the fetched Action revision. It does not by itself prove the upstream revision is benign, eliminate runner compromise, or replace repository/environment permission controls.

## Credential and execution containment

- Checkout MUST set `persist-credentials: false`; CI verification/build jobs do not push and must not retain a repository token in Git configuration.
- Every CI job MUST have an explicit finite `timeout-minutes` value. The static provenance job is capped at 15 minutes and the managed build/test job at 30 minutes.
- Workflow permissions remain least-privilege (`contents: read`) unless a separately reviewed workflow requires narrower or different permissions.
