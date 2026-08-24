#!/usr/bin/env python3
from __future__ import annotations

import json
import re
import sys
import xml.etree.ElementTree as ET
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]

REQUIRED_FILES = [
    "VERSION",
    "global.json",
    "Directory.Build.props",
    "Directory.Packages.props",
    "NuGet.Config",
    "Converty.slnx",
    "src/Converty.Contracts/Converty.Contracts.csproj",
    "src/Converty.Core/Converty.Core.csproj",
    "src/Converty.FakeProviders/Converty.FakeProviders.csproj",
    "src/Converty.Serialization/Converty.Serialization.csproj",
    "src/Converty.Serialization/ContractJson.cs",
    "tests/Converty.Contracts.Tests/Converty.Contracts.Tests.csproj",
    "tests/Converty.Core.Tests/Converty.Core.Tests.csproj",
    "tests/Converty.Serialization.Tests/Converty.Serialization.Tests.csproj",
    "docs/superpowers/specs/2026-08-24-foundation-design.md",
    "docs/superpowers/plans/2026-08-24-foundation-implementation.md",
    "docs/superpowers/plans/2026-08-24-foundation-dev2-implementation.md",
    "docs/superpowers/plans/2026-08-24-foundation-dev3-implementation.md",
    "docs/superpowers/plans/2026-08-24-foundation-dev4-implementation.md",
    "docs/supply-chain/SBOM_POLICY.md",
    "docs/supply-chain/RELEASE_SIGNING_POLICY.md",
    "docs/supply-chain/CI_PROVENANCE_POLICY.md",
    "machine-readable/release_policy.json",
    "machine-readable/ci_action_pins.json",
    "machine-readable/source_sbom.spdx.json",
    "scripts/generate_sbom.py",
    "scripts/verify_release_inputs.py",
    "scripts/verify_ci_actions.py",
    "scripts/verify_dependency_audit.py",
    "build/dependency-audit.ps1",
    "scripts/verify_contract_vectors.py",
    "tests/vectors/v1/manifest.json",
]

REQUIRED_SOURCE_TOKENS = {
    "src/Converty.Contracts/SchemaVersions.cs": ["Current = 1"],
    "src/Converty.Contracts/Identifiers/IdentifierRules.cs": ["IsValid"],
    "src/Converty.Core/Capabilities/CapabilityGraph.cs": ["sealed class CapabilityGraph"],
    "src/Converty.Core/Planning/ConversionPlanner.cs": ["sealed class ConversionPlanner"],
    "src/Converty.Core/Output/OutputPathResolver.cs": ["sealed class OutputPathResolver"],
    "src/Converty.Serialization/ContractJson.cs": [
        "JsonUnmappedMemberHandling.Disallow",
        "Unsupported schema version",
        "PropertyNameCaseInsensitive = false",
    ],
}

FORBIDDEN_ENGINE_INDEPENDENT_TOKENS = [
    "Process.Start(",
    "ProcessStartInfo",
    "ffmpeg",
    "ffprobe",
    "cmd.exe",
    "powershell.exe",
    "HttpClient",
    "Socket",
    "NamedPipe",
    "DllImport",
    "LibraryImport",
]


def fail(message: str) -> None:
    print(f"FAIL: {message}")
    raise SystemExit(1)


