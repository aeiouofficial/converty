# Converty continuation handover — dev.18 candidate

Repository: `https://github.com/aeiouofficial/converty`  
Default branch: `main`

Read `docs/HANDOVER_PROMPT.txt` first; it is the canonical recursive continuation prompt for this tranche.

## Candidate authority
- Version: `0.1.0-dev.18`
- Behavior head: `6075aa3973b75e170cb5f9b812a8ca3b9b71f528`
- Behavior run: `33350141373`
- Managed: `99361743241`
- Static: `99361743276`
- Prior frozen main: `8b2756910b58b678745e6fda89866ed3bf545474`

Dev.18 behavior is green but finality still requires version-aligned generated-authority sync, branch zero-diff qualification, non-force main promotion and a fully green exact-current-main ordinary CI run. Do not skip those gates and do not overwrite newer concurrent work.

## What dev.18 proves
All eight advertised Image source extensions are exercised against all three fixed Image actions through packaged Bridge→Strict Worker/provider→app-local FFmpeg. 24/24 conversions pass with codec/dimension checks, source/pre-existing-destination preservation, numbered publication and zero partial residue. Repeated malformed/truncated Image inputs reject deterministically with exit 4. Existing Audio gates remain green. No product-code change was necessary because the fixed Image path already satisfied the acceptance contract.

## Next after freeze
Dev.19 is Image multi-file/mixed-valid-invalid failure isolation and final Image matrix closure. Video expansion remains blocked until that tranche is frozen.
