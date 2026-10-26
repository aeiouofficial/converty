"""Generated release manifests must have identical bytes on Windows and Linux."""

from pathlib import Path

import pytest

ROOT = Path(__file__).resolve().parents[2]
AUTHORITY = (
    "machine-readable/source_sbom.spdx.json",
    "machine-readable/release_sbom.spdx.json",
    "machine-readable/package_manifest.json",
    "SHA256SUMS.txt",
)


@pytest.mark.parametrize("relative_path", AUTHORITY)
def test_generated_authority_uses_lf_only(relative_path: str) -> None:
    data = (ROOT / relative_path).read_bytes()
    assert data, f"empty generated authority: {relative_path}"
    assert b"\r\n" not in data, f"Windows CRLF changes generated authority: {relative_path}"
