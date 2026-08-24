# No untrusted parser in coordinator

**Status:** Accepted for planning baseline.

## Decision
All substantive probing/decoding/conversion runs in disposable workers. Host consumes bounded typed results only.

## Rationale
Media parsers are the highest-risk attack surface. Process isolation materially limits blast radius and avoids loading codec/plugin code into the durable coordinator.

## Consequences
Implementation complexity is accepted in exchange for smaller blast radius, stronger testability, and future-proof provider isolation. Any reversal requires a new ADR and threat-model update.
