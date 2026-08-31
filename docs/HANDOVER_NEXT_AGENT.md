# Converty continuation handover — dev.17 candidate

Repository: `https://github.com/aeiouofficial/converty`  
Default branch: `main`

Read `docs/HANDOVER_PROMPT.txt` first; it is the canonical recursive continuation prompt for this tranche.

## Candidate authority
- Version: `0.1.0-dev.17`
- Behavior head: `5829c868c5d192c70f21ea0da9337250a8d9c961`
- Behavior run: `33347652162`
- Managed: `99354775361`
- Static: `99354775208`
- Prior frozen main: `dca3cbcba326a35801bc442ec93f16d84f58a692`

Dev.17 behavior is green but finality still requires version-aligned generated-authority sync, branch zero-diff qualification, non-force main promotion and a fully green exact-current-main ordinary CI run. Do not skip those gates and do not overwrite newer concurrent work.

## What dev.17 proves
A real five-file same-family Audio selection with valid, malformed and truncated inputs is processed by one Bridge process. Ordinary per-file conversion failures no longer suppress later valid items. Successful outputs survive, bad inputs publish nothing, collisions number deterministically, sources/pre-existing destinations are preserved and no partial residue remains. The matrix is exercised twice.

## Next after freeze
Dev.18 starts the first fixed typed Image action matrix through the existing Strict Worker/provider boundary. Preserve all Audio qualification and do not weaken containment or executable/preset trust boundaries.
