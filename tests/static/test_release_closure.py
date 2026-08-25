from __future__ import annotations

import json
import subprocess
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
CURRENT = "0.1.0-dev.6"
NEXT = "0.1.0-dev.7"


def data(path: str) -> dict:
    return json.loads((ROOT / path).read_text(encoding="utf-8"))


def test_workspace_authority_is_synchronized() -> None:
    assert (ROOT / "VERSION").read_text(encoding="utf-8").strip() == CURRENT
    assert data("eng/toolchain.json")["workspaceVersion"] == CURRENT
    assert data("machine-readable/release_policy.json")["workspaceVersion"] == CURRENT
    assert data("machine-readable/ci_action_pins.json")["workspaceVersion"] == CURRENT
    assert data("machine-readable/build_evidence.json")["workspaceVersion"] == CURRENT
    handover = data("machine-readable/handover_state.json")
    assert handover["workspaceVersion"] == CURRENT
    assert handover["nextWorkspaceVersion"] == NEXT


def test_every_managed_project_has_a_committed_lock_file() -> None:
    projects = sorted(path for path in ROOT.rglob("*.csproj") if not any(part in {"bin", "obj", "artifacts"} for part in path.parts))
    assert len(projects) == 15
    assert all((project.parent / "packages.lock.json").is_file() for project in projects)


def test_release_sbom_generator_skips_project_references_as_nuget_packages() -> None:
    result = subprocess.run([sys.executable, "scripts/generate_sbom.py", "--mode", "release"], cwd=ROOT, text=True, capture_output=True, check=False)
    assert result.returncode == 0, result.stderr or result.stdout
    sbom = data("machine-readable/release_sbom.spdx.json")
    nuget_names = {package["name"].lower() for package in sbom["packages"] if package["SPDXID"].startswith("SPDXRef-NuGet-")}
    assert "converty.contracts" not in nuget_names
    assert "converty.core" not in nuget_names
    assert "converty.host" not in nuget_names
    assert "converty.bridge" not in nuget_names
    assert "xunit.v3.mtp-v2" in nuget_names


def test_dev6_managed_behavior_qualification_is_recorded() -> None:
    managed = data("machine-readable/build_evidence.json")["behaviorQualification"]["managed"]
    assert managed["lockedRestore"] == "PASS"
    assert managed["managedProjects"] == 15
    assert managed["dependencyAudit"] == "PASS"
    assert managed["vulnerablePackages"] == 0
    assert managed["releaseBuild"] == "PASS"
    assert managed["warnings"] == 0
    assert managed["errors"] == 0
    assert managed["tests"] == {"total": 108, "succeeded": 108, "failed": 0, "skipped": 0}
    assert managed["ipcFuzzCorpus"]["cases"] == 7
    assert managed["nativeTopologySmoke"] == "PASS"


def test_permanent_ci_runs_release_preflight_and_release_sbom() -> None:
    workflow = (ROOT / ".github/workflows/ci.yml").read_text(encoding="utf-8")
    preflight = "python scripts/verify_release_inputs.py"
    release_sbom = "python scripts/generate_sbom.py --mode release"
    static_gate = "python -m pytest -q tests/static"
    assert preflight in workflow
    assert release_sbom in workflow
    assert workflow.index(preflight) < workflow.index(release_sbom) < workflow.index(static_gate)


def test_source_sbom_and_readme_are_current() -> None:
    sbom = data("machine-readable/source_sbom.spdx.json")
    assert sbom["name"] == f"Converty-source-{CURRENT}"
    assert all(package["versionInfo"] == CURRENT for package in sbom["packages"])
    readme = (ROOT / "README.md").read_text(encoding="utf-8")
    assert f"**{CURRENT}**" in readme
