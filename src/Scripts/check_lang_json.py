# File: src/Scripts/check_lang_json.py
# Purpose:
#   Check JSON translation files in /lang or /src/lang.
#
#   What it checks:
#   - Missing keys vs English baseline
#   - Extra keys vs English baseline
#   - Marker balance in values: **, < >, { }
#   - Placeholder mismatch vs baseline: {0}, {1}, ...
#   - Invalid JSON / wrong top-level shape
#   - UTF-8 BOM presence (warn by default, optionally fix with --fix-bom)
#
# Output behavior:
#   - Default: print only files with problems or BOM fixes
#   - If no problems anywhere: print "All checks GOOD - no problems detected."
#   - Use --verbose to print every file report
#
# Exit codes:
#   - 0 = no unresolved problems
#   - 1 = problems found
#   - 2 = configuration / filesystem error

import argparse
import json
import re
from collections import defaultdict
from pathlib import Path
from typing import Dict, List, Optional, Set, Tuple


UTF8_BOM = b"\xef\xbb\xbf"

# Folders to skip during recursive search.
SKIP_DIR_NAMES = {
    ".git",
    ".vs",
    ".idea",
    ".vscode",
    "bin",
    "obj",
    "node_modules",
    "dist",
    "build",
    "__pycache__",
}


def _has_child_case_insensitive(parent: Path, target_name: str) -> bool:
    """Return True if parent contains target_name, case-insensitively."""
    if not parent.exists() or not parent.is_dir():
        return False

    target_lower = target_name.lower()

    try:
        for child in parent.iterdir():
            if child.name.lower() == target_lower:
                return True
    except OSError:
        return False

    return False


def find_repo_root(start: Path) -> Optional[Path]:
    """
    Walk upward to find a likely repo root.

    Preference order:
    1) .git
    2) .gitignore
    3) README.md (case-insensitive)
    """
    p = start.resolve()

    for parent in [p] + list(p.parents):
        if (parent / ".git").exists():
            return parent

    for parent in [p] + list(p.parents):
        if _has_child_case_insensitive(parent, ".gitignore"):
            return parent

    for parent in [p] + list(p.parents):
        if _has_child_case_insensitive(parent, "README.md"):
            return parent

    return None


def _common_lang_dir_candidates(repo_root: Path) -> List[Path]:
    """Common JSON translation folder guesses."""
    return [
        repo_root / "src" / "lang",
        repo_root / "lang",
    ]


def _score_baseline_parent(parent: Path) -> int:
    """Score a folder for how likely it is to be the real lang folder."""
    score = 0
    parts_lower = [p.lower() for p in parent.parts]
    name_lower = parent.name.lower()

    if name_lower == "lang":
        score += 100

    if "src" in parts_lower:
        score += 10

    # Slight preference for shallower paths.
    score -= len(parent.parts)

    return score


def _recursive_find_lang_dir(repo_root: Path, baseline: str) -> Optional[Path]:
    """
    Fallback search:
    recursively look for the baseline file anywhere under repo_root,
    skipping obvious junk folders.
    """
    parents_seen: Set[Path] = set()
    candidates: List[Tuple[int, Path]] = []

    for p in repo_root.rglob(baseline):
        if not p.is_file():
            continue

        parts_lower = {part.lower() for part in p.parts}
        if parts_lower & SKIP_DIR_NAMES:
            continue

        parent = p.parent
        if parent in parents_seen:
            continue

        parents_seen.add(parent)
        candidates.append((_score_baseline_parent(parent), parent))

    if not candidates:
        return None

    candidates.sort(key=lambda x: x[0], reverse=True)
    return candidates[0][1]


def resolve_lang_dir(repo_root: Path, baseline: str, lang_dir_arg: str) -> Optional[Path]:
    """
    Resolve the JSON lang directory.

    Behavior:
    - If --lang-dir was given, use it directly relative to repo root.
    - Otherwise try common locations first.
    - If still not found, do a recursive search for the baseline file.
    """
    if lang_dir_arg.lower() != "auto":
        return repo_root / lang_dir_arg

    for c in _common_lang_dir_candidates(repo_root):
        if c.exists() and (c / baseline).exists():
            return c

    return _recursive_find_lang_dir(repo_root, baseline)


