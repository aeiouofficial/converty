# Transactional output commit

**Status:** Accepted for planning baseline.

## Decision
Workers write only private temporary output. Host validates and performs final collision-safe commit.

## Rationale
Prevents partial/corrupt output from masquerading as success and protects existing user files from worker failures.

## Consequences
Implementation complexity is accepted in exchange for smaller blast radius, stronger testability, and future-proof provider isolation. Any reversal requires a new ADR and threat-model update.
