# File: src/Scripts/check_locales.py
# Purpose:
#   Generic checker for C# Locale*.cs dictionary files.
#
#   What it checks:
#   - Duplicate keys (runtime crash risk)
#   - Missing / extra keys vs baseline (default: LocaleEN.cs)
#   - Unbalanced markers in VALUES: **, < >, { }
#   - Placeholder mismatch vs baseline: {0}, {1}, ...
#
#   Notes:
#   - CS2 uses <...> as markup to highlight text in green.
#     Those brackets are intentional and should be counted as valid markup.
#   - The angle-bracket check ignores only true numeric comparators such as:
#       1 < 2
#       10 > 3
#     It does NOT treat numbered instruction lines like:
#       4. <RMB cycles>
#     as comparators.
#
# Output behavior:
#   - Default: print only locales with problems
#   - If no problems anywhere: print "All checks GOOD - no problems detected."
#   - Use --verbose to print every locale report
#
# Exit codes:
#   - 0 = no problems
#   - 1 = problems found
#   - 2 = configuration / filesystem error

import argparse
import re
from collections import Counter, defaultdict
from pathlib import Path
from typing import Dict, List, Optional, Set, Tuple

RE_DICT_START = re.compile(
    r"(?:return\s+)?new\s+Dictionary<string,\s*string>\s*\{",
    re.IGNORECASE,
)

# Common folders to skip during recursive auto-search.
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


def norm_ws(s: str) -> str:
    """Normalize whitespace so equivalent key expressions compare the same."""
    return re.sub(r"\s+", "", s)


def find_repo_root(start: Path) -> Optional[Path]:
    """
    Walk upward to find a likely repo root.

    Preference order:
    1) .git
    2) .gitattributes
    3) .gitignore
    4) README.md / readme.md

    This avoids assuming every repo has a /src folder.
    """
    p = start.resolve()

    # Strongest signal: .git
    for parent in [p] + list(p.parents):
        if (parent / ".git").exists():
            return parent

    # Next-best signals
    for marker in [".gitattributes", ".gitignore", "README.md", "readme.md"]:
        for parent in [p] + list(p.parents):
            if (parent / marker).exists():
                return parent

    return None

def strip_comments_preserve_strings(text: str) -> str:
    """
    Remove // and /* */ comments without touching text inside strings.

    Supports:
    - normal strings: "..."
    - verbatim strings: @"..."
    """
    out: List[str] = []
    i = 0
    n = len(text)
    in_str = False
    in_verbatim = False

    while i < n:
        ch = text[i]

        if not in_str:
            # Start of a string?
            if ch == '"' and (i == 0 or text[i - 1] != "\\"):
                if i > 0 and text[i - 1] == "@":
                    in_str = True
                    in_verbatim = True
                else:
                    in_str = True
                    in_verbatim = False
                out.append(ch)
                i += 1
                continue

            # Line comment: //
            if ch == "/" and i + 1 < n and text[i + 1] == "/":
                i += 2
                while i < n and text[i] not in "\r\n":
                    i += 1
                continue

            # Block comment: /* ... */
            if ch == "/" and i + 1 < n and text[i + 1] == "*":
                i += 2
                while i + 1 < n and not (text[i] == "*" and text[i + 1] == "/"):
                    i += 1
                i += 2 if i + 1 < n else 1
                continue

            out.append(ch)
            i += 1
            continue

        # Inside a string
        if in_verbatim:
            # Verbatim string escape: ""
            if ch == '"' and i + 1 < n and text[i + 1] == '"':
                out.append('""')
                i += 2
                continue
            if ch == '"':
                in_str = False
                in_verbatim = False
                out.append(ch)
                i += 1
                continue
            out.append(ch)
            i += 1
            continue

        # Normal string closes on unescaped "
        if ch == '"' and (i == 0 or text[i - 1] != "\\"):
            in_str = False
            out.append(ch)
            i += 1
            continue

        out.append(ch)
        i += 1

    return "".join(out)


