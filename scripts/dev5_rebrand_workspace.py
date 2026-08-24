from __future__ import annotations

from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
LEGACY = "File" + "Convert"
BRAND = "Converty"
TEXT_SUFFIXES = {
    ".cs",
    ".csproj",
    ".slnx",
    ".ps1",
    ".py",
    ".json",
    ".yml",
    ".yaml",
    ".md",
    ".txt",
    ".dot",
    ".cmake",
    ".props",
    ".targets",
}
SPECIAL_TEXT_NAMES = {"CMakeLists.txt", "MODULE.md"}
HISTORICAL_PREFIXES = (
    "source/",
    "docs/superpowers/plans/2026-08-24-foundation-",
)
HISTORICAL_FILES = {"CHANGELOG.md"}
SKIP_DIR_NAMES = {".git", ".pytest_cache", "bin", "obj", "artifacts"}


def relative(path: Path) -> str:
    return path.relative_to(ROOT).as_posix()


def is_historical(path: Path) -> bool:
    rel = relative(path)
    return rel in HISTORICAL_FILES or any(rel.startswith(prefix) for prefix in HISTORICAL_PREFIXES)


def is_skipped(path: Path) -> bool:
    try:
        parts = path.relative_to(ROOT).parts
    except ValueError:
        return True
    return any(part in SKIP_DIR_NAMES for part in parts)


def rename_paths() -> None:
    directories = [
        path
        for path in ROOT.rglob("*")
        if path.is_dir() and not is_skipped(path) and LEGACY in path.name and not is_historical(path)
    ]
    for path in sorted(directories, key=lambda item: len(item.parts), reverse=True):
        path.rename(path.with_name(path.name.replace(LEGACY, BRAND)))

    files = [
        path
        for path in ROOT.rglob("*")
        if path.is_file() and not is_skipped(path) and LEGACY in path.name and not is_historical(path)
    ]
    for path in files:
        path.rename(path.with_name(path.name.replace(LEGACY, BRAND)))


def rewrite_active_text() -> None:
    for path in ROOT.rglob("*"):
        if not path.is_file() or is_skipped(path) or is_historical(path):
            continue
        if path.suffix.lower() not in TEXT_SUFFIXES and path.name not in SPECIAL_TEXT_NAMES:
            continue
        text = path.read_text(encoding="utf-8")
        updated = text.replace(LEGACY, BRAND).replace(LEGACY.lower(), BRAND.lower()).replace(LEGACY.upper(), BRAND.upper())
        if updated != text:
            path.write_text(updated, encoding="utf-8", newline="\n")


def remove_dependency_locks() -> None:
    for path in ROOT.rglob("packages.lock.json"):
        if not is_skipped(path):
            path.unlink()


def verify_no_active_legacy_identity() -> None:
    offenders: list[str] = []
    for path in ROOT.rglob("*"):
        if is_skipped(path) or is_historical(path):
            continue
        rel = relative(path)
        if LEGACY in rel:
            offenders.append(rel)
            continue
        if not path.is_file():
            continue
        if path.suffix.lower() not in TEXT_SUFFIXES and path.name not in SPECIAL_TEXT_NAMES:
            continue
        if LEGACY in path.read_text(encoding="utf-8"):
            offenders.append(rel)
    if offenders:
        raise SystemExit("Active legacy identity remains after migration:\n" + "\n".join(sorted(set(offenders))))


def main() -> None:
    rename_paths()
    rewrite_active_text()
    remove_dependency_locks()
    verify_no_active_legacy_identity()
    print("Converty rebrand migration complete; dependency locks intentionally removed for regeneration.")


if __name__ == "__main__":
    main()
