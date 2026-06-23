# <copyright file="add_file_headers.py" company="River-Mochi">
# Copyright (c) 2026 River-Mochi. All rights reserved.
# Licensed under the MIT License. You may not use this file except in compliance with this License.
# See LICENSE file in the project root for full license information.
# This notice and the MIT License notice must be kept with
# all copies or substantial portions of this code.
# ================= </copyright> ======================

# version 0.5.1
"""
Add standard River-Mochi MIT file headers to source files.

Dry run by default. Run commands from the repo root:

  # 1. Preview only. Use this first.
  py -3 Scripts/add_file_headers.py

  # 2. Add headers to files that do not already have one.
  py -3 Scripts/add_file_headers.py --apply

  # 3. Replace old headers with this exact current River-Mochi header.
  #    Use this when you intentionally want every supported source file updated.
  py -3 Scripts/add_file_headers.py --apply --replace-existing

  # 4. CI/check mode. Fails if any supported file still needs a header.
  py -3 Scripts/add_file_headers.py --check

  # 5. Strict CI/check mode. Also fails if an old header needs replacement.
  py -3 Scripts/add_file_headers.py --check --replace-existing

Supported source files:
  .cs
  .py
  .ps1

Not supported on purpose:
  .json, .xml, .scss, .css, .ts, .tsx
  Those file types either cannot safely use this same header style or are
  bundled into COHTML/UI output where extra comments are not always helpful.

Scan behavior:
  Uses git ls-files when available, so ignored folders such as bin, obj,
  node_modules, .git, and .vs are not scanned.

Repo-root behavior:
  The script finds the repo root by walking upward from its own location.
  This works when the script is in:
    /Scripts
    /Project/Scripts
    /Project/Project/Scripts

Important:
  Python must still be given the real path to this script. Repo-root detection
  cannot run until Python has opened this file.
"""

from __future__ import annotations

import argparse
import os
import subprocess
import sys
from dataclasses import dataclass
from pathlib import Path


UTF8_BOM = b"\xef\xbb\xbf"

SKIP_DIRS = {
    ".git",
    ".vs",
    "bin",
    "obj",
    "generated",
    "node_modules",
    "packages",
}

SUPPORTED_SUFFIXES = {
    ".cs": "//",
    ".py": "#",
    ".ps1": "#",
}


@dataclass
class FileResult:
    """Result from processing one file."""

    changed: bool
    new_text: str
    had_header: bool
    header_replaced: bool
    header_added: bool
    had_bom: bool
    had_crlf: bool
    utf8_error: bool


@dataclass
class RunStats:
    """Summary counters for one run."""

    candidate_files: int = 0
    supported_files: int = 0
    skipped_files: int = 0
    updated_files: int = 0
    unchanged_files: int = 0
    header_added: int = 0
    header_replaced: int = 0
    bom_found: int = 0
    crlf_found: int = 0
    utf8_errors: int = 0


def find_repo_root(script_path: Path) -> Path:
    """Find the repo root by walking upward from this script."""
    for parent in [script_path.parent, *script_path.parents]:
        if (parent / ".git").exists():
            return parent

    for parent in [script_path.parent, *script_path.parents]:
        if (parent / ".editorconfig").exists() or (parent / ".gitattributes").exists():
            return parent

    return Path.cwd().resolve()


def should_skip(path: Path) -> bool:
    """Return true for generated/build files that should not be edited."""
    if path.name.endswith(".g.cs"):
        return True

    parts = {part.lower() for part in path.parts}
    return any(skip_dir in parts for skip_dir in SKIP_DIRS)


def is_supported_source_file(path: Path) -> bool:
    """Return true if the file extension is supported."""
    return path.suffix.lower() in SUPPORTED_SUFFIXES


def normalize_lf(text: str) -> str:
    """Normalize line endings in files the script writes."""
    return text.replace("\r\n", "\n").replace("\r", "\n")


def read_utf8_text(path: Path) -> tuple[str, bool, bool, bool]:
    """Read UTF-8 text and report BOM, CRLF, and UTF-8 errors."""
    raw = path.read_bytes()
    had_bom = raw.startswith(UTF8_BOM)
    had_crlf = b"\r\n" in raw

    if had_bom:
        raw = raw[len(UTF8_BOM):]

    try:
        return raw.decode("utf-8"), had_bom, had_crlf, False
    except UnicodeDecodeError:
        return "", had_bom, had_crlf, True


def get_comment_prefix(path: Path) -> str:
    """Return the comment prefix for this source file."""
    return SUPPORTED_SUFFIXES[path.suffix.lower()]


def has_copyright_header(text: str) -> bool:
    """Return true if a copyright header appears near the top of the file."""
    top = text[:1500].lower()
    return "copyright" in top and "river-mochi" in top


def is_comment_line(line: str, prefix: str) -> bool:
    """Return true if the line is a comment line for this source type."""
    return line.lstrip().startswith(prefix)