def main() -> int:
    missing = [p for p in REQUIRED_FILES if not (ROOT / p).is_file()]
    if missing:
        fail("required files missing: " + ", ".join(missing))

    version = (ROOT / "VERSION").read_text(encoding="utf-8").strip()
    if version != "0.1.0-dev.4":
        fail(f"VERSION must be 0.1.0-dev.4, got {version!r}")

    global_json = json.loads((ROOT / "global.json").read_text(encoding="utf-8"))
    sdk = global_json.get("sdk", {})
    if sdk.get("version") != "10.0.400":
        fail("global.json must pin .NET SDK 10.0.400")
    if sdk.get("rollForward") != "latestPatch":
        fail("global.json rollForward must be latestPatch")
    if global_json.get("test", {}).get("runner") != "Microsoft.Testing.Platform":
        fail("global.json must select Microsoft.Testing.Platform for .NET 10 dotnet test")

    for xml_path in ["Directory.Build.props", "Directory.Packages.props", "Converty.slnx"]:
        try:
            ET.parse(ROOT / xml_path)
        except ET.ParseError as exc:
            fail(f"invalid XML in {xml_path}: {exc}")

    props = (ROOT / "Directory.Build.props").read_text(encoding="utf-8")
    for token in [
        "<TargetFramework>net10.0</TargetFramework>",
        "<Nullable>enable</Nullable>",
        "<TreatWarningsAsErrors>true</TreatWarningsAsErrors>",
        "<LangVersion>14.0</LangVersion>",
        "<NuGetAudit>true</NuGetAudit>",
        "<NuGetAuditMode>all</NuGetAuditMode>",
        "<NuGetAuditLevel>low</NuGetAuditLevel>",
    ]:
        if token not in props:
            fail(f"Directory.Build.props missing policy token: {token}")

    packages = (ROOT / "Directory.Packages.props").read_text(encoding="utf-8")
    if 'PackageVersion Include="xunit.v3.mtp-v2" Version="4.0.0"' not in packages:
        fail("Directory.Packages.props must pin xunit.v3.mtp-v2 4.0.0")

    release_policy = json.loads((ROOT / "machine-readable/release_policy.json").read_text(encoding="utf-8"))
    if release_policy.get("workspaceVersion") != version:
        fail("release policy workspaceVersion must match VERSION")
    if release_policy.get("hashAlgorithm") != "SHA-256":
        fail("release policy must require SHA-256")
    if release_policy.get("signing", {}).get("privateKeysInWorkspace") is not False:
        fail("release policy must forbid private keys in the workspace")
    ci_policy = release_policy.get("ciProvenance", {})
    if ci_policy.get("externalActionsPinnedToFullSha") is not True:
        fail("release policy must require immutable GitHub Action pins")
    if ci_policy.get("checkoutPersistCredentials") is not False:
        fail("release policy must forbid persisted checkout credentials")
    if ci_policy.get("workflowPermissions") != {"contents": "read"}:
        fail("release policy must keep CI workflow permissions at contents: read")
    if ci_policy.get("jobTimeoutMinutes") != {"managed": 30, "supply-chain-static": 15}:
        fail("release policy must encode the reviewed CI job timeout ceilings")
    audit = release_policy.get("dependencyAudit", {})
    if audit.get("enabled") is not True or audit.get("mode") != "all" or audit.get("level") != "low":
        fail("release policy must require NuGet audit all/low")

    nuget_config = ET.parse(ROOT / "NuGet.Config").getroot()
    audit_sources = nuget_config.find("auditSources")
    if audit_sources is None or not any(
        node.attrib.get("value") == "https://data.nuget.org/v3/index.json" for node in audit_sources.findall("add")
    ):
        fail("NuGet.Config must use the vulnerability-only nuget.org audit source")

    ci_pin_check = __import__("subprocess").run(
        [sys.executable, "scripts/verify_ci_actions.py"], cwd=ROOT, text=True, capture_output=True, check=False
    )
    if ci_pin_check.returncode != 0:
        fail("CI action pin verification failed: " + (ci_pin_check.stderr or ci_pin_check.stdout).strip())

    source_sbom = json.loads((ROOT / "machine-readable/source_sbom.spdx.json").read_text(encoding="utf-8"))
    if source_sbom.get("spdxVersion") != "SPDX-2.3":
        fail("source SBOM must be SPDX-2.3")
    if source_sbom.get("name") != f"Converty-source-{version}":
        fail("source SBOM version/name must match VERSION")

    solution = (ROOT / "Converty.slnx").read_text(encoding="utf-8")
    for project in [
        "src/Converty.Serialization/Converty.Serialization.csproj",
        "tests/Converty.Serialization.Tests/Converty.Serialization.Tests.csproj",
    ]:
        if project not in solution:
            fail(f"solution missing {project}")

    serialization_project = (ROOT / "src/Converty.Serialization/Converty.Serialization.csproj").read_text(encoding="utf-8")
    if "Converty.Contracts" not in serialization_project or "PackageReference" in serialization_project:
        fail("Serialization must reference Contracts only and introduce no package dependency")
    if "Converty.Core" in serialization_project:
        fail("Serialization must not reference Core")

    for project in [
        ROOT / "src/Converty.Contracts/Converty.Contracts.csproj",
        ROOT / "src/Converty.Core/Converty.Core.csproj",
    ]:
        text = project.read_text(encoding="utf-8")
        if "Converty.Serialization" in text:
            fail(f"{project.relative_to(ROOT)} must not reference Serialization")

    for rel, tokens in REQUIRED_SOURCE_TOKENS.items():
        path = ROOT / rel
        if not path.is_file():
            fail(f"required source file missing: {rel}")
        text = path.read_text(encoding="utf-8")
        for token in tokens:
            if token not in text:
                fail(f"{rel} missing expected token {token!r}")

    for project_dir in [
        ROOT / "src/Converty.Contracts",
        ROOT / "src/Converty.Core",
        ROOT / "src/Converty.Serialization",
    ]:
        for path in project_dir.rglob("*.cs"):
            text = path.read_text(encoding="utf-8", errors="replace")
            for token in FORBIDDEN_ENGINE_INDEPENDENT_TOKENS:
                if token.lower() in text.lower():
                    fail(f"forbidden execution/network token {token!r} found in {path.relative_to(ROOT)}")

    all_cs = "\n".join(p.read_text(encoding="utf-8", errors="replace") for p in (ROOT / "src").rglob("*.cs"))
    if re.search(r'(?:command|arguments?)\s*=\s*"[^"]*(?:ffmpeg|cmd|powershell)', all_cs, re.I):
        fail("production source appears to embed executable command strings")

    print("PASS: repository static verification succeeded")
    print(f"PASS: version={version}")
    print("PASS: SDK pin=10.0.400/latestPatch")
    print("PASS: XML policy files parse")
    print("PASS: Contracts/Core/Serialization contain no process/network/FFmpeg/native-loading tokens")
    print("PASS: serialization dependency direction is Contracts -> Serialization only")
    print("PASS: source SBOM/release/CI provenance policy authority is present and version-aligned")
    return 0


if __name__ == "__main__":
    sys.exit(main())
