from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]


def test_destination_volume_publisher_is_durable_and_no_overwrite() -> None:
    publisher = ROOT / "src/Converty.Core/Execution/DestinationOutputPublisher.cs"
    assert publisher.is_file()
    text = publisher.read_text(encoding="utf-8")
    assert "Path.GetDirectoryName(outputPath)" in text
    assert ".converty-" in text
    assert "Flush(flushToDisk: true)" in text
    assert "File.Move" in text
    assert "overwrite: false" in text


def test_staging_recovery_is_owned_and_age_bounded() -> None:
    text = (ROOT / "src/Converty.Core/Execution/ConversionStagingDirectory.cs").read_text(encoding="utf-8")
    assert ".converty-owned" in text
    assert "CleanupStaleOwnedJobs" in text
    assert "ReparsePoint" in text
    assert "StaleJobAge" in text


def test_batch_result_exposes_member_failures() -> None:
    text = (ROOT / "src/Converty.Core/Execution/ConversionBatchResult.cs").read_text(encoding="utf-8")
    assert "ConversionFileFailure" in text
    assert "Failures" in text
    assert "HasFailures" in text
