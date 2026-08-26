from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]


def test_reserved_worker_and_ffmpeg_provider_are_real_projects() -> None:
    assert (ROOT / "src/Converty.EngineWorker/Converty.EngineWorker.csproj").is_file()
    assert (ROOT / "providers/Converty.Provider.FFmpeg/Converty.Provider.FFmpeg.csproj").is_file()


def test_core_no_longer_contains_ffmpeg_process_implementation() -> None:
    execution = ROOT / "src/Converty.Core/Execution"
    forbidden = [
        execution / "FfmpegProcessLauncher.cs",
        execution / "IFfmpegProcessLauncher.cs",
        execution / "FfmpegExecutionResult.cs",
        execution / "TrustedFfmpegPath.cs",
    ]
    assert not [path.relative_to(ROOT).as_posix() for path in forbidden if path.exists()]


def test_bridge_no_longer_uses_dev9_direct_ffmpeg_factory() -> None:
    program = (ROOT / "src/Converty.Bridge/Program.cs").read_text(encoding="utf-8")
    assert "ConversionBatchRunner.CreateForApplicationBaseDirectory" not in program
