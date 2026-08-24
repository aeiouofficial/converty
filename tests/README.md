# Tests

- `Converty.Contracts.Tests`: managed contract/identifier validation plus seeded identifier properties.
- `Converty.Core.Tests`: managed registry/capability/planner/output/fake-provider tests plus seeded capability/output properties.
- `Converty.Serialization.Tests`: managed strict JSON round-trip and adversarial parsing tests.
- `static`: executable Python tests for schema validity, toolchain/package policy, source/security boundaries, and cross-checks between schema and serializer source.

The managed projects use xUnit v3 on Microsoft Testing Platform. Their existence is not evidence that they passed; handover/release status must distinguish authored tests from executed results.
