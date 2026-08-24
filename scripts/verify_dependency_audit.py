#!/usr/bin/env python3
from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path
from typing import Any


def fail(message: str) -> int:
    print(f"dependency audit: FAIL - {message}", file=sys.stderr)
    return 1


def require_list(value: Any, label: str) -> list[Any]:
    if not isinstance(value, list):
        raise ValueError(f"{label} must be a list")
    return value


def main() -> int:
    parser = argparse.ArgumentParser(description="Fail closed on malformed or vulnerable NuGet package-list JSON.")
    parser.add_argument("report", type=Path)
    args = parser.parse_args()

    try:
        data = json.loads(args.report.read_text(encoding="utf-8-sig"))
    except (OSError, json.JSONDecodeError) as exc:
        return fail(f"cannot parse report JSON: {exc}")

    if not isinstance(data, dict):
        return fail("report root must be an object")
    if data.get("version") != 1:
        return fail("NuGet package-list output must use JSON version 1")

    try:
        projects = require_list(data.get("projects"), "projects")
        project_count = framework_count = package_count = 0
        findings: list[str] = []
        for project_index, project in enumerate(projects):
            if not isinstance(project, dict):
                raise ValueError(f"projects[{project_index}] must be an object")
            project_path = project.get("path")
            if not isinstance(project_path, str) or not project_path:
                raise ValueError(f"projects[{project_index}].path must be a non-empty string")
            project_count += 1
            frameworks = require_list(project.get("frameworks"), f"{project_path}.frameworks")
            for framework_index, framework in enumerate(frameworks):
                if not isinstance(framework, dict):
                    raise ValueError(f"{project_path}.frameworks[{framework_index}] must be an object")
                framework_name = framework.get("framework")
                if not isinstance(framework_name, str) or not framework_name:
                    raise ValueError(f"{project_path}.frameworks[{framework_index}].framework must be a non-empty string")
                framework_count += 1
                for collection_name in ("topLevelPackages", "transitivePackages"):
                    raw_packages = framework.get(collection_name, [])
                    packages = require_list(raw_packages, f"{project_path}/{framework_name}.{collection_name}")
                    for package_index, package in enumerate(packages):
                        if not isinstance(package, dict):
                            raise ValueError(f"{project_path}/{framework_name}.{collection_name}[{package_index}] must be an object")
                        package_id = package.get("id")
                        resolved = package.get("resolvedVersion")
                        if not isinstance(package_id, str) or not package_id:
                            raise ValueError("package id must be a non-empty string")
                        if not isinstance(resolved, str) or not resolved:
                            raise ValueError(f"{package_id} resolvedVersion must be a non-empty string")
                        package_count += 1
                        vulnerabilities = package.get("vulnerabilities", [])
                        vulnerabilities = require_list(vulnerabilities, f"{package_id}.vulnerabilities")
                        for vulnerability_index, vulnerability in enumerate(vulnerabilities):
                            if not isinstance(vulnerability, dict):
                                raise ValueError(f"{package_id}.vulnerabilities[{vulnerability_index}] must be an object")
                            severity = vulnerability.get("severity")
                            advisory = vulnerability.get("advisoryurl")
                            if not isinstance(severity, str) or not severity:
                                raise ValueError(f"{package_id} vulnerability severity must be a non-empty string")
                            if not isinstance(advisory, str) or not advisory.startswith("https://"):
                                raise ValueError(f"{package_id} vulnerability advisoryurl must be an HTTPS URL")
                            findings.append(f"{package_id} {resolved}: {severity} {advisory}")
    except ValueError as exc:
        return fail(str(exc))

    if findings:
        print("dependency audit: FAIL - known vulnerabilities reported", file=sys.stderr)
        for finding in findings:
            print(f"- {finding}", file=sys.stderr)
        return 1

    print(f"dependency audit: PASS ({project_count} projects, {framework_count} frameworks, {package_count} vulnerable-result packages)")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
