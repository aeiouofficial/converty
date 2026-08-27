from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]


def test_bridge_product_worker_uses_strict_isolation_without_compatibility_fallback() -> None:
    source = (ROOT / "src/Converty.Bridge/Workers/EngineWorkerClient.cs").read_text(
        encoding="utf-8"
    )

    assert "WorkerIsolationLevel.Strict" in source
    assert "WorkerIsolationLevel.Compatibility" not in source
