from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
WORKERS = ROOT / "src/Converty.Security/Workers"


def _worker_sources() -> str:
    return "\n".join(
        path.read_text(encoding="utf-8")
        for path in sorted(WORKERS.glob("*.cs"))
    )


def test_worker_launch_policy_requires_explicit_isolation_and_resource_limits() -> None:
    assert (WORKERS / "WorkerIsolationLevel.cs").is_file()
    assert (WORKERS / "WorkerResourceLimits.cs").is_file()

    isolation = (WORKERS / "WorkerIsolationLevel.cs").read_text(encoding="utf-8")
    assert "Strict" in isolation
    assert "Compatibility" in isolation

    request = (WORKERS / "WorkerProcessLaunchRequest.cs").read_text(encoding="utf-8")
    assert "WorkerIsolationLevel IsolationLevel" in request
    assert "WorkerResourceLimits ResourceLimits" in request


def test_windows_worker_launcher_uses_suspended_kill_on_close_job_object_containment() -> None:
    sources = _worker_sources()
    required = [
        "CREATE_SUSPENDED",
        "CreateJobObject",
        "AssignProcessToJobObject",
        "JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE",
        "JOB_OBJECT_LIMIT_ACTIVE_PROCESS",
        "JOB_OBJECT_LIMIT_PROCESS_MEMORY",
        "JOB_OBJECT_LIMIT_JOB_MEMORY",
        "ResumeThread",
    ]
    missing = [token for token in required if token not in sources]
    assert not missing, missing


def test_windows_worker_launcher_applies_finite_cpu_rate_control() -> None:
    sources = _worker_sources()
    required = [
        "JOBOBJECT_CPU_RATE_CONTROL_INFORMATION",
        "JOB_OBJECT_CPU_RATE_CONTROL_ENABLE",
        "JOB_OBJECT_CPU_RATE_CONTROL_HARD_CAP",
    ]
    missing = [token for token in required if token not in sources]
    assert not missing, missing


def test_strict_policy_is_not_silently_rewritten_to_compatibility() -> None:
    launcher = (WORKERS / "WindowsWorkerProcessLauncher.cs").read_text(encoding="utf-8")
    forbidden = [
        "request with { IsolationLevel = WorkerIsolationLevel.Compatibility }",
        "request with {IsolationLevel = WorkerIsolationLevel.Compatibility}",
        "IsolationLevel = WorkerIsolationLevel.Compatibility",
    ]
    assert not [token for token in forbidden if token in launcher]
