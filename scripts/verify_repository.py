#!/usr/bin/env python3
from __future__ import annotations

import json
import re
import subprocess
import sys
import xml.etree.ElementTree as ET
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
CURRENT = (ROOT / "VERSION").read_text(encoding="utf-8").strip()

REQUIRED_FILES = [
    "VERSION", "global.json", "Directory.Build.props", "Directory.Packages.props",
    "NuGet.Config", "Converty.slnx",
    "src/Converty.Contracts/Converty.Contracts.csproj",
    "src/Converty.Core/Converty.Core.csproj",
    "src/Converty.FakeProviders/Converty.FakeProviders.csproj",
    "src/Converty.Serialization/Converty.Serialization.csproj",
    "src/Converty.Ipc/Converty.Ipc.csproj",
    "src/Converty.Security/Converty.Security.csproj",
    "src/Converty.Host/Converty.Host.csproj",
    "src/Converty.Bridge/Converty.Bridge.csproj",
    "src/Converty.Serialization/ContractJson.cs",
    "src/Converty.Host/Jobs/HostJobJournal.cs",
    "src/Converty.Host/Runtime/HostRuntime.cs",
    "src/Converty.Host/Program.cs",
    "machine-readable/release_policy.json",
    "machine-readable/ci_action_pins.json",
    "machine-readable/handover_state.json",
    "machine-readable/build_evidence.json",
    "machine-readable/source_sbom.spdx.json",
    "scripts/generate_sbom.py",
    "scripts/verify_release_inputs.py",
    "scripts/verify_ci_actions.py",
    "scripts/verify_dependency_audit.py",
    "scripts/verify_contract_vectors.py",
    "tests/vectors/v1/manifest.json",
    "tests/fuzz/ipc/v1/corpus.json",
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
    "src/Converty.Host/Ipc/HostPipeServer.cs": [
        "NamedPipeServerStreamAcl.Create",
        "_peerValidator.IsExpectedUser",
        "BoundedProtocolFrameIo.ReadAsync",
        "BoundedProtocolFrameIo.WriteAndFlushAsync",
    ],
    "src/Converty.Bridge/Ipc/BridgeClient.cs": [
        "MaximumConnectTimeout",
        "BoundedProtocolFrameIo.WriteAndFlushAsync",
        "BoundedProtocolFrameIo.ReadAsync",
    ],
    "src/Converty.Ipc/Protocol/BoundedProtocolFrameIo.cs": [
        "MaximumTimeout = TimeSpan.FromSeconds(30)",
        "ProtocolFrameCodec.WriteAsync",
        "ProtocolFrameCodec.ReadAsync",
    ],
    "src/Converty.Host/Runtime/HostSingleInstanceLease.cs": ["Local\\Converty.Host.", "new Mutex(initiallyOwned: true"],
    "src/Converty.Host/Jobs/HostJobJournal.cs": ["MaximumJournalBytes", "FileOptions.WriteThrough", "Flush(flushToDisk: true)"],
    "src/Converty.Host/Runtime/HostRuntime.cs": ["HostSingleInstanceLease.TryAcquire", "HostJobQueue queue = _queueFactory()"],
    "src/Converty.Host/Program.cs": ["Environment.SpecialFolder.LocalApplicationData", "HostRuntime.CreateForCurrentUser"],
    "src/Converty.Core/Execution/FfmpegProcessLauncher.cs": [
        "UseShellExecute = false",
        "ArgumentList.Add",
        "CreateNoWindow = true",
        "WaitForExitAsync",
    ],
    "src/Converty.Core/Execution/TrustedFfmpegPath.cs": [
        'ExecutableFileName = "ffmpeg.exe"',
        'Path.Combine(root, "tools", "ffmpeg")',
    ],
}

FORBIDDEN_ENGINE_INDEPENDENT_TOKENS = [
    "Process.Start(", "ProcessStartInfo", "ffmpeg", "ffprobe", "cmd.exe",
    "powershell.exe", "HttpClient", "Socket", "NamedPipe", "DllImport", "LibraryImport",
]

FORBIDDEN_B2_MEDIA_TOKENS = [
    "ffmpeg", "ffprobe", "cmd.exe", "powershell.exe", "HttpClient", "WebRequest",
]

FORBIDDEN_B2_PROCESS_TOKENS = ["Process.Start(", "ProcessStartInfo", "System.Diagnostics.Process"]

