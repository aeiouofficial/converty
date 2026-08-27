from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
WORKERS = ROOT / "src/Converty.Security/Workers"


def _worker_sources() -> str:
    return "\n".join(
        path.read_text(encoding="utf-8")
        for path in sorted(WORKERS.glob("*.cs"))
    )


def test_strict_request_carries_one_explicit_writable_filesystem_scope() -> None:
    scope_path = WORKERS / "WorkerFileSystemScope.cs"
    assert scope_path.is_file()
    request = (WORKERS / "WorkerProcessLaunchRequest.cs").read_text(encoding="utf-8")
    assert "WorkerFileSystemScope FileSystemScope" in request


def test_strict_launcher_uses_zero_capability_appcontainer_process_attribute() -> None:
    sources = _worker_sources()
    required = [
        "CreateAppContainerProfile",
        "DeriveAppContainerSidFromAppContainerName",
        "PROC_THREAD_ATTRIBUTE_SECURITY_CAPABILITIES",
        "SECURITY_CAPABILITIES",
        "CapabilityCount = 0",
    ]
    missing = [token for token in required if token not in sources]
    assert not missing, missing

    forbidden_network_capabilities = ["internetClient", "privateNetworkClientServer"]
    present = [token for token in forbidden_network_capabilities if token in sources]
    assert not present, present


def test_strict_launcher_grants_only_explicit_app_read_execute_and_staging_write_scope() -> None:
    sources = _worker_sources()
    required = [
        "GetNamedSecurityInfoW",
        "SetEntriesInAclW",
        "SetNamedSecurityInfoW",
        "FILE_GENERIC_READ",
        "FILE_GENERIC_EXECUTE",
        "FILE_GENERIC_WRITE",
    ]
    missing = [token for token in required if token not in sources]
    assert not missing, missing


def test_strict_path_has_no_compatibility_retry() -> None:
    launcher = (WORKERS / "WindowsWorkerProcessLauncher.cs").read_text(encoding="utf-8")
    assert "WorkerIsolationLevel.Strict" in launcher
    forbidden_rewrites = [
        "request with { IsolationLevel = WorkerIsolationLevel.Compatibility }",
        "request with {IsolationLevel = WorkerIsolationLevel.Compatibility}",
        "IsolationLevel = WorkerIsolationLevel.Compatibility",
    ]
    assert not [token for token in forbidden_rewrites if token in launcher]
    assert "retry compatibility" not in launcher.lower()