def find_dictionary_block(text: str) -> Optional[str]:
    """
    Return only the contents inside the outer dictionary initializer braces.

    Example target:
        new Dictionary<string, string>
        {
            { key, value },
            ...
        }
    """
    clean = strip_comments_preserve_strings(text)
    m = RE_DICT_START.search(clean)
    if not m:
        return None

    start_brace = clean.find("{", m.end() - 1)
    if start_brace < 0:
        return None

    depth = 1
    in_str = False
    in_verbatim = False
    start = start_brace + 1
    i = start

    while i < len(clean):
        ch = clean[i]

        if not in_str:
            if ch == '"' and (i == 0 or clean[i - 1] != "\\"):
                if i > 0 and clean[i - 1] == "@":
                    in_str = True
                    in_verbatim = True
                else:
                    in_str = True
                    in_verbatim = False
                i += 1
                continue

            if ch == "{":
                depth += 1
            elif ch == "}":
                depth -= 1
                if depth == 0:
                    return clean[start:i]

            i += 1
            continue

        # Inside string
        if in_verbatim:
            if ch == '"' and i + 1 < len(clean) and clean[i + 1] == '"':
                i += 2
                continue
            if ch == '"':
                in_str = False
                in_verbatim = False
                i += 1
                continue
            i += 1
            continue

        if ch == '"' and (i == 0 or clean[i - 1] != "\\"):
            in_str = False
            i += 1
            continue

        i += 1

    return None


def extract_entries(block: str) -> List[str]:
    """
    Extract each top-level entry blob inside the dictionary block.

    Each entry looks like:
        { keyExpr, valueExpr }

    Returns only the inside of the braces:
        keyExpr, valueExpr
    """
    entries: List[str] = []
    i = 0
    n = len(block)
    in_str = False
    in_verbatim = False
    depth = 0
    start = None

    while i < n:
        ch = block[i]

        if not in_str:
            if ch == '"' and (i == 0 or block[i - 1] != "\\"):
                if i > 0 and block[i - 1] == "@":
                    in_str = True
                    in_verbatim = True
                else:
                    in_str = True
                    in_verbatim = False
                i += 1
                continue

            if ch == "{":
                if depth == 0:
                    start = i + 1
                depth += 1
            elif ch == "}":
                depth -= 1
                if depth == 0 and start is not None:
                    entries.append(block[start:i])
                    start = None

            i += 1
            continue

        if in_verbatim:
            if ch == '"' and i + 1 < n and block[i + 1] == '"':
                i += 2
                continue
            if ch == '"':
                in_str = False
                in_verbatim = False
                i += 1
                continue
            i += 1
            continue

        if ch == '"' and (i == 0 or block[i - 1] != "\\"):
            in_str = False
            i += 1
            continue

        i += 1

    return entries


def split_top_level_comma(entry: str) -> Optional[Tuple[str, str]]:
    """
    Split "keyExpr, valueExpr" on the first comma at top level.

    Ignores commas inside:
    - parentheses
    - strings
    """
    s = entry.strip()
    depth_paren = 0
    in_str = False
    in_verbatim = False

    i = 0
    while i < len(s):
        ch = s[i]

        if not in_str:
            if ch == '"' and (i == 0 or s[i - 1] != "\\"):
                if i > 0 and s[i - 1] == "@":
                    in_str = True
                    in_verbatim = True
                else:
                    in_str = True
                    in_verbatim = False
                i += 1
                continue

            if ch == "(":
                depth_paren += 1
            elif ch == ")":
                depth_paren = max(0, depth_paren - 1)
            elif ch == "," and depth_paren == 0:
                return s[:i].strip(), s[i + 1 :].strip()

            i += 1
            continue

        if in_verbatim:
            if ch == '"' and i + 1 < len(s) and s[i + 1] == '"':
                i += 2
                continue
            if ch == '"':
                in_str = False
                in_verbatim = False
                i += 1
                continue
            i += 1
            continue

        if ch == '"' and (i == 0 or s[i - 1] != "\\"):
            in_str = False
            i += 1
            continue

        i += 1

    return None


def decode_csharp_string_literal(token: str) -> str:
    """Decode one C# string literal into plain text."""
    token = token.strip()

    # Verbatim string: @"..."
    if token.startswith('@"') and token.endswith('"'):
        body = token[2:-1]
        return body.replace('""', '"')

    # Normal string: "..."
    if token.startswith('"') and token.endswith('"'):
        body = token[1:-1]
        body = body.replace(r"\\", "\\").replace(r"\"", '"')
        body = body.replace(r"\n", "\n").replace(r"\r", "\r").replace(r"\t", "\t")
        return body

    return ""