def build_fix_bom_command(script_path: Path, repo_root: Path) -> str:
    """
    Build a copy-pasteable fix command.

    Prefer a repo-root-relative path when possible, otherwise fall back to
    an absolute quoted path.
    """
    try:
        rel = script_path.relative_to(repo_root)
        return f"py {rel.as_posix()} --fix-bom"
    except ValueError:
        return f'py "{script_path}" --fix-bom'


def placeholders(s: str) -> List[str]:
    """
    Extract placeholder numbers from text.

    Example:
        "Value {0} of {1}" -> ["0", "1"]

    Escaped double braces {{ }} are ignored.
    """
    s2 = s.replace("{{", "").replace("}}", "")
    return re.findall(r"\{(\d+)\}", s2)


def _prev_nonspace_same_line(s: str, i: int) -> str:
    """Previous non-space char on the same line, or '' if none."""
    j = i - 1
    while j >= 0 and s[j] in " \t":
        j -= 1
    if j < 0 or s[j] in "\r\n":
        return ""
    return s[j]


def _next_nonspace_same_line(s: str, i: int) -> str:
    """Next non-space char on the same line, or '' if none."""
    j = i + 1
    while j < len(s) and s[j] in " \t":
        j += 1
    if j >= len(s) or s[j] in "\r\n":
        return ""
    return s[j]


def count_markup_angle_brackets(s: str) -> Tuple[int, int]:
    """
    Count '<' and '>' intended as CS2 markup markers.

    Valid examples:
      <Ctrl+V>
      <Contour>
      <anything here>

    Ignore only true numeric comparators where BOTH sides are digits:
      1 < 2
      10 > 3

    This avoids false positives for lines like:
      4. <RMB cycles>
    """
    lt = 0
    gt = 0

    for i, ch in enumerate(s):
        if ch not in "<>":
            continue

        left = _prev_nonspace_same_line(s, i)
        right = _next_nonspace_same_line(s, i)

        left_digit = left.isdigit() if left else False
        right_digit = right.isdigit() if right else False

        if left_digit and right_digit:
            continue

        if ch == "<":
            lt += 1
        else:
            gt += 1

    return lt, gt


def marker_issues(s: str) -> List[str]:
    """
    Check a text value for simple marker balance issues.

    Checks:
    - ** pairs
    - < >
    - { }

    Escaped double braces {{ }} are ignored for brace counting.
    """
    issues: List[str] = []

    bold = s.count("**")
    if bold % 2 != 0:
        issues.append(f'unbalanced "**" (count={bold})')

    lt, gt = count_markup_angle_brackets(s)
    if lt != gt:
        issues.append(f'unbalanced "<" vs ">" (lt={lt}, gt={gt})')

    s2 = s.replace("{{", "").replace("}}", "")
    opens = s2.count("{")
    closes = s2.count("}")
    if opens != closes:
        issues.append(f'unbalanced "{{" vs "}}" (opens={opens}, closes={closes})')

    return issues


def _read_text_and_bom(path: Path) -> Tuple[str, bool]:
    """
    Read file as UTF-8, tolerating an optional BOM.

    Returns:
      - decoded text
      - whether a UTF-8 BOM was present
    """
    raw = path.read_bytes()
    has_bom = raw.startswith(UTF8_BOM)

    try:
        text = raw.decode("utf-8-sig")
    except UnicodeDecodeError as ex:
        raise RuntimeError(f"File is not valid UTF-8: {ex}") from ex

    return text, has_bom


def _strip_bom_in_place(path: Path, decoded_text: str) -> None:
    """
    Rewrite file as UTF-8 without BOM.

    This preserves the decoded text content and removes only the BOM.
    """
    path.write_bytes(decoded_text.encode("utf-8"))