ALLOWED_CORE_EXECUTION_PROCESS_FILES = {
    "src/Converty.Core/Execution/FfmpegProcessLauncher.cs",
}

ALLOWED_CORE_FFMPEG_FILES = {
    "src/Converty.Core/Execution/ConversionBatchRunner.cs",
    "src/Converty.Core/Execution/FfmpegExecutionResult.cs",
    "src/Converty.Core/Execution/FfmpegProcessLauncher.cs",
    "src/Converty.Core/Execution/IFfmpegProcessLauncher.cs",
    "src/Converty.Core/Execution/TrustedFfmpegPath.cs",
    "src/Converty.Core/Presets/ProductPresetDefinition.cs",
    "src/Converty.Core/Presets/ProductPresetRegistry.cs",
}


def fail(message: str) -> None:
    print(f"FAIL: {message}")
    raise SystemExit(1)


def load_json(path: str) -> dict:
    return json.loads((ROOT / path).read_text(encoding="utf-8"))


def main() -> int:
    missing = [path for path in REQUIRED_FILES if not (ROOT / path).is_file()]
    if missing:
        fail("required files missing: " + ", ".join(missing))

    version = (ROOT / "VERSION").read_text(encoding="utf-8").strip()
    if version != CURRENT:
        fail(f"VERSION must be {CURRENT}, got {version!r}")

    global_json = load_json("global.json")
    sdk = global_json.get("sdk", {})
    if sdk.get("version") != "10.0.400" or sdk.get("rollForward") != "latestPatch":
        fail("global.json must pin .NET SDK 10.0.400/latestPatch")
    if global_json.get("test", {}).get("runner") != "Microsoft.Testing.Platform":
        fail("global.json must select Microsoft.Testing.Platform")

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

    authorities = {
        "release policy": load_json("machine-readable/release_policy.json").get("workspaceVersion"),
        "CI action pins": load_json("machine-readable/ci_action_pins.json").get("workspaceVersion"),
        "handover": load_json("machine-readable/handover_state.json").get("workspaceVersion"),
        "build evidence": load_json("machine-readable/build_evidence.json").get("workspaceVersion"),
        "toolchain": load_json("eng/toolchain.json").get("workspaceVersion"),
    }
    drift = [name for name, authority_version in authorities.items() if authority_version != version]
    if drift:
        fail("workspace version drift: " + ", ".join(drift))

    release_policy = load_json("machine-readable/release_policy.json")
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
        fail("release policy must keep permanent CI workflow permissions at contents: read")
    if ci_policy.get("jobTimeoutMinutes") != {"managed": 30, "supply-chain-static": 15}:
        fail("release policy must encode reviewed CI job timeout ceilings")

    audit = release_policy.get("dependencyAudit", {})
    if audit.get("enabled") is not True or audit.get("mode") != "all" or audit.get("level") != "low":
        fail("release policy must require NuGet audit all/low")

    nuget_config = ET.parse(ROOT / "NuGet.Config").getroot()
    audit_sources = nuget_config.find("auditSources")
    if audit_sources is None or not any(
        node.attrib.get("value") == "https://data.nuget.org/v3/index.json"
        for node in audit_sources.findall("add")
    ):
        fail("NuGet.Config must use the vulnerability-only nuget.org audit source")

    ci_pin_check = subprocess.run(
        [sys.executable, "scripts/verify_ci_actions.py"],
        cwd=ROOT, text=True, capture_output=True, check=False,
    )
    if ci_pin_check.returncode != 0:
        fail("CI action pin verification failed: " + (ci_pin_check.stderr or ci_pin_check.stdout).strip())

    projects = sorted(
        path for path in ROOT.rglob("*.csproj")
        if not any(part in {"bin", "obj", "artifacts"} for part in path.parts)
    )
    missing_locks = [project.relative_to(ROOT).as_posix() for project in projects if not (project.parent / "packages.lock.json").is_file()]
    if missing_locks:
        fail("managed lock files missing for: " + ", ".join(missing_locks))

    source_sbom = load_json("machine-readable/source_sbom.spdx.json")
    if source_sbom.get("spdxVersion") != "SPDX-2.3":
        fail("source SBOM must be SPDX-2.3")
    if source_sbom.get("name") != f"Converty-source-{version}":
        fail("source SBOM name/version must match VERSION")
    if any(package.get("versionInfo") != version for package in source_sbom.get("packages", [])):
        fail("source SBOM package versions must match VERSION")

    serialization_project = (ROOT / "src/Converty.Serialization/Converty.Serialization.csproj").read_text(encoding="utf-8")
    if "Converty.Contracts" not in serialization_project or "PackageReference" in serialization_project or "Converty.Core" in serialization_project:
        fail("Serialization must reference Contracts only and introduce no package/Core dependency")

    for rel, tokens in REQUIRED_SOURCE_TOKENS.items():
        path = ROOT / rel
        if not path.is_file():
            fail(f"required source file missing: {rel}")
        source = path.read_text(encoding="utf-8")
        for token in tokens:
            if token not in source:
                fail(f"{rel} missing expected token {token!r}")

    engine_independent_roots = [ROOT / "src/Converty.Contracts", ROOT / "src/Converty.Serialization"]
    for project_dir in engine_independent_roots:
        for path in project_dir.rglob("*.cs"):
            source = path.read_text(encoding="utf-8", errors="replace")
            for token in FORBIDDEN_ENGINE_INDEPENDENT_TOKENS:
                if token.lower() in source.lower():
                    fail(f"forbidden execution/network token {token!r} found in {path.relative_to(ROOT)}")

    core_root = ROOT / "src/Converty.Core"
    for path in core_root.rglob("*.cs"):
        rel = path.relative_to(ROOT).as_posix()
        source = path.read_text(encoding="utf-8", errors="replace")
        lower = source.lower()
        if rel not in ALLOWED_CORE_EXECUTION_PROCESS_FILES:
            for token in ("Process.Start(", "ProcessStartInfo", "System.Diagnostics.Process"):
                if token.lower() in lower:
                    fail(f"process execution token {token!r} found outside dedicated product launcher in {rel}")
        if "ffmpeg" in lower and rel not in ALLOWED_CORE_FFMPEG_FILES:
            fail(f"FFmpeg reference found outside approved product preset/execution boundary in {rel}")
        for token in ("cmd.exe", "powershell.exe", "HttpClient", "Socket", "DllImport", "LibraryImport"):
            if token.lower() in lower:
                fail(f"forbidden shell/network/native token {token!r} found in Core file {rel}")

    b2_roots = [ROOT / "src/Converty.Host", ROOT / "src/Converty.Bridge"]
    for project_dir in b2_roots:
        for path in project_dir.rglob("*.cs"):
            source = path.read_text(encoding="utf-8", errors="replace")
            for token in FORBIDDEN_B2_MEDIA_TOKENS:
                if token.lower() in source.lower():
                    fail(f"forbidden B2 media/network execution token {token!r} found in {path.relative_to(ROOT)}")

    for path in (ROOT / "src/Converty.Host").rglob("*.cs"):
        source = path.read_text(encoding="utf-8", errors="replace")
        for token in FORBIDDEN_B2_PROCESS_TOKENS:
            if token.lower() in source.lower():
                fail(f"forbidden Host process token {token!r} found in {path.relative_to(ROOT)}")

    bridge_root = ROOT / "src/Converty.Bridge"
    startup_root = bridge_root / "Startup"
    for path in bridge_root.rglob("*.cs"):
        if startup_root in path.parents:
            continue
        source = path.read_text(encoding="utf-8", errors="replace")
        for token in FORBIDDEN_B2_PROCESS_TOKENS:
            if token.lower() in source.lower():
                fail(f"forbidden Bridge process token {token!r} found outside Startup in {path.relative_to(ROOT)}")

    all_cs = "\n".join(path.read_text(encoding="utf-8", errors="replace") for path in (ROOT / "src").rglob("*.cs"))
    if re.search(r'(?:command|arguments?)\s*=\s*"[^"]*(?:ffmpeg|cmd|powershell)', all_cs, re.I):
        fail("production source appears to embed executable command strings")

    print("PASS: repository static verification succeeded")
    print(f"PASS: version={version}")
    print("PASS: SDK pin=10.0.400/latestPatch")
    print(f"PASS: {len(projects)}/{len(projects)} managed lock files present")
    print("PASS: source SBOM/release/CI/handover/toolchain authority is version-aligned")
    print("PASS: Host remains non-executing; product FFmpeg execution is confined to the dedicated Core launcher")
    return 0


if __name__ == "__main__":
    sys.exit(main())