def extract_string_literals(expr: str) -> str:
    """
    Concatenate all string literals found in an expression.

    Example:
        "Hello " + "World"
    becomes:
        "Hello World"

    If the expression contains no string literals, returns "".
    """
    parts: List[str] = []
    i = 0
    n = len(expr)

    while i < n:
        ch = expr[i]

        # Verbatim string: @"..."
        if ch == "@" and i + 1 < n and expr[i + 1] == '"':
            j = i + 2
            while j < n:
                if expr[j] == '"' and j + 1 < n and expr[j + 1] == '"':
                    j += 2
                    continue
                if expr[j] == '"':
                    parts.append(decode_csharp_string_literal(expr[i : j + 1]))
                    i = j + 1
                    break
                j += 1
            else:
                break
            continue

        # Normal string: "..."
        if ch == '"':
            j = i + 1
            while j < n:
                if expr[j] == '"' and expr[j - 1] != "\\":
                    parts.append(decode_csharp_string_literal(expr[i : j + 1]))
                    i = j + 1
                    break
                j += 1
            else:
                break
            continue

        i += 1

    return "".join(parts)


def placeholders(s: str) -> List[str]:
    """
    Extract placeholder numbers from text.

    Example:
        "Speed {0} of {1}"
    returns:
        ["0", "1"]

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

    Important:
    - <some words> are valid markup CS2 shows as green highlighted text.
    - Only ignore real numeric comparators where BOTH sides look numeric
      on the same line, such as:
          1 < 2
          10 > 3

    Avoids false positives for numbered instructions like:
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

        # Ignore only true numeric comparators.
        if left_digit and right_digit:
            continue

        if ch == "<":
            lt += 1
        else:
            gt += 1

    return lt, gt


def marker_issues(s: str) -> List[str]:
    """
    Check a text value for simple markup balance issues.

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


def load_locale(path: Path) -> Tuple[Dict[str, str], List[str], Dict[str, str]]:
    """
    Load one locale file.

    Returns:
      - values: map of normalized key -> concatenated literal text
      - keys_raw: list of normalized keys in original order (used for duplicate detection)
      - pretty: map of normalized key -> original key expression for readable reporting
    """
    text = path.read_text(encoding="utf-8")
    block = find_dictionary_block(text)
    if block is None:
        raise RuntimeError("Dictionary initializer not found")

    entries = extract_entries(block)

    values: Dict[str, str] = {}
    keys_raw: List[str] = []
    pretty: Dict[str, str] = {}

    for e in entries:
        split = split_top_level_comma(e)
        if not split:
            continue

        key_expr, val_expr = split
        k_norm = norm_ws(key_expr)

        keys_raw.append(k_norm)

        if k_norm not in pretty:
            pretty[k_norm] = key_expr.strip()

        values[k_norm] = extract_string_literals(val_expr)

    return values, keys_raw, pretty


def _common_loc_dir_candidates(repo_root: Path) -> List[Path]:
    """Common locale folder guesses."""
    return [
        repo_root / "src" / "Localization",
        repo_root / "Localization",
        repo_root / "src" / "Settings",   # legacy
        repo_root / "Settings",           # legacy without /src
    ]


def _score_baseline_parent(parent: Path) -> int:
    """
    Score a folder for how likely it is to be the real locale folder.
    Higher score wins.
    """
    score = 0
    parts_lower = [p.lower() for p in parent.parts]
    name_lower = parent.name.lower()

    if name_lower == "localization":
        score += 100
    elif name_lower == "settings":
        score += 80

    if "src" in parts_lower:
        score += 10

    # Slight preference for shallower paths.
    score -= len(parent.parts)
    return score


