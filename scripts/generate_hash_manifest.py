#!/usr/bin/env python3
from __future__ import annotations

import hashlib
from pathlib import Path

from workspace_file_policy import is_workspace_file

ROOT = Path(__file__).resolve().parents[1]
OUTPUT = ROOT / "SHA256SUMS.txt"
EXCLUDED_FILES = {OUTPUT.name}


def sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as handle:
        for chunk in iter(lambda: handle.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest()


def main() -> None:
    files = sorted(
        (path for path in ROOT.rglob("*") if is_workspace_file(path, ROOT, excluded_files=EXCLUDED_FILES)),
        key=lambda path: path.as_posix().lower(),
    )
    lines = [f"{sha256(path)}  {path.relative_to(ROOT).as_posix()}" for path in files]
    OUTPUT.write_text("\n".join(lines) + "\n", encoding="utf-8")
    print(f"Wrote {len(lines)} hashes to {OUTPUT.name}")


if __name__ == "__main__":
    main()
