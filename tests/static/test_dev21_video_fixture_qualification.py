import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
QUALIFIED_SDR_FILTER = "setparams=colorspace=bt709:color_primaries=bt709:color_trc=bt709"


def test_task8_video_acceptance_fixtures_are_explicitly_qualified_sdr():
    text = (ROOT / "build/video-input-acceptance-smoke.ps1").read_text(encoding="utf-8")
    assert QUALIFIED_SDR_FILTER in text
    assert re.search(r"Name\s*=\s*'avi'.*?'mpeg2video'", text)
    assert re.search(r"Name\s*=\s*'wmv'.*?'mpeg2video'", text)


def test_task8_video_mixed_batch_fixtures_are_explicitly_qualified_sdr():
    text = (ROOT / "build/video-batch-isolation-smoke.ps1").read_text(encoding="utf-8")
    assert QUALIFIED_SDR_FILTER in text
