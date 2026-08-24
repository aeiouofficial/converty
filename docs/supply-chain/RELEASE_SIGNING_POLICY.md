# FileConvert Release Signing Policy

## Trust model
Signing keys are external release infrastructure. Private keys, PFX/P12 bundles, PEM private material, secrets, tokens, and `.env` files must never be stored in or packaged from the FileConvert workspace.

## Required release behavior
- Hash manifests use SHA-256 or stronger; SHA-1 and MD5 are forbidden for release integrity.
- Production Windows binaries/packages must be Authenticode/MSIX signed by an approved identity and timestamped by an approved timestamp service.
- Signing happens only after build/test/SBOM gates pass and before final release verification.
- Signature verification is a separate required release gate; presence of a signature file is not proof of a valid signature.
- The release process must record signer identity/thumbprint, timestamp result, artifact digest, and verification command output.

## Workspace packaging rule
The deterministic workspace packager excludes common private-key/secret file forms by construction. This is defense in depth, not a substitute for secret scanning or release-host access controls.