def is_copyright_block_line(line: str) -> bool:
    """Return true if the line looks like part of a copyright/license block."""
    lower = line.lower()

    return (
        "copyright" in lower
        or "license" in lower
        or "river-mochi" in lower
        or "<copyright" in lower
        or "</copyright>" in lower
        or "all rights reserved" in lower
        or "all copies" in lower
        or "substantial portions" in lower
        or "project root" in lower
        or "license file" in lower
        or "license notice" in lower
        or "full license information" in lower
        or "full license info" in lower
    )


def find_existing_header_range(text: str, prefix: str) -> tuple[int, int] | None:
    """Find a top-of-file copyright block to remove.

    This supports both the current XML-style block and older comment-only blocks.
    It only removes a top comment block if that block contains copyright/license text.
    """
    lines = text.split("\n")

    start = 0
    while start < len(lines) and lines[start].strip() == "":
        start += 1

    if start >= len(lines):
        return None

    if not is_comment_line(lines[start], prefix):
        return None

    end = start
    saw_copyright_text = False
    saw_explicit_end = False

    while end < len(lines):
        line = lines[end]

        if not is_comment_line(line, prefix):
            break

        if is_copyright_block_line(line):
            saw_copyright_text = True

        if "</copyright>" in line.lower():
            saw_explicit_end = True
            end += 1
            break

        end += 1

    if not saw_copyright_text:
        return None

    # If there was no explicit </copyright>, keep the removal conservative:
    # remove only the leading comment block that contains copyright/license text.
    if not saw_explicit_end:
        while end > start and not is_copyright_block_line(lines[end - 1]):
            end -= 1

    while end < len(lines) and lines[end].strip() == "":
        end += 1

    return start, end


def remove_existing_header(text: str, prefix: str) -> tuple[str, bool]:
    """Remove an existing top-of-file copyright block."""
    header_range = find_existing_header_range(text, prefix)

    if header_range is None:
        return text, False

    start, end = header_range
    lines = text.split("\n")
    new_text = "\n".join(lines[:start] + lines[end:])

    return new_text, True


def make_header(path: Path, year: int) -> str:
    """Create the exact header for this source file."""
    prefix = get_comment_prefix(path)

    return (
        f'{prefix} <copyright file="{path.name}" company="River-Mochi">\n'
        f"{prefix} Copyright (c) {year} River-Mochi. All rights reserved.\n"
        f"{prefix} Licensed under the MIT License. You may not use this file except in compliance with this License.\n"
        f"{prefix} See LICENSE file in the project root for full license information.\n"
        f"{prefix} This notice and the MIT License notice must be kept with\n"
        f"{prefix} all copies or substantial portions of this code.\n"
        f"{prefix} ================= </copyright> ======================\n"
        "\n"
    )


def process_file(path: Path, year: int, replace_existing: bool) -> FileResult:
    """Return whether the file would change and the new file text."""
    original_text, had_bom, had_crlf, utf8_error = read_utf8_text(path)

    if utf8_error:
        return FileResult(
            changed=False,
            new_text="",
            had_header=False,
            header_replaced=False,
            header_added=False,
            had_bom=had_bom,
            had_crlf=had_crlf,
            utf8_error=True,
        )

    text = normalize_lf(original_text)
    prefix = get_comment_prefix(path)

    had_header = has_copyright_header(text)
    header_replaced = False
    header_added = False

    if replace_existing:
        text, header_replaced = remove_existing_header(text, prefix)

        if not header_replaced:
            header_added = True

        text = text.lstrip("\n")
        new_text = make_header(path, year) + text

        return FileResult(
            changed=had_bom or had_crlf or new_text != original_text,
            new_text=new_text,
            had_header=had_header,
            header_replaced=header_replaced,
            header_added=header_added,
            had_bom=had_bom,
            had_crlf=had_crlf,
            utf8_error=False,
        )

    if had_header:
        # Rewrites only when needed for UTF-8 no BOM or LF normalization.
        return FileResult(
            changed=had_bom or had_crlf or text != original_text,
            new_text=text,
            had_header=True,
            header_replaced=False,
            header_added=False,
            had_bom=had_bom,
            had_crlf=had_crlf,
            utf8_error=False,
        )

    text = text.lstrip("\n")
    new_text = make_header(path, year) + text

    return FileResult(
        changed=True,
        new_text=new_text,
        had_header=False,
        header_replaced=False,
        header_added=True,
        had_bom=had_bom,
        had_crlf=had_crlf,
        utf8_error=False,
    )