def load_json_locale(path: Path, fix_bom: bool) -> Tuple[Dict[str, str], Dict[str, str], bool, bool]:
    """
    Load one JSON translation file.

    Returns:
      - values: key -> string value
      - pretty: key -> key (used for readable reporting)
      - has_bom: True if file started with UTF-8 BOM
      - bom_fixed: True if BOM was removed during this run

    Notes:
    - Top-level JSON must be an object/dictionary.
    - All values must be strings or null.
    """
    text, has_bom = _read_text_and_bom(path)
    bom_fixed = False

    if has_bom and fix_bom:
        _strip_bom_in_place(path, text)
        bom_fixed = True

    try:
        data = json.loads(text)
    except json.JSONDecodeError as ex:
        raise RuntimeError(
            f"Invalid JSON at line {ex.lineno}, column {ex.colno}: {ex.msg}"
        ) from ex

    if not isinstance(data, dict):
        raise RuntimeError("Top-level JSON must be an object/dictionary")

    values: Dict[str, str] = {}
    pretty: Dict[str, str] = {}

    for k, v in data.items():
        key = str(k)

        if v is None:
            val = ""
        elif isinstance(v, str):
            val = v
        else:
            raise RuntimeError(
                f'Key "{key}" must map to a string or null, not {type(v).__name__}'
            )

        values[key] = val
        pretty[key] = key

    return values, pretty, has_bom, bom_fixed


