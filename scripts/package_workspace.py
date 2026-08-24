#!/usr/bin/env python3
from __future__ import annotations

import argparse
import hashlib
import json
import zipfile
from pathlib import Path

from workspace_file_policy import is_workspace_file

ROOT = Path(__file__).resolve().parents[1]


def file_sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as handle:
        for chunk in iter(lambda: handle.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest()


def main() -> None:
    parser = argparse.ArgumentParser(description="Create a deterministic full-workspace Converty ZIP.")
    parser.add_argument("--output-dir", type=Path, default=ROOT.parent)
    args = parser.parse_args()

    version = (ROOT / "VERSION").read_text(encoding="utf-8").strip()
    archive = args.output_dir / f"Converty_{version}_full_workspace.zip"
    if archive.exists():
        archive.unlink()

    files = sorted(
        (path for path in ROOT.rglob("*") if is_workspace_file(path, ROOT)),
        key=lambda path: path.as_posix().lower(),
    )
    root_name = f"Converty_{version}"
    with zipfile.ZipFile(archive, "w", compression=zipfile.ZIP_DEFLATED, compresslevel=9) as zf:
        for path in files:
            arcname = f"{root_name}/{path.relative_to(ROOT).as_posix()}"
            info = zipfile.ZipInfo(arcname)
            info.date_time = (2026, 8, 24, 0, 0, 0)
            info.compress_type = zipfile.ZIP_DEFLATED
            info.external_attr = 0o100644 << 16
            zf.writestr(info, path.read_bytes(), compress_type=zipfile.ZIP_DEFLATED, compresslevel=9)

    result = {
        "archive": str(archive),
        "sha256": file_sha256(archive),
        "bytes": archive.stat().st_size,
        "files": len(files),
    }
    print(json.dumps(result, indent=2))


if __name__ == "__main__":
    main()
