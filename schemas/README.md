# Versioned schemas

`schemas/v1` is the external machine-readable contract authority for conversion request, preset, provider capability, conversion plan, format descriptor, and job-status shapes. Schemas are strict (`additionalProperties: false`) to reject accidental or malicious field smuggling, including executable command fields at root-contract level.

Path-bearing schemas also reject embedded NUL. The C# `Converty.Serialization` adapter adds an additional parser-hardening layer by rejecting duplicate JSON member names recursively before domain mapping.

Changing an existing v1 schema incompatibly is prohibited. Add a new schema version and an explicit migration path instead. Until such a version exists, every version other than `1` is rejected.
