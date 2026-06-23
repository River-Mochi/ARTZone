# <copyright file="CRLF2LF.py" company="River-Mochi">
# Copyright (c) 2026 River-Mochi. All rights reserved.
# Licensed under the MIT License. You may not use this file except in compliance with this License.
# See LICENSE file in the project root for full license information.
# This notice and the MIT License notice must be kept with
# all copies or substantial portions of this code.
# ================= </copyright> ======================

# File: Scripts/CRLF2LF.py
# Version: 0.3.1
# Purpose:
#   Safely check/fix CRLF or mixed line endings in tracked repo text files
#   that are supposed to be LF.
#
# Safe policy for VS2026 + GitHub:
#   - .sln stays CRLF and is NEVER touched by this script.
#   - .bat and .cmd stay CRLF and are NEVER touched by this script.
#   - .slnx is treated as LF because this repo's .gitattributes says:
#       *.slnx text eol=lf
#   - Only Git-tracked files are considered.
#   - Binary files are skipped.
#   - UTF-8 BOM is removed automatically when --fix is used.
#
# Usage:
#   Check only, no writes:
#     py -3 Scripts/CRLF2LF.py
#
#   Fix files:
#     py -3 Scripts/CRLF2LF.py --fix

from __future__ import annotations

import argparse
import subprocess
import sys
from pathlib import Path


UTF8_BOM = b"\xef\xbb\xbf"

# Files with these exact names are LF text files.
LF_TEXT_NAMES = {
    ".editorconfig",
    ".gitattributes",
    ".gitignore",
}

# Files with these extensions are LF text files.
# Keep this aligned with .gitattributes.
LF_TEXT_EXTENSIONS = {
    ".bash",
    ".config",
    ".cs",
    ".csproj",
    ".css",
    ".editorconfig",
    ".gitattributes",
    ".gitignore",
    ".html",
    ".js",
    ".jsx",
    ".json",
    ".jsonc",
    ".md",
    ".mjs",
    ".props",
    ".ps1",
    ".pubxml",
    ".py",
    ".resx",
    ".ruleset",
    ".scss",
    ".sh",
    ".slnx",
    ".svg",
    ".targets",
    ".toml",
    ".ts",
    ".tsx",
    ".txt",
    ".xml",
    ".yaml",
    ".yml",
}

# These are intentionally CRLF in this repo.
# Do not "fix" them to LF.
CRLF_EXTENSIONS = {
    ".sln",
    ".bat",
    ".cmd",
}

# Extra safety for generated/local folders, even if something odd is tracked.
SKIP_PARTS = {
    ".git",
    ".vs",
    "bin",
    "obj",
    "node_modules",
    "packages",
}


def run_git(args: list[str], cwd: Path) -> bytes:
    """Run a git command and return stdout."""
    try:
        result = subprocess.run(
            ["git", *args],
            cwd=cwd,
            check=True,
            stdout=subprocess.PIPE,
            stderr=subprocess.PIPE,
        )
    except FileNotFoundError:
        print("ERROR: git was not found on PATH.", file=sys.stderr)
        raise SystemExit(2)
    except subprocess.CalledProcessError as ex:
        message = ex.stderr.decode("utf-8", errors="replace").strip()
        if message:
            print(f"ERROR: git {' '.join(args)} failed:\n{message}", file=sys.stderr)
        else:
            print(f"ERROR: git {' '.join(args)} failed.", file=sys.stderr)
        raise SystemExit(2)

    return result.stdout


def get_repo_root() -> Path:
    """Return the Git repository root. Exit if not inside a Git repo."""
    output = run_git(["rev-parse", "--show-toplevel"], Path.cwd())
    return Path(output.decode("utf-8", errors="replace").strip())


def get_tracked_files(repo_root: Path) -> list[Path]:
    """Return all Git-tracked files as paths relative to repo root."""
    output = run_git(["ls-files", "-z"], repo_root)
    names = [name for name in output.decode("utf-8", errors="replace").split("\0") if name]
    return [Path(name) for name in names]


def should_skip_path(relative_path: Path) -> bool:
    """Return true if the path should never be touched."""
    lower_parts = {part.lower() for part in relative_path.parts}
    if lower_parts & SKIP_PARTS:
        return True

    suffix = relative_path.suffix.lower()
    if suffix in CRLF_EXTENSIONS:
        return True

    return False


def is_supported_lf_text_file(relative_path: Path) -> bool:
    """Return true if this tracked file is supposed to be LF text."""
    if should_skip_path(relative_path):
        return False

    name = relative_path.name.lower()
    suffix = relative_path.suffix.lower()

    return name in LF_TEXT_NAMES or suffix in LF_TEXT_EXTENSIONS


def normalize_to_lf_and_remove_bom(data: bytes) -> bytes:
    """Remove UTF-8 BOM, then normalize CRLF/mixed endings to LF."""
    if data.startswith(UTF8_BOM):
        data = data[len(UTF8_BOM):]

    return data.replace(b"\r\n", b"\n").replace(b"\r", b"\n")


def main() -> int:
    parser = argparse.ArgumentParser(
        description="Safely check/fix LF endings for supported Git-tracked text files."
    )

    parser.add_argument(
        "--fix",
        action="store_true",
        help="Rewrite supported LF text files that have CRLF, mixed endings, or UTF-8 BOM.",
    )

    args = parser.parse_args()

    write_files = args.fix
    repo_root = get_repo_root()
    tracked_files = get_tracked_files(repo_root)

    changed: list[Path] = []
    skipped_binary: list[Path] = []

    for relative_path in tracked_files:
        if not is_supported_lf_text_file(relative_path):
            continue

        full_path = repo_root / relative_path

        try:
            original_data = full_path.read_bytes()
        except OSError as ex:
            print(f"WARNING: Could not read {relative_path}: {ex}", file=sys.stderr)
            continue

        # Very simple binary guard.
        if b"\0" in original_data:
            skipped_binary.append(relative_path)
            continue

        new_data = normalize_to_lf_and_remove_bom(original_data)

        if new_data == original_data:
            continue

        changed.append(relative_path)

        if write_files:
            full_path.write_bytes(new_data)

    if write_files:
        if changed:
            print("Fixed LF line endings and/or UTF-8 BOM:")
            for path in changed:
                print(f"  {path.as_posix()}")
        else:
            print("No LF line-ending or UTF-8 BOM fixes needed.")
    else:
        if changed:
            print("These supported LF files need line-ending and/or UTF-8 BOM fixes:")
            for path in changed:
                print(f"  {path.as_posix()}")
            print()
            print("No files were changed. Run with --fix to write changes.")
        else:
            print("No LF line-ending or UTF-8 BOM fixes needed.")

    if skipped_binary:
        print()
        print("Skipped binary-looking tracked files:")
        for path in skipped_binary:
            print(f"  {path.as_posix()}")

    return 1 if changed and not write_files else 0


if __name__ == "__main__":
    raise SystemExit(main())
