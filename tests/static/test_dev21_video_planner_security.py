from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
CORE_PRESETS = ROOT / "src" / "Converty.Core" / "Presets"
COMPILER = ROOT / "providers" / "Converty.Provider.FFmpeg" / "FfmpegPresetCompiler.cs"
LAUNCHER = ROOT / "providers" / "Converty.Provider.FFmpeg" / "FfmpegProcessLauncher.cs"
ENGINE_WORKER = ROOT / "src" / "Converty.EngineWorker" / "Program.cs"
ENGINE_WORKER_CLIENT = ROOT / "src" / "Converty.Bridge" / "Workers" / "EngineWorkerClient.cs"
MANAGED_COPY = ROOT / "src" / "Converty.Core" / "Execution" / "ManagedByteCopy.cs"


def _text(path: Path) -> str:
    return path.read_text(encoding="utf-8")


def test_core_presets_are_engine_neutral() -> None:
    core = "\n".join(_text(path) for path in sorted(CORE_PRESETS.glob("*.cs")))
    forbidden = (
        "FfmpegArgumentsAfterInput",
        "BuildFfmpegArguments",
        "_ffmpegArgumentsAfterInput",
        "libx264",
        "libvpx-vp9",
        "libmp3lame",
        "libopus",
        "libvorbis",
        "pcm_s16le",
        '"-map"',
        '"-c:v"',
        '"-c:a"',
        '"-protocol_whitelist"',
    )
    for token in forbidden:
        assert token not in core, f"Core preset layer still owns FFmpeg token/policy: {token}"


def test_provider_compiler_is_closed_and_owns_ffmpeg_policy() -> None:
    assert COMPILER.is_file(), "closed provider compiler is missing"
    compiler = _text(COMPILER)
    for token in (
        "FfmpegPresetCompiler",
        "PresetId",
        "ConversionMode",
        "video.mp4.h264",
        "video.webm.vp9",
        "extract.audio.mp3",
        "audio.mp3",
        "audio.flac",
        "audio.m4a.aac",
        "audio.opus",
        "audio.ogg.vorbis",
        "audio.wav",
        "image.png",
        "image.jpeg",
        "image.webp",
        '"-protocol_whitelist"',
        '"file"',
        '"-map_metadata"',
        '"-map_chapters"',
        '"-1"',
        '"yuv420p"',
        '"48000"',
        '"44100"',
    ):
        assert token in compiler, f"provider compiler missing fixed policy token: {token}"

    lowered = compiler.lower()
    for forbidden in ("-hwaccel", "nvenc", "cuda", "qsv", "d3d11va", "videotoolbox", "amf"):
        assert forbidden not in lowered, f"hardware acceleration token is forbidden: {forbidden}"


def test_launcher_consumes_explicit_mode_without_raw_argument_surface() -> None:
    launcher = _text(LAUNCHER)
    assert "FfmpegPresetCompiler.Compile" in launcher
    assert "ConversionMode mode" in launcher
    assert "ResolveCurrentProductMode" not in launcher
    assert "BuildFfmpegArguments" not in launcher
    assert "FfmpegArgumentsAfterInput" not in launcher
    assert "ArgumentList.Add" in launcher
    assert "file,pipe" not in launcher


def test_engine_worker_surface_requires_mode_and_copy_bypasses_ffmpeg() -> None:
    worker = _text(ENGINE_WORKER)
    client = _text(ENGINE_WORKER_CLIENT)

    assert "args.Length != 8" in worker
    for token in ('"--preset"', '"--mode"', '"--input"', '"--output"'):
        assert token in worker
        assert token in client
    assert "ConversionModeArgument.Parse" in worker
    assert "ManagedByteCopy.CopyAndVerifyAsync" in worker
    assert "ConversionModeArgument.Format" in client

    copy_dispatch = worker.index("request.Mode == ConversionMode.Copy")
    ffmpeg_resolution = worker.index("TrustedFfmpegPath.ResolveFromApplicationBaseDirectory")
    assert copy_dispatch < ffmpeg_resolution, "Copy must dispatch before any FFmpeg resolution"


def test_managed_copy_uses_create_new_and_sha256_equality_verification() -> None:
    assert MANAGED_COPY.is_file(), "managed byte-copy implementation is missing"
    copy = _text(MANAGED_COPY)
    for token in (
        "FileMode.CreateNew",
        "CopyToAsync",
        "SHA256",
        "CryptographicOperations.FixedTimeEquals",
    ):
        assert token in copy, f"managed Copy is missing required byte/hash invariant: {token}"
