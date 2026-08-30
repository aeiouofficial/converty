from __future__ import annotations

import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]


def text(path: str) -> str:
    return (ROOT / path).read_text(encoding="utf-8")


def current_version() -> str:
    return text("VERSION").strip()


def next_version(version: str) -> str:
    prefix, dev = version.rsplit("-dev.", 1)
    return f"{prefix}-dev.{int(dev) + 1}"


def test_dev6_b2_projects_remain_present_in_later_tranches() -> None:
    version = current_version()
    assert int(version.rsplit(".", 1)[1]) >= 6
    for path in (
        "src/Converty.Ipc/Converty.Ipc.csproj",
        "src/Converty.Security/Converty.Security.csproj",
        "src/Converty.Host/Converty.Host.csproj",
        "src/Converty.Bridge/Converty.Bridge.csproj",
        "tests/Converty.Ipc.Tests/Converty.Ipc.Tests.csproj",
        "tests/Converty.Security.Tests/Converty.Security.Tests.csproj",
        "tests/Converty.Host.Tests/Converty.Host.Tests.csproj",
        "tests/Converty.Bridge.Tests/Converty.Bridge.Tests.csproj",
    ):
        assert (ROOT / path).is_file(), path


def test_ipc_framing_and_windows_pipe_security_are_bounded() -> None:
    limits = text("src/Converty.Ipc/Protocol/ProtocolLimits.cs")
    codec = text("src/Converty.Ipc/Protocol/ProtocolFrameCodec.cs")
    bounded_io = text("src/Converty.Ipc/Protocol/BoundedProtocolFrameIo.cs")
    pipe_security = text("src/Converty.Security/Ipc/CurrentUserPipeSecurity.cs")
    server = text("src/Converty.Host/Ipc/HostPipeServer.cs")
    bridge = text("src/Converty.Bridge/Ipc/BridgeClient.cs")

    assert "MaxPayloadBytes = 1_048_576" in limits
    for token in ("BadMagic", "UnsupportedVersion", "InvalidLength", "FrameTooLarge", "TruncatedFrame", "checked("):
        assert token in codec
    assert "MaximumTimeout = TimeSpan.FromSeconds(30)" in bounded_io
    assert "ProtocolFrameCodec.ReadAsync" in bounded_io
    assert "ProtocolFrameCodec.WriteAsync" in bounded_io
    assert "SetAccessRuleProtection(isProtected: true" in pipe_security
    assert "NamedPipeServerStreamAcl.Create" in server
    assert server.index("_peerValidator.IsExpectedUser") < server.index("BoundedProtocolFrameIo.ReadAsync")
    assert "BoundedProtocolFrameIo.WriteAndFlushAsync" in server
    assert "MaximumConnectTimeout = TimeSpan.FromSeconds(30)" in bridge
    assert "ConnectAsync(_connectTimeout, cancellationToken)" in bridge
    assert "BoundedProtocolFrameIo.WriteAndFlushAsync" in bridge
    assert "BoundedProtocolFrameIo.ReadAsync" in bridge


def test_host_single_instance_queue_and_fuzz_corpus_are_present() -> None:
    lease = text("src/Converty.Host/Runtime/HostSingleInstanceLease.cs")
    queue = text("src/Converty.Host/Jobs/HostJobQueue.cs")
    corpus = json.loads(text("tests/fuzz/ipc/v1/corpus.json"))
    ids = {case["id"] for case in corpus["cases"]}

    assert "Local\\Converty.Host." in lease
    assert "new Mutex(initiallyOwned: true" in lease
    assert "DuplicateRequest" in queue and "QueueFull" in queue and "ConversionJobState.Cancelled" in queue
    assert ids == {
        "bad-magic",
        "future-version",
        "negative-length",
        "oversized-length",
        "truncated-payload",
        "malformed-request-json",
        "unknown-request-member",
        "control-unknown-member",
        "control-duplicate-operation",
        "control-noncanonical-job-id",
        "control-future-schema",
        "control-hybrid-conversion-members",
    }
    assert "IpcFuzzCorpusTests" in text("tests/Converty.Host.Tests/Ipc/IpcFuzzCorpusTests.cs")


def test_b2_host_and_bridge_do_not_execute_media_or_unapproved_processes() -> None:
    media_forbidden = ("ffmpeg", "ffprobe", "cmd.exe", "powershell.exe")
    for root in (ROOT / "src/Converty.Host", ROOT / "src/Converty.Bridge"):
        for path in root.rglob("*.cs"):
            source = path.read_text(encoding="utf-8").lower()
            for token in media_forbidden:
                assert token not in source, f"{token} found in {path.relative_to(ROOT)}"

    process_forbidden = ("process.start", "system.diagnostics.process", "processstartinfo")
    for path in (ROOT / "src/Converty.Host").rglob("*.cs"):
        source = path.read_text(encoding="utf-8").lower()
        for token in process_forbidden:
            assert token not in source, f"{token} found in {path.relative_to(ROOT)}"

    bridge_root = ROOT / "src/Converty.Bridge"
    startup_root = bridge_root / "Startup"
    for path in bridge_root.rglob("*.cs"):
        if startup_root in path.parents:
            continue
        source = path.read_text(encoding="utf-8").lower()
        for token in process_forbidden:
            assert token not in source, f"{token} found outside approved startup boundary in {path.relative_to(ROOT)}"


def test_all_managed_projects_are_locked_and_handover_advances_one_tranche() -> None:
    projects = sorted(ROOT.glob("src/**/*.csproj")) + sorted(ROOT.glob("tests/**/*.csproj"))
    assert len(projects) >= 15
    missing = [str(project.relative_to(ROOT)) for project in projects if not (project.parent / "packages.lock.json").is_file()]
    assert missing == []

    current = current_version()
    next_target = next_version(current)
    handover_state = json.loads(text("machine-readable/handover_state.json"))
    handover_prompt = text("docs/HANDOVER_PROMPT.txt")
    assert handover_state["workspaceVersion"] == current
    assert handover_state["nextWorkspaceVersion"] == next_target
    assert next_target in handover_prompt
