#!/usr/bin/env python3
from __future__ import annotations

import hashlib
import json
from pathlib import Path

from workspace_file_policy import EXCLUDED_NAMES, EXCLUDED_PARTS, EXCLUDED_SUFFIXES, is_workspace_file

ROOT = Path(__file__).resolve().parents[1]
OUTPUT = ROOT / "machine-readable" / "package_manifest.json"
EXCLUDED_FILES = {OUTPUT.name, "SHA256SUMS.txt"}


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
    manifest = {
        "schemaVersion": 1,
        "workspaceVersion": (ROOT / "VERSION").read_text(encoding="utf-8").strip(),
        "generatedDate": "2026-08-24",
        "excludedParts": sorted(EXCLUDED_PARTS),
        "excludedSuffixes": sorted(EXCLUDED_SUFFIXES),
        "excludedNames": sorted(EXCLUDED_NAMES),
        "excludedFiles": sorted(EXCLUDED_FILES),
        "files": [
            {
                "path": path.relative_to(ROOT).as_posix(),
                "bytes": path.stat().st_size,
                "sha256": sha256(path),
            }
            for path in files
        ],
    }
    OUTPUT.parent.mkdir(parents=True, exist_ok=True)
    OUTPUT.write_text(json.dumps(manifest, indent=2) + "\n", encoding="utf-8")
    print(f"Wrote {len(files)} entries to {OUTPUT.relative_to(ROOT)}")


if __name__ == "__main__":
    main()
