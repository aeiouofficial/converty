from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]


def test_task8_copy_and_remux_contracts_preserve_source_video_facts():
    text = (ROOT / "src/Converty.Core/Execution/TargetMediaContract.cs").read_text(encoding="utf-8")
    assert "sourceVideo.PixelFormat" in text
    assert "sourceVideo.BitDepth" in text
    assert "sourceVideo.ColorTransfer" in text
    assert "sourceVideo.HdrState" in text
    assert "mode == ConversionMode.Copy" in text
    assert "mode == ConversionMode.Remux" in text
