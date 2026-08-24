#!/usr/bin/env python3
from __future__ import annotations

import json
import sys
from pathlib import Path

import jsonschema

ROOT = Path(__file__).resolve().parents[1]
VECTOR_DIR = ROOT / "tests" / "vectors" / "v1"
SCHEMA = json.loads((ROOT / "schemas" / "v1" / "conversion-request.schema.json").read_text(encoding="utf-8"))
VALIDATOR = jsonschema.Draft202012Validator(SCHEMA, format_checker=jsonschema.FormatChecker())


class DuplicateMemberError(ValueError):
    pass


def reject_duplicate_members(pairs: list[tuple[str, object]]) -> dict[str, object]:
    result: dict[str, object] = {}
    for key, value in pairs:
        if key in result:
            raise DuplicateMemberError(f"duplicate JSON property: {key}")
        result[key] = value
    return result


def load_strict(raw: str) -> object:
    return json.loads(raw, object_pairs_hook=reject_duplicate_members)


def main() -> int:
    manifest = json.loads((VECTOR_DIR / "manifest.json").read_text(encoding="utf-8"))
    failures: list[str] = []
    cases = manifest.get("cases", [])

    for case in cases:
        case_id = case["id"]
        expectation = case["expect"]
        raw = (VECTOR_DIR / case["file"]).read_text(encoding="utf-8")
        try:
            value = load_strict(raw)
        except DuplicateMemberError:
            if expectation != "duplicateReject":
                failures.append(f"{case_id}: duplicate member rejected unexpectedly")
            continue
        except json.JSONDecodeError as exc:
            failures.append(f"{case_id}: invalid JSON syntax: {exc}")
            continue

        errors = list(VALIDATOR.iter_errors(value))
        if expectation == "valid" and errors:
            failures.append(f"{case_id}: expected valid, schema rejected: {errors[0].message}")
        elif expectation == "schemaReject" and not errors:
            failures.append(f"{case_id}: expected schema rejection but vector was accepted")
        elif expectation == "duplicateReject":
            failures.append(f"{case_id}: expected duplicate rejection but strict parser accepted it")
        elif expectation not in {"valid", "schemaReject", "duplicateReject"}:
            failures.append(f"{case_id}: unknown manifest expectation {expectation!r}")

    if failures:
        print("contract vectors: FAIL", file=sys.stderr)
        for failure in failures:
            print(f"- {failure}", file=sys.stderr)
        return 2

    print(f"contract vectors: PASS ({len(cases)} cases)")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
