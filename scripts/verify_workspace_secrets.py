#!/usr/bin/env python3
from __future__ import annotations

import argparse
import re
from pathlib import Path

EXCLUDED_PARTS = {".git", ".packages", "artifacts", "__pycache__", ".pytest_cache", "bin", "obj", "TestResults"}
BINARY_SUFFIXES = {".png", ".jpg", ".jpeg", ".gif", ".ico", ".zip", ".dll", ".exe", ".wav", ".mp3", ".flac", ".webp", ".bmp", ".tif", ".tiff"}
PATTERNS = (
    ("private-key", re.compile(r"-----BEGIN (?:RSA |EC |OPENSSH )?PRIVATE KEY-----")),
    ("github-token", re.compile(r"gh[pousr]_[A-Za-z0-9]{20,}")),
    ("aws-access-key", re.compile(r"AKIA[0-9A-Z]{16}")),
    ("openai-key", re.compile(r"sk-[A-Za-z0-9_-]{20,}")),
)


def scan(root: Path) -> list[tuple[Path, str]]:
    findings: list[tuple[Path, str]] = []
    root = root.resolve()
    for path in sorted(root.rglob("*")):
        if not path.is_file():
            continue
        relative = path.relative_to(root)
        if any(part in EXCLUDED_PARTS for part in relative.parts):
            continue
        try:
            if path.suffix.lower() in BINARY_SUFFIXES or path.stat().st_size > 4 * 1024 * 1024:
                continue
            text = path.read_text(encoding="utf-8")
        except (UnicodeDecodeError, OSError):
            continue
        for name, pattern in PATTERNS:
            if pattern.search(text):
                findings.append((relative, name))
    return findings


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path, default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    findings = scan(args.root)
    if findings:
        print("workspace secret scan: FAIL")
        for path, kind in findings:
            print(f"- {path.as_posix()}: {kind}")
        return 2
    print("workspace secret scan: PASS")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
