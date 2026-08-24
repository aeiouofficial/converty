# No silent isolation downgrade

**Status:** Accepted for planning baseline.

## Decision
A job marked StrictRequired fails closed if the required isolation profile cannot be established.

## Rationale
Silent fallback converts a security requirement into a suggestion and makes the effective security state unknowable.

## Consequences
Implementation complexity is accepted in exchange for smaller blast radius, stronger testability, and future-proof provider isolation. Any reversal requires a new ADR and threat-model update.