def _print_problem_report(
    filename: str,
    key_count: int,
    missing: List[str],
    extra: List[str],
    marker_warn: Dict[str, List[str]],
    placeholder_warn: Dict[str, List[str]],
    file_notes: List[str],
    pretty: Dict[str, str],
    base_pretty: Dict[str, str],
) -> None:
    """Print one file report."""
    print("\n" + "=" * 70)
    print(filename)
    print(
        f"Keys: {key_count} | "
        f"Missing vs baseline: {len(missing)} | "
        f"Extra vs baseline: {len(extra)}"
    )
    print(
        f"Marker warnings: {len(marker_warn)} | "
        f"Placeholder warnings: {len(placeholder_warn)} | "
        f"File notes: {len(file_notes)}"
    )

    if file_notes:
        print("-- File notes:")
        for note in file_notes:
            print(f"   {note}")

    if missing:
        print("-- Missing keys:")
        for k in missing:
            print(f"   {base_pretty.get(k, k)}")

    if extra:
        print("-- Extra keys:")
        for k in extra:
            print(f"   {pretty.get(k, k)}")

    def show(title: str, d: Dict[str, List[str]]) -> None:
        if not d:
            return

        print(f"-- {title}:")
        shown = 0
        for k in sorted(d.keys()):
            label = pretty.get(k, k)
            for msg in d[k]:
                print(f"   {label}: {msg}")
                shown += 1
                if shown >= 30:
                    print("   ... (more omitted)")
                    return

    show("Marker issues", marker_warn)
    show("Placeholder issues", placeholder_warn)


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument(
        "--lang-dir",
        default="auto",
        help='JSON lang directory relative to repo root, or "auto" (default: auto)',
    )
    ap.add_argument(
        "--baseline",
        default="en-US.json",
        help="Baseline JSON file name (default: en-US.json)",
    )
    ap.add_argument(
        "--pattern",
        default="*.json",
        help="Glob pattern inside lang-dir (default: *.json)",
    )
    ap.add_argument(
        "--verbose",
        action="store_true",
        help="Print every file result even if clean",
    )
    ap.add_argument(
        "--fix-bom",
        action="store_true",
        help="Remove UTF-8 BOM from JSON files instead of only warning about it",
    )
    args = ap.parse_args()

    repo_root = find_repo_root(Path(__file__).resolve())
    if repo_root is None:
        print("ERROR: Repo root not found.")
        print("Expected one of these markers somewhere above the script:")
        print("  - .git")
        print("  - .gitignore")
        print("  - README.md")
        return 2

    script_path = Path(__file__).resolve()
    fix_bom_cmd = build_fix_bom_command(script_path, repo_root)

    lang_dir = resolve_lang_dir(repo_root, args.baseline, args.lang_dir)
    if lang_dir is None or not lang_dir.exists():
        print("ERROR: JSON lang directory not found.")
        print("Tried common locations:")
        for c in _common_lang_dir_candidates(repo_root):
            print(f"  - {c}")
        print("Also tried recursive search for the baseline file.")
        print('Override with: --lang-dir "src/lang" (or the correct folder).')
        return 2

    base_path = lang_dir / args.baseline
    if not base_path.exists():
        print(f"ERROR: Baseline not found: {base_path}")
        return 2

    try:
        # Do not auto-fix during baseline preload.
        # Fix/report happens later in the normal file loop.
        base_map, base_pretty, _base_has_bom, _base_bom_fixed = load_json_locale(
            base_path,
            fix_bom=False,
        )
    except Exception as ex:
        print(f"ERROR: Failed to parse baseline JSON: {base_path}")
        print(f"Reason: {ex}")
        return 2

    base_keys = set(base_map.keys())

    print(f"Repo root: {repo_root}")
    print(f"JSON lang dir: {lang_dir}")
    print(f"Baseline: {args.baseline}")
    print(f"Baseline keys: {len(base_keys)}")
    print(f"BOM mode: {'fix' if args.fix_bom else 'warn only'}")

    files = sorted(lang_dir.glob(args.pattern))
    if not files:
        print(f"ERROR: No files match {lang_dir / args.pattern}")
        return 2

    any_problem = False
    any_fix = False

    for p in files:
        try:
            m, pretty, has_bom, bom_fixed = load_json_locale(p, fix_bom=args.fix_bom)
        except Exception as ex:
            any_problem = True
            print("\n" + "=" * 60)
            print(p.name)
            print(f"ERROR parsing JSON locale: {ex}")
            continue

        missing = sorted(base_keys - set(m.keys()))
        extra = sorted(set(m.keys()) - base_keys)

        marker_warn: Dict[str, List[str]] = defaultdict(list)
        placeholder_warn: Dict[str, List[str]] = defaultdict(list)
        file_notes: List[str] = []


        if has_bom and not args.fix_bom:
            file_notes.append("UTF-8 BOM detected")
            file_notes.append("To remove BOM now, run:")
            file_notes.append(f"  {fix_bom_cmd}")
        elif bom_fixed:
            file_notes.append("UTF-8 BOM removed; rewritten as UTF-8 without BOM")
            any_fix = True

        for k_norm, val in m.items():
            if val:
                for iss in marker_issues(val):
                    marker_warn[k_norm].append(iss)

            base_val = base_map.get(k_norm, "")
            if base_val and val:
                pb = placeholders(base_val)
                ph = placeholders(val)
                if pb != ph:
                    placeholder_warn[k_norm].append(
                        f"placeholders differ: baseline={pb} locale={ph}"
                    )

        unresolved_problem = bool(
            missing
            or extra
            or marker_warn
            or placeholder_warn
            or (has_bom and not args.fix_bom)
        )

        if unresolved_problem:
            any_problem = True

        should_print = args.verbose or unresolved_problem or bom_fixed
        if not should_print:
            continue

        _print_problem_report(
            filename=p.name,
            key_count=len(m),
            missing=missing,
            extra=extra,
            marker_warn=marker_warn,
            placeholder_warn=placeholder_warn,
            file_notes=file_notes,
            pretty=pretty,
            base_pretty=base_pretty,
        )

    if not any_problem and not args.verbose:
        if any_fix:
            print("\nAll checks GOOD - BOM removed where needed.")
        else:
            print("\nAll checks GOOD - no problems detected.")

    return 1 if any_problem else 0


if __name__ == "__main__":
    raise SystemExit(main())
