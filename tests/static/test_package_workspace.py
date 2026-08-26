from __future__ import annotations

import subprocess
import sys
import zipfile
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
PACKAGE_SCRIPT = ROOT / "scripts" / "package_workspace.py"


def run(*args: str, cwd: Path) -> subprocess.CompletedProcess[str]:
    return subprocess.run(
        args,
        cwd=cwd,
        check=False,
        capture_output=True,
        text=True,
        encoding="utf-8",
    )


def test_package_uses_committed_git_bytes_not_mutated_worktree(tmp_path: Path) -> None:
    source = tmp_path / "source"
    output = tmp_path / "output"
    source.mkdir()
    output.mkdir()

    assert run("git", "init", "-b", "main", cwd=source).returncode == 0
    assert run("git", "config", "user.name", "Converty Test", cwd=source).returncode == 0
    assert run("git", "config", "user.email", "converty-test@example.invalid", cwd=source).returncode == 0

    (source / "VERSION").write_bytes(b"0.1.0-dev.7\n")
    (source / "sample.txt").write_bytes(b"committed\nbytes\n")
    assert run("git", "add", "VERSION", "sample.txt", cwd=source).returncode == 0
    assert run("git", "commit", "-m", "fixture", cwd=source).returncode == 0

    # Simulate a platform checkout/build mutating text bytes after the commit.
    (source / "sample.txt").write_bytes(b"committed\r\nbytes\r\n")

    result = run(
        sys.executable,
        str(PACKAGE_SCRIPT),
        "--source-root",
        str(source),
        "--output-dir",
        str(output),
        cwd=ROOT,
    )
    assert result.returncode == 0, result.stderr

    archive = output / "Converty_0.1.0-dev.7_full_workspace.zip"
    with zipfile.ZipFile(archive) as package:
        assert package.read("Converty_0.1.0-dev.7/sample.txt") == b"committed\nbytes\n"
        assert package.read("Converty_0.1.0-dev.7/VERSION") == b"0.1.0-dev.7\n"
