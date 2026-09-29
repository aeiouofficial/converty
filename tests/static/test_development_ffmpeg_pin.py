from __future__ import annotations

import json
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
PIN = ROOT / "eng" / "ffmpeg-development.json"
PREPARE = ROOT / "build" / "prepare-dev-ffmpeg.ps1"


def test_development_ffmpeg_pin_uses_tagged_hash_locked_github_release_asset() -> None:
    data = json.loads(PIN.read_text(encoding="utf-8"))
    assert data["purpose"] == "development-qualification-only"
    assert data["vendor"] == "BtbN FFmpeg-Builds"
    assert re.fullmatch(r"autobuild-\d{4}-\d{2}-\d{2}-\d{2}-\d{2}", data["releaseTag"])
    assert isinstance(data["releaseAssetId"], int) and data["releaseAssetId"] > 0
    assert re.fullmatch(r"[0-9a-f]{64}", data["archiveSha256"])
    assert re.fullmatch(r"[0-9a-f]{10}", data["sourceCommit"])
    url = data["archiveUrl"]
    assert url.startswith(
        "https://github.com/BtbN/FFmpeg-Builds/releases/download/"
        + data["releaseTag"]
        + "/"
    )
    assert "/latest/" not in url
    assert url.endswith(data["archiveAssetName"])
    assert data["assetApiUrl"] == (
        "https://api.github.com/repos/BtbN/FFmpeg-Builds/releases/assets/"
        + str(data["releaseAssetId"])
    )


def test_prepare_script_rejects_unversioned_or_incomplete_dev_pin() -> None:
    text = PREPARE.read_text(encoding="utf-8")
    for token in (
        "releaseTag",
        "releaseAssetId",
        "archiveAssetName",
        "assetApiUrl",
        "sourceCommit",
        "/latest/",
    ):
        assert token in text
