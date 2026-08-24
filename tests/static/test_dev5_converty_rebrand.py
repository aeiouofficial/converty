from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
LEGACY = "File" + "Convert"
ACTIVE_ROOTS = ("src", "tests", "native", "providers", "packaging")
TEXT_SUFFIXES = {".cs", ".csproj", ".slnx", ".ps1", ".py", ".json", ".yml", ".yaml", ".md", ".txt", ".dot", ".cmake"}
HISTORICAL_PREFIXES = (
    "docs/superpowers/plans/2026-08-24-foundation-",
    "source/",
)
HISTORICAL_FILES = {"CHANGELOG.md"}


def _is_historical(path: Path) -> bool:
    relative = path.relative_to(ROOT).as_posix()
    return relative in HISTORICAL_FILES or any(relative.startswith(prefix) for prefix in HISTORICAL_PREFIXES)


def _active_text_files():
    explicit = {
        ROOT / "README.md",
        ROOT / "SECURITY.md",
        ROOT / "CMakeLists.txt",
        ROOT / "CMakePresets.json",
        ROOT / "Directory.Build.props",
        ROOT / "Directory.Packages.props",
        ROOT / "SHA256SUMS.txt",
    }
    for path in explicit:
        if path.exists():
            yield path

    for root_name in (*ACTIVE_ROOTS, "build", "scripts", "docs", "machine-readable", "reference-images"):
        base = ROOT / root_name
        if not base.exists():
            continue
        for path in base.rglob("*"):
            if not path.is_file() or _is_historical(path):
                continue
            if path.suffix.lower() in TEXT_SUFFIXES or path.name in {"CMakeLists.txt", "MODULE.md"}:
                yield path


def test_authoritative_solution_and_master_plan_use_converty_name():
    assert (ROOT / "Converty.slnx").is_file()
    assert not (ROOT / (LEGACY + ".slnx")).exists()
    assert (ROOT / "docs" / "Converty_Master_Build_Plan.md").is_file()
    assert not (ROOT / "docs" / (LEGACY + "_Master_Build_Plan.md")).exists()


def test_active_project_module_and_test_paths_do_not_use_legacy_brand():
    offenders = []
    for root_name in ACTIVE_ROOTS:
        base = ROOT / root_name
        if not base.exists():
            continue
        for path in base.rglob("*"):
            relative = path.relative_to(ROOT).as_posix()
            if LEGACY in relative:
                offenders.append(relative)
    assert offenders == []


def test_active_text_authority_does_not_use_legacy_brand():
    offenders = []
    seen = set()
    for path in _active_text_files():
        relative = path.relative_to(ROOT).as_posix()
        if relative in seen:
            continue
        seen.add(relative)
        text = path.read_text(encoding="utf-8")
        if LEGACY in text:
            offenders.append(relative)
    assert offenders == []


def test_managed_projects_and_namespaces_use_converty_prefix():
    projects = sorted((ROOT / "src").glob("Converty.*/*.csproj")) + sorted((ROOT / "tests").glob("Converty.*.Tests/*.csproj"))
    assert projects
    assert all(project.name.startswith("Converty.") for project in projects)

    csharp_files = [
        path
        for root_name in ("src", "tests")
        for path in (ROOT / root_name).rglob("*.cs")
    ]
    assert csharp_files
    assert all(LEGACY not in path.read_text(encoding="utf-8") for path in csharp_files)


def test_workspace_package_name_is_converty():
    package_script = (ROOT / "scripts" / "package_workspace.py").read_text(encoding="utf-8")
    assert "Converty_" in package_script
    assert LEGACY + "_" not in package_script


def test_readme_uses_converty_as_product_heading():
    first_line = (ROOT / "README.md").read_text(encoding="utf-8").splitlines()[0]
    assert first_line.strip() == "# Converty"
