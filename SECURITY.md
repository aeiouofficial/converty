# Security Policy

## Supported versions
Only the newest `0.x` development line is actively maintained before the first stable release.

## Reporting
Do not put suspected vulnerabilities or exploit samples into public issues. Use the repository owner's private security reporting channel once repository hosting is configured.

## Architecture invariants
- Explorer-loaded code is trigger-only and performs no conversion, media parsing, or network work.
- The Host does not parse untrusted media or load codec/plugin binaries.
- Worker processes are the parser/codec trust boundary and must fail closed if strict isolation cannot be applied.
- Ordinary local conversion has no network requirement.
- Output becomes user-visible only after validation and atomic commit.

See `docs/SECURITY_THREAT_MODEL.md` for the complete authority.
