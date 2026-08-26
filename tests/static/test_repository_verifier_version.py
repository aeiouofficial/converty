from __future__ import annotations

import subprocess
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]


def test_repository_verifier_uses_current_workspace_version() -> None:
    result = subprocess.run(
        [sys.executable, "scripts/verify_repository.py"],
        cwd=ROOT,
        text=True,
        capture_output=True,
        check=False,
    )
    assert result.returncode == 0, result.stderr or result.stdout
    current = (ROOT / "VERSION").read_text(encoding="utf-8").strip()
    assert f"PASS: version={current}" in result.stdout
