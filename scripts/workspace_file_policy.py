from __future__ import annotations

from pathlib import Path
from typing import Collection

# Workspace archives are source/handover snapshots, never build-cache snapshots.
EXCLUDED_PARTS = frozenset(
    {
        ".git",
        ".packages",
        "artifacts",
        "__pycache__",
        ".pytest_cache",
        "bin",
        "obj",
        "TestResults",
    }
)
EXCLUDED_SUFFIXES = frozenset({".pyc", ".pyo", ".pfx", ".p12", ".key", ".pem"})
EXCLUDED_NAMES = frozenset({".env"})


def is_workspace_file(
    path: Path,
    root: Path,
    *,
    excluded_files: Collection[str] = (),
) -> bool:
    if not path.is_file() or path.name in excluded_files or path.name in EXCLUDED_NAMES:
        return False

    relative = path.relative_to(root)
    if any(part in EXCLUDED_PARTS for part in relative.parts):
        return False

    return path.suffix.lower() not in EXCLUDED_SUFFIXES
