from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]


def read(path: str) -> str:
    return (ROOT / path).read_text(encoding="utf-8")


def test_dev7_journal_is_bounded_strict_and_atomically_published() -> None:
    source = read("src/Converty.Host/Jobs/HostJobJournal.cs")
    assert "MaximumEntries = 4096" in source
    assert "MaximumJournalBytes = 8L * 1024 * 1024" in source
    assert '"schemaVersion"' in source and '"jobs"' in source
    assert "HashSet<string>" in source
    assert "FileOptions.WriteThrough" in source
    assert "Flush(flushToDisk: true)" in source
    assert 'File.Move(_temporaryPath, _path, overwrite: true)' in source
    assert 'Interrupted by Host restart.' in source


def test_queue_persists_before_publishing_mutations() -> None:
    source = read("src/Converty.Host/Jobs/HostJobQueue.cs")
    persist_index = source.index("TryPersistWith(status")
    add_index = source.index("_jobs.Add(jobId, status)")
    assert persist_index < add_index
    assert "JobAdmissionRejection.PersistenceFailure" in source
    assert "LoadForRecovery()" in source


def test_host_runtime_restores_before_sessions_and_owns_single_instance() -> None:
    source = read("src/Converty.Host/Runtime/HostRuntime.cs")
    assert "HostSingleInstanceLease.TryAcquire" in source
    queue_index = source.index("HostJobQueue queue = _queueFactory()")
    session_index = source.index("_sessionFactory(queue)")
    assert queue_index < session_index
    assert "while (true)" in source
    assert "OperationCanceledException" in source
    assert "HostPipeServer" in source
    assert "WindowsConnectedPeerIdentityReader" in source


def test_host_is_no_console_executable_with_local_state_journal() -> None:
    project = read("src/Converty.Host/Converty.Host.csproj")
    program = read("src/Converty.Host/Program.cs")
    assert "<OutputType>WinExe</OutputType>" in project
    assert "Environment.SpecialFolder.LocalApplicationData" in program
    assert 'Path.Combine(localAppData, "Converty", "state")' in program
    assert '"jobs-v1.json"' in program
    assert "HostRuntime.CreateForCurrentUser" in program


def test_host_and_bridge_still_do_not_execute_engines_or_parse_media() -> None:
    forbidden = (
        "System.Diagnostics.Process",
        "Process.Start(",
        "ProcessStartInfo",
        "ffmpeg",
        "ffprobe",
        "HttpClient",
        "WebRequest",
    )
    active_files = list((ROOT / "src/Converty.Host").rglob("*.cs")) + list(
        (ROOT / "src/Converty.Bridge").rglob("*.cs")
    )
    combined = "\n".join(path.read_text(encoding="utf-8") for path in active_files)
    for token in forbidden:
        assert token not in combined
