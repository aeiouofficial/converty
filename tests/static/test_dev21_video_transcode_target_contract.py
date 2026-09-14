from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
QUALIFIED_SDR_FILTER = "setparams=colorspace=bt709:color_primaries=bt709:color_trc=bt709"


def test_task8_video_transcode_provider_owns_fixed_color_contract():
    text = (ROOT / "providers/Converty.Provider.FFmpeg/FfmpegPresetCompiler.cs").read_text(encoding="utf-8")
    mp4 = text.split('("video.mp4.h264", ConversionMode.Transcode) =>', 1)[1].split('("video.webm.vp9", ConversionMode.Remux)', 1)[0]
    webm = text.split('("video.webm.vp9", ConversionMode.Transcode) =>', 1)[1].split('("extract.audio.mp3", ConversionMode.Remux)', 1)[0]
    assert QUALIFIED_SDR_FILTER in mp4
    assert QUALIFIED_SDR_FILTER in webm
    assert '"-vf"' in mp4
    assert '"-vf"' in webm
