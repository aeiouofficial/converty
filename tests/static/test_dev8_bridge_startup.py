from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]


def read(path: str) -> str:
    return (ROOT / path).read_text(encoding="utf-8")


def test_trusted_host_path_is_fixed_and_rejects_reparse_points() -> None:
    source = read("src/Converty.Bridge/Startup/TrustedHostPath.cs")
    assert 'HostExecutableFileName = "Converty.Host.exe"' in source
    assert "Path.IsPathFullyQualified" in source
    assert "Directory.Exists" in source
    assert "File.Exists" in source
    assert "FileAttributes.ReparsePoint" in source
    assert "Path.Combine(fullDirectory, HostExecutableFileName)" in source


def test_installed_launcher_has_no_shell_or_argument_surface() -> None:
    source = read("src/Converty.Bridge/Startup/InstalledHostProcessLauncher.cs")
    interface = read("src/Converty.Bridge/Startup/IHostProcessLauncher.cs")
    assert "void StartHost();" in interface
    assert "FileName = trustedHostPath.ExecutablePath" in source
    assert "Arguments = string.Empty" in source
    assert "UseShellExecute = false" in source
    assert "CreateNoWindow = true" in source
    assert "ProcessWindowStyle.Hidden" in source
    assert "WorkingDirectory = trustedHostPath.InstallDirectory" in source
    assert "Process.Start(startInfo)" in source
    for forbidden in ("cmd.exe", "powershell.exe", "ffmpeg", "ffprobe", "ArgumentList.Add"):
        assert forbidden not in source


def test_process_start_is_confined_to_one_bridge_startup_file() -> None:
    bridge_root = ROOT / "src/Converty.Bridge"
    matches = []
    for path in bridge_root.rglob("*.cs"):
        if "Process.Start" in path.read_text(encoding="utf-8"):
            matches.append(path.relative_to(ROOT).as_posix())
    assert matches == ["src/Converty.Bridge/Startup/InstalledHostProcessLauncher.cs"]


def test_startup_coordinator_catches_only_connect_stage_unavailability() -> None:
    coordinator = read("src/Converty.Bridge/Startup/BridgeSubmissionCoordinator.cs")
    client = read("src/Converty.Bridge/Ipc/BridgeClient.cs")

    assert "MaximumStartupTimeout = TimeSpan.FromSeconds(30)" in coordinator
    assert "MaximumRetryDelay = TimeSpan.FromSeconds(1)" in coordinator
    assert coordinator.count("_launcher.StartHost();") == 1
    assert coordinator.count("catch (BridgeHostUnavailableException)") == 2
    assert "catch (InvalidDataException" not in coordinator
    assert "catch (Exception" not in coordinator
    assert "CancellationTokenSource.CreateLinkedTokenSource" in coordinator
    assert "startupCancellation.CancelAfter(_startupTimeout)" in coordinator
    assert "throw new TimeoutException" in coordinator

    connect_index = client.index("await pipe.ConnectAsync")
    write_index = client.index("ProtocolFrameCodec.WriteAsync")
    unavailable_index = client.index("BridgeHostUnavailableException")
    assert connect_index < unavailable_index < write_index
    assert "catch (TimeoutException error)" in client
    assert "catch (IOException error)" in client
    assert "catch (OperationCanceledException" not in client
