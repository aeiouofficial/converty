from __future__ import annotations

from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]


def text(path: str) -> str:
    return (ROOT / path).read_text(encoding="utf-8")


def test_dev13_uses_existing_pipe_and_separate_bridge_control_interface() -> None:
    bridge = text("src/Converty.Bridge/Ipc/BridgeClient.cs")
    request_interface = text("src/Converty.Bridge/Ipc/IBridgeRequestClient.cs")
    control_interface = text("src/Converty.Bridge/Ipc/IBridgeJobControlClient.cs")
    host_server = text("src/Converty.Host/Ipc/HostPipeServer.cs")

    assert "IBridgeRequestClient, IBridgeJobControlClient" in bridge
    assert "GetStatusAsync" in control_interface
    assert "CancelAsync" in control_interface
    assert "SubmitAsync" in request_interface
    assert "GetStatusAsync" not in request_interface
    assert "CancelAsync" not in request_interface

    control_pipe_files = [
        path
        for path in (ROOT / "src/Converty.Host").rglob("*.cs")
        if "control" in path.name.lower() and "pipe" in path.name.lower()
    ]
    assert control_pipe_files == []
    assert "NamedPipeServerStreamAcl.Create" in host_server


def test_dev13_authenticates_host_before_first_control_or_submission_frame() -> None:
    bridge = text("src/Converty.Bridge/Ipc/BridgeClient.cs")
    verifier = "_serverIdentityVerifier.VerifyConnectedServer(pipe);"
    first_write = "BoundedProtocolFrameIo.WriteAndFlushAsync(pipe, payload"

    assert verifier in bridge
    assert first_write in bridge
    assert bridge.index(verifier) < bridge.index(first_write)
    assert "ExchangeAsync(payload, cancellationToken)" in bridge


def test_dev13_host_authorizes_peer_before_reading_application_semantics() -> None:
    server = text("src/Converty.Host/Ipc/HostPipeServer.cs")
    handler = text("src/Converty.Host/Ipc/HostRequestHandler.cs")

    assert server.index("_peerValidator.IsExpectedUser") < server.index("BoundedProtocolFrameIo.ReadAsync")
    assert "PeerAuthorization.ExpectedUser" in server
    assert "authorization != PeerAuthorization.ExpectedUser" in handler
    assert handler.index("authorization != PeerAuthorization.ExpectedUser") < handler.index("StrictUtf8.GetString")


def test_dev13_control_contracts_use_existing_status_snapshot_and_strict_serialization() -> None:
    request = text("src/Converty.Contracts/Jobs/JobControlRequest.cs")
    response = text("src/Converty.Contracts/Jobs/JobControlResponse.cs")
    serialization = text("src/Converty.Serialization/ContractJson.cs")
    enums = text("src/Converty.Serialization/V1/WireEnumText.cs")

    assert "JobControlOperation" in request
    assert "Guid jobId" in request
    assert "JobStatusSnapshot" in response
    assert "JobControlFailureReason" in response
    assert "DeserializeJobControlRequest" in serialization
    assert "DeserializeJobControlResponse" in serialization
    assert "Guid.TryParseExact(value, \"D\"" in serialization
    for token in ("status", "cancel", "jobNotFound", "notCancellable", "persistenceFailure"):
        assert f'"{token}"' in enums


def test_dev13_cancellation_remains_queued_only_and_transactional() -> None:
    queue = text("src/Converty.Host/Jobs/HostJobQueue.cs")
    handler = text("src/Converty.Host/Ipc/HostRequestHandler.cs")

    assert "current.State != ConversionJobState.Queued" in queue
    assert "ConversionJobState.Cancelled" in queue
    assert "Cancelled before execution." in queue
    assert "JobControlFailureReason.PersistenceFailure" in handler
    assert "JobControlFailureReason.NotCancellable" in handler
    assert "JobControlFailureReason.JobNotFound" in handler


def test_dev13_host_and_bridge_remain_media_neutral() -> None:
    media_forbidden = ("ffmpeg", "ffprobe")
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
