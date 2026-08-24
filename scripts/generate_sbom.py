#!/usr/bin/env python3
from __future__ import annotations

import argparse
import hashlib
import json
import re
import sys
import xml.etree.ElementTree as ET
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
CREATED = "2026-08-24T00:00:00Z"


def fail(message: str) -> None:
    print(f"SBOM generation failed: {message}", file=sys.stderr)
    raise SystemExit(2)


def project_name(project: Path) -> str:
    tree = ET.parse(project)
    assembly = tree.find(".//AssemblyName")
    if assembly is not None and assembly.text:
        return assembly.text.strip()
    return project.stem


def spdx_id(prefix: str, value: str) -> str:
    safe = re.sub(r"[^A-Za-z0-9.-]", "-", value)
    return f"SPDXRef-{prefix}-{safe}"


def managed_projects() -> list[Path]:
    return sorted(
        (p for p in ROOT.rglob("*.csproj") if not any(part in {"bin", "obj", "artifacts"} for part in p.parts)),
        key=lambda p: p.relative_to(ROOT).as_posix().lower(),
    )


def require_lock_files(projects: list[Path]) -> list[Path]:
    locks: list[Path] = []
    missing: list[str] = []
    for project in projects:
        lock = project.parent / "packages.lock.json"
        if lock.is_file():
            locks.append(lock)
        else:
            missing.append(project.relative_to(ROOT).as_posix())
    if missing:
        fail("packages.lock.json missing for: " + ", ".join(missing))
    return locks


def nuget_packages(locks: list[Path]) -> list[tuple[str, str]]:
    found: set[tuple[str, str]] = set()
    for lock in locks:
        data = json.loads(lock.read_text(encoding="utf-8"))
        dependencies = data.get("dependencies")
        if not isinstance(dependencies, dict):
            fail(f"invalid dependencies object in {lock.relative_to(ROOT)}")
        for framework in dependencies.values():
            if not isinstance(framework, dict):
                fail(f"invalid framework dependency map in {lock.relative_to(ROOT)}")
            for package_name, package_info in framework.items():
                if not isinstance(package_info, dict):
                    fail(f"invalid package entry {package_name!r} in {lock.relative_to(ROOT)}")
                version = package_info.get("resolved")
                if not isinstance(version, str) or not version:
                    fail(f"package {package_name!r} has no resolved version in {lock.relative_to(ROOT)}")
                found.add((package_name, version))
    return sorted(found, key=lambda item: (item[0].lower(), item[1]))


def build_document(mode: str) -> dict:
    version = (ROOT / "VERSION").read_text(encoding="utf-8").strip()
    projects = managed_projects()
    packages: list[dict] = []

    for project in projects:
        name = project_name(project)
        packages.append({
            "SPDXID": spdx_id("Project", name),
            "name": name,
            "versionInfo": version,
            "downloadLocation": "NOASSERTION",
            "filesAnalyzed": False,
            "licenseConcluded": "NOASSERTION",
            "licenseDeclared": "NOASSERTION",
            "copyrightText": "NOASSERTION",
            "comment": f"First-party managed project: {project.relative_to(ROOT).as_posix()}",
        })

    if mode == "release":
        locks = require_lock_files(projects)
        for name, resolved in nuget_packages(locks):
            packages.append({
                "SPDXID": spdx_id("NuGet", f"{name}-{resolved}"),
                "name": name,
                "versionInfo": resolved,
                "downloadLocation": "NOASSERTION",
                "filesAnalyzed": False,
                "licenseConcluded": "NOASSERTION",
                "licenseDeclared": "NOASSERTION",
                "copyrightText": "NOASSERTION",
                "externalRefs": [{
                    "referenceCategory": "PACKAGE-MANAGER",
                    "referenceType": "purl",
                    "referenceLocator": f"pkg:nuget/{name}@{resolved}",
                }],
            })

    seed = "\n".join(f"{p['name']}@{p.get('versionInfo','')}" for p in packages).encode("utf-8")
    namespace_hash = hashlib.sha256(seed).hexdigest()
    document_id = "SPDXRef-DOCUMENT"
    relationships = [
        {"spdxElementId": document_id, "relationshipType": "DESCRIBES", "relatedSpdxElement": p["SPDXID"]}
        for p in packages
    ]
    return {
        "spdxVersion": "SPDX-2.3",
        "dataLicense": "CC0-1.0",
        "SPDXID": document_id,
        "name": f"FileConvert-{mode}-{version}",
        "documentNamespace": f"https://fileconvert.invalid/spdx/{mode}/{version}/{namespace_hash}",
        "creationInfo": {"created": CREATED, "creators": ["Tool: FileConvert deterministic SBOM generator"]},
        "packages": packages,
        "relationships": relationships,
        "comment": "Source mode is development inventory only. Release mode requires committed NuGet lock files and still requires human license/vulnerability review.",
    }


def main() -> None:
    parser = argparse.ArgumentParser(description="Generate deterministic SPDX 2.3 JSON for FileConvert.")
    parser.add_argument("--mode", choices=("source", "release"), required=True)
    args = parser.parse_args()
    document = build_document(args.mode)
    output = ROOT / "machine-readable" / f"{args.mode}_sbom.spdx.json"
    output.write_text(json.dumps(document, indent=2, sort_keys=True) + "\n", encoding="utf-8")
    print(f"Wrote {output.relative_to(ROOT)} with {len(document['packages'])} packages")


if __name__ == "__main__":
    main()
