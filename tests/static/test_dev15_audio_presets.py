from __future__ import annotations

from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]


def text(path: str) -> str:
    return (ROOT / path).read_text(encoding="utf-8")


def test_dev15_native_explorer_exposes_the_fixed_audio_matrix() -> None:
    shell = text("native/Converty.ShellExtension/ConvertyShellExtension.cpp")

    assert "constexpr std::array<PresetDefinition, 12> kPresets" in shell
    expected = {
        'audio.m4a.aac': 'Convert to M4A (AAC)',
        'audio.opus': 'Convert to Opus',
        'audio.ogg.vorbis': 'Convert to Ogg Vorbis',
    }
    for preset_id, title in expected.items():
        assert f'L"{preset_id}"' in shell
        assert f'L"{title}"' in shell


def test_dev15_product_smoke_qualifies_every_new_audio_target() -> None:
    smoke = text("build/product-conversion-smoke.ps1")

    for preset_id, extension, codec in (
        ("audio.mp3", ".mp3", "mp3"),
        ("audio.m4a.aac", ".m4a", "aac"),
        ("audio.opus", ".opus", "opus"),
        ("audio.ogg.vorbis", ".ogg", "vorbis"),
    ):
        assert preset_id in smoke
        assert extension in smoke
        assert codec in smoke

    assert "foreach ($case in $cases)" in smoke
    assert "Existing destination preserved" in smoke
    assert "Source preserved" in smoke


def test_dev15_keeps_audio_product_semantics_in_core_and_engine_tokens_in_provider() -> None:
    registry = text("src/Converty.Core/Presets/ProductPresetRegistry.cs")
    compiler = text("providers/Converty.Provider.FFmpeg/FfmpegPresetCompiler.cs")
    bridge_program = text("src/Converty.Bridge/Program.cs")
    bridge_parser = text("src/Converty.Bridge/Shell/ShellConversionRequestParser.cs")

    for preset_id in ("audio.m4a.aac", "audio.opus", "audio.ogg.vorbis"):
        assert f'PresetId.Parse("{preset_id}")' in registry
        assert f'"{preset_id}"' in compiler

    for token in ("libopus", "libvorbis", '"aac"', '"256k"', '"192k"'):
        assert token in compiler
        assert token not in registry

    assert "FfmpegArgumentsAfterInput" not in registry
    assert "BuildFfmpegArguments" not in registry
    assert "ffmpeg" not in bridge_program.lower()
    assert 'PresetSwitch = "--preset"' in bridge_parser
    assert "ProductPresetRegistry.Default.GetRequired(presetId)" in bridge_parser
