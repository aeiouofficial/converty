# Modern Windows 11 Explorer command

**Status:** Accepted for planning baseline.

## Decision
Use native C++ IExplorerCommand registered through package identity. Registry-only legacy verbs may be a developer fallback but are not the product UX.

## Rationale
Microsoft explicitly documents IExplorerCommand + package identity for the modern Windows 11 context menu and requires menu-path methods to remain fast.

## Consequences
Implementation complexity is accepted in exchange for smaller blast radius, stronger testability, and future-proof provider isolation. Any reversal requires a new ADR and threat-model update.
