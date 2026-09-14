import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]


def test_task8_video_acceptance_fixtures_are_explicitly_qualified_sdr():
    text = (ROOT / "build/video-input-acceptance-smoke.ps1").read_text(encoding="utf-8")
    assert re.search(
        r"\$fixture\.EncoderArgs\s*\+\s*@\('-color_trc',\s*'bt709',\s*'-y'",
        text,
    )


def test_task8_video_mixed_batch_fixtures_are_explicitly_qualified_sdr():
    text = (ROOT / "build/video-batch-isolation-smoke.ps1").read_text(encoding="utf-8")
    assert re.search(
        r"\$EncoderArgs\s*\+\s*@\('-color_trc',\s*'bt709',\s*'-y'",
        text,
    )