def _recursive_find_loc_dir(repo_root: Path, baseline: str) -> Optional[Path]:
    """
    Fallback search: recursively look for the baseline file anywhere under repo_root,
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


def resolve_loc_dir(repo_root: Path, baseline: str, loc_dir_arg: str) -> Optional[Path]:
    """
    Resolve the locale directory.

    Behavior:
    - If --loc-dir was given, use it directly relative to repo root.
    - Otherwise try common locations first.
    - If still not found, do a recursive search for the baseline file.
    """
    if loc_dir_arg.lower() != "auto":
        return repo_root / loc_dir_arg

    for c in _common_loc_dir_candidates(repo_root):
        if c.exists() and (c / baseline).exists():
            return c

    return _recursive_find_loc_dir(repo_root, baseline)


def _print_problem_report(
    filename: str,
    key_count: int,
    dup: List[str],
    missing: List[str],
    extra: List[str],
    marker_warn: Dict[str, List[str]],
    placeholder_warn: Dict[str, List[str]],
    pretty: Dict[str, str],
    base_pretty: Dict[str, str],
) -> None:
    """Print one locale report."""
    print("\n" + "=" * 70)
    print(filename)
    print(
        f"Keys: {key_count} | Duplicates: {len(dup)} | "
        f"Missing vs baseline: {len(missing)} | Extra vs baseline: {len(extra)}"
    )
    print(
        f"Marker warnings: {len(marker_warn)} | "
        f"Placeholder warnings: {len(placeholder_warn)}"
    )

    if dup:
        print("!! DUPLICATE KEYS (runtime crash risk):")
        for k in dup:
            print(f"   {pretty.get(k, k)}")

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
        "--loc-dir",
        default="auto",
        help='Localization directory relative to repo root, or "auto" (default: auto)',
    )
    ap.add_argument(
        "--baseline",
        default="LocaleEN.cs",
        help="Baseline file name (default: LocaleEN.cs)",
    )
    ap.add_argument(
        "--pattern",
        default="Locale*.cs",
        help="Glob pattern inside loc-dir (default: Locale*.cs)",
    )
    ap.add_argument(
        "--verbose",
        action="store_true",
        help="Print every locale result even if clean",
    )
    args = ap.parse_args()

    repo_root = find_repo_root(Path(__file__).resolve())
    if repo_root is None:
        print("ERROR: Repo root not found.")
        print("Expected one of these markers somewhere above the script:")
        print("  - .git")
        print("  - .gitattributes")
        print("  - .gitignore")
        print("  - README.md")
        return 2

    loc_dir = resolve_loc_dir(repo_root, args.baseline, args.loc_dir)
    if loc_dir is None or not loc_dir.exists():
        print("ERROR: Localization dir not found.")
        print("Tried common locations:")
        for c in _common_loc_dir_candidates(repo_root):
            print(f"  - {c}")
        print("Also tried recursive search for the baseline file.")
        print('Override with: --loc-dir "src/Localization" (or the correct folder).')
        return 2

    base_path = loc_dir / args.baseline
    if not base_path.exists():
        print(f"ERROR: Baseline not found: {base_path}")
        return 2

    base_map, _base_keys_raw, base_pretty = load_locale(base_path)
    base_keys = set(base_map.keys())

    print(f"Repo root: {repo_root}")
    print(f"Localization dir: {loc_dir}")
    print(f"Baseline: {args.baseline}")
    print(f"Baseline keys: {len(base_keys)}")

    files = sorted(loc_dir.glob(args.pattern))
    if not files:
        print(f"ERROR: No files match {loc_dir / args.pattern}")
        return 2

    any_problem = False

    for p in files:
        try:
            m, keys_raw, pretty = load_locale(p)
        except Exception as ex:
            any_problem = True
            print("\n" + "=" * 70)
            print(p.name)
            print(f"ERROR parsing locale: {ex}")
            continue

        dup = [k for k, c in Counter(keys_raw).items() if c > 1]
        missing = sorted(base_keys - set(m.keys()))
        extra = sorted(set(m.keys()) - base_keys)

        marker_warn: Dict[str, List[str]] = defaultdict(list)
        placeholder_warn: Dict[str, List[str]] = defaultdict(list)

        for k_norm, val in m.items():
            # Check marker balance only when there is literal text.
            if val:
                for iss in marker_issues(val):
                    marker_warn[k_norm].append(iss)

            # Compare placeholders only when both sides have literal text.
            # This avoids noisy warnings for intentionally blank descriptions.
            base_val = base_map.get(k_norm, "")
            if base_val and val:
                pb = placeholders(base_val)
                ph = placeholders(val)
                if pb != ph:
                    placeholder_warn[k_norm].append(
                        f"placeholders differ: baseline={pb} locale={ph}"
                    )

        has_problem = bool(dup or missing or extra or marker_warn or placeholder_warn)
        if has_problem:
            any_problem = True

        if (not args.verbose) and (not has_problem):
            continue

        _print_problem_report(
            filename=p.name,
            key_count=len(m),
            dup=dup,
            missing=missing,
            extra=extra,
            marker_warn=marker_warn,
            placeholder_warn=placeholder_warn,
            pretty=pretty,
            base_pretty=base_pretty,
        )

    if not any_problem and not args.verbose:
        print("\nAll checks GOOD - no problems detected.")

    return 1 if any_problem else 0


if __name__ == "__main__":
    raise SystemExit(main())
