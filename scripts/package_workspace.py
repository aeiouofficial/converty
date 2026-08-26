#!/usr/bin/env python3
from __future__ import annotations

import argparse
import hashlib
import json
import subprocess
import zipfile
from pathlib import Path, PurePosixPath

from workspace_file_policy import (
    EXCLUDED_NAMES,
    EXCLUDED_PARTS,
    EXCLUDED_SUFFIXES,
    is_workspace_file,
)

ROOT = Path(__file__).resolve().parents[1]


def file_sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as handle:
        for chunk in iter(lambda: handle.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest()


def allowed_relative_path(relative: PurePosixPath) -> bool:
    if relative.name in EXCLUDED_NAMES:
        return False
    if any(part in EXCLUDED_PARTS for part in relative.parts):
        return False
    return relative.suffix.lower() not in EXCLUDED_SUFFIXES


def committed_entries(root: Path) -> list[tuple[PurePosixPath, bytes]] | None:
    inside = subprocess.run(
        ["git", "-C", str(root), "rev-parse", "--is-inside-work-tree"],
        check=False,
        capture_output=True,
        text=True,
        encoding="utf-8",
    )
    if inside.returncode != 0 or inside.stdout.strip() != "true":
        return None

    tree = subprocess.run(
        ["git", "-C", str(root), "ls-tree", "-r", "-z", "--full-tree", "HEAD"],
        check=True,
        capture_output=True,
    ).stdout
    entries: list[tuple[PurePosixPath, bytes]] = []
    for record in tree.split(b"\0"):
        if not record:
            continue
        metadata, raw_path = record.split(b"\t", 1)
        _mode, object_type, object_sha = metadata.split(b" ", 2)
        if object_type != b"blob":
            continue
        relative = PurePosixPath(raw_path.decode("utf-8"))
        if not allowed_relative_path(relative):
            continue
        data = subprocess.run(
            ["git", "-C", str(root), "cat-file", "blob", object_sha.decode("ascii")],
            check=True,
            capture_output=True,
        ).stdout
        entries.append((relative, data))
    return sorted(entries, key=lambda item: item[0].as_posix().lower())


def filesystem_entries(root: Path) -> list[tuple[PurePosixPath, bytes]]:
    files = sorted(
        (path for path in root.rglob("*") if is_workspace_file(path, root)),
        key=lambda path: path.as_posix().lower(),
    )
    return [(PurePosixPath(path.relative_to(root).as_posix()), path.read_bytes()) for path in files]


def main() -> None:
    parser = argparse.ArgumentParser(description="Create a deterministic full-workspace Converty ZIP.")
    parser.add_argument("--source-root", type=Path, default=ROOT)
    parser.add_argument("--output-dir", type=Path, default=ROOT.parent)
    args = parser.parse_args()

    source_root = args.source_root.resolve()
    entries = committed_entries(source_root)
    source_mode = "git-head"
    if entries is None:
        entries = filesystem_entries(source_root)
        source_mode = "filesystem"

    version_entry = next((data for relative, data in entries if relative.as_posix() == "VERSION"), None)
    if version_entry is None:
        raise SystemExit(f"VERSION is missing from workspace source: {source_root}")
    version = version_entry.decode("utf-8").strip()

    args.output_dir.mkdir(parents=True, exist_ok=True)
    archive = args.output_dir / f"Converty_{version}_full_workspace.zip"
    if archive.exists():
        archive.unlink()

    root_name = f"Converty_{version}"
    with zipfile.ZipFile(archive, "w", compression=zipfile.ZIP_DEFLATED, compresslevel=9) as zf:
        for relative, data in entries:
            arcname = f"{root_name}/{relative.as_posix()}"
            info = zipfile.ZipInfo(arcname)
            info.date_time = (2026, 8, 25, 0, 0, 0)
            info.compress_type = zipfile.ZIP_DEFLATED
            info.external_attr = 0o100644 << 16
            zf.writestr(info, data, compress_type=zipfile.ZIP_DEFLATED, compresslevel=9)

    result = {
        "archive": str(archive),
        "sha256": file_sha256(archive),
        "bytes": archive.stat().st_size,
        "files": len(entries),
        "source": source_mode,
    }
    print(json.dumps(result, indent=2))


if __name__ == "__main__":
    main()
