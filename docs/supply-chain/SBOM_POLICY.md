# FileConvert SBOM Policy

## Purpose
FileConvert publishes deterministic software-bill-of-materials evidence without inventing dependency state. Source-only SBOM generation is allowed before managed restore. Release SBOM generation is fail-closed until every managed project has a reviewed `packages.lock.json`.

## Formats and scope
- SPDX JSON 2.3 is the canonical generated format.
- `source` mode inventories first-party managed projects only and is development evidence, not a release SBOM.
- `release` mode inventories first-party projects plus NuGet packages parsed from committed lock files.
- The generator never contacts a package feed and never fabricates package versions.
- Generated documents are deterministic for a fixed workspace version and input graph.

## Release gate
A shipping/release claim requires: all managed lock files present, locked restore passing, release SBOM generated after that restore, human review of dependency/license metadata, and the shipping artifact/hash manifest bound to the same workspace version.

## Non-claims
The source-only SBOM does not prove runtime dependencies, licenses, vulnerabilities, binary provenance, or redistribution rights. Those remain release-gate work.