def try_git_ls_files(root: Path) -> list[Path] | None:
    """Return tracked and untracked non-ignored files using git, or None on failure."""
    try:
        completed = subprocess.run(
            ["git", "ls-files", "--cached", "--others", "--exclude-standard", "-z"],
            cwd=root,
            check=True,
            capture_output=True,
        )
    except (FileNotFoundError, subprocess.CalledProcessError):
        return None

    raw_paths = completed.stdout.split(b"\0")
    paths: list[Path] = []

    for raw_path in raw_paths:
        if not raw_path:
            continue

        try:
            relative_text = raw_path.decode("utf-8")
        except UnicodeDecodeError:
            relative_text = raw_path.decode(sys.getfilesystemencoding(), errors="replace")

        paths.append(root / relative_text)

    return paths


def walk_source_files(root: Path) -> list[Path]:
    """Fallback scanner that prunes skipped directories before entering them."""
    paths: list[Path] = []

    for current_root, dirnames, filenames in os.walk(root):
        current_path = Path(current_root)

        dirnames[:] = [
            dirname
            for dirname in dirnames
            if dirname.lower() not in SKIP_DIRS
        ]

        for filename in filenames:
            paths.append(current_path / filename)

    return paths


def iter_candidate_files(root: Path, use_git: bool) -> tuple[list[Path], str]:
    """Return candidate files and the scan method used."""
    if use_git:
        git_paths = try_git_ls_files(root)

        if git_paths is not None:
            return git_paths, "git ls-files"

    return walk_source_files(root), "os.walk fallback"


def print_summary(stats: RunStats, apply: bool) -> None:
    """Print a scan summary."""
    action_word = "Fixed" if apply else "Would fix"

    print()
    print("Summary")
    print("-------")
    print(f"Candidate files:      {stats.candidate_files}")
    print(f"Supported files:      {stats.supported_files}")
    print(f"Skipped files:        {stats.skipped_files}")
    print(f"Updated files:        {stats.updated_files}")
    print(f"Unchanged files:      {stats.unchanged_files}")
    print(f"Header added:         {stats.header_added}")
    print(f"Header replaced:      {stats.header_replaced}")
    print(f"UTF-8 BOM {action_word}:    {stats.bom_found}")
    print(f"CRLF {action_word}:         {stats.crlf_found}")
    print(f"UTF-8 decode errors:  {stats.utf8_errors}")
    print(f"Valid UTF-8:          {stats.utf8_errors == 0}")


def main() -> int:
    """Run the file header tool."""
    default_root = find_repo_root(Path(__file__).resolve())

    parser = argparse.ArgumentParser()
    parser.add_argument("--apply", action="store_true", help="Write changes.")
    parser.add_argument("--check", action="store_true", help="Fail if files need header updates.")
    parser.add_argument(
        "--replace-existing",
        action="store_true",
        help="Replace existing top-of-file copyright headers.",
    )
    parser.add_argument(
        "--root",
        default=str(default_root),
        help="Repo root. Default: auto-detected from this script.",
    )
    parser.add_argument("--year", type=int, default=2026)
    parser.add_argument(
        "--no-git",
        action="store_true",
        help="Do not use git ls-files; use directory walk fallback instead.",
    )
    args = parser.parse_args()

    if args.apply and args.check:
        print("ERROR: Use either --apply or --check, not both.", file=sys.stderr)
        return 2

    root = Path(args.root).resolve()
    stats = RunStats()

    candidate_files, scan_method = iter_candidate_files(
        root=root,
        use_git=not args.no_git,
    )

    print(f"Scan root: {root}")
    print(f"Script path: {Path(__file__).resolve()}")
    print(f"Scan method: {scan_method}")

    for path in sorted(candidate_files):
        stats.candidate_files += 1

        if not path.is_file():
            continue

        try:
            rel = path.relative_to(root)
        except ValueError:
            stats.skipped_files += 1
            continue

        if not is_supported_source_file(path):
            continue

        if should_skip(rel):
            stats.skipped_files += 1
            continue

        stats.supported_files += 1

        result = process_file(
            path=path,
            year=args.year,
            replace_existing=args.replace_existing,
        )

        if result.utf8_error:
            stats.utf8_errors += 1
            print(f"ERROR: Invalid UTF-8: {rel}")
            continue

        if result.header_added:
            stats.header_added += 1

        if result.header_replaced:
            stats.header_replaced += 1

        if result.had_bom:
            stats.bom_found += 1

        if result.had_crlf:
            stats.crlf_found += 1

        if not result.changed:
            stats.unchanged_files += 1
            continue

        stats.updated_files += 1

        if args.apply:
            path.write_text(result.new_text, encoding="utf-8", newline="\n")
            print(f"Updated: {rel}")
        else:
            print(f"Would update: {rel}")

    print_summary(stats, apply=args.apply)

    if args.check:
        if stats.updated_files or stats.utf8_errors:
            print()
            print(f"Header check failed. {stats.updated_files} file(s) need updates.")
            return 1

        print()
        print("Header check passed.")
        return 0

    if not args.apply:
        print()
        print("Dry run only. Re-run with --apply to write changes.")

    return 0


if __name__ == "__main__":
    raise SystemExit(main())
