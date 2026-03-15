# File: src/Scripts/check_all_locales.py
# Purpose:
#   Run both localization checkers:
#   - check_locales.py   for C# Locale*.cs files
#   - check_lang_json.py for JSON lang/*.json files
#
# Works fine from Git Bash because it is just Python running Python.

import argparse
import subprocess
import sys
from pathlib import Path


def run_check(label: str, script_path: Path, extra_args: list[str]) -> int:
    print("\n" + "=" * 60)
    print(label)
    print("=" * 60)

    cmd = [sys.executable, str(script_path), *extra_args]
    result = subprocess.run(cmd)
    return result.returncode


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument(
        "--verbose",
        action="store_true",
        help="Pass --verbose to both checkers",
    )
    ap.add_argument(
        "--fix-bom",
        action="store_true",
        help="Pass --fix-bom to check_lang_json.py",
    )
    args = ap.parse_args()

    script_dir = Path(__file__).resolve().parent

    locales_script = script_dir / "check_locales.py"
    json_script = script_dir / "check_lang_json.py"

    if not locales_script.exists():
        print(f"ERROR: Missing script: {locales_script}")
        return 2

    if not json_script.exists():
        print(f"ERROR: Missing script: {json_script}")
        return 2

    locale_args: list[str] = []
    json_args: list[str] = []

    if args.verbose:
        locale_args.append("--verbose")
        json_args.append("--verbose")

    if args.fix_bom:
        json_args.append("--fix-bom")

    rc_locales = run_check("C# locale check", locales_script, locale_args)
    rc_json = run_check("JSON lang check", json_script, json_args)

    overall_rc = max(rc_locales, rc_json)

    print("\n" + "=" * 60)
    if overall_rc == 0:
        print("ALL Localization checks GOOD")
    elif overall_rc == 1:
        print("Localization checks FOUND PROBLEMS")
    else:
        print("Localization checks FAILED TO RUN")
    print("=" * 60)

    return overall_rc


if __name__ == "__main__":
    raise SystemExit(main())
