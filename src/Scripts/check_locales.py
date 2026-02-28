# File: tools/check_locales.py
# Purpose: Generic checker for C# Locale*.cs dictionary files:
# - Duplicate keys (runtime crash risk)
# - Missing/extra keys vs baseline (default: LocaleEN.cs)
# - Unbalanced markers in VALUES: **, < >, { }
# - Placeholder mismatch vs baseline for each key: {0}, {1}, ...
#
# Output behavior:
# - Default: print only locales with problems
# - If no problems anywhere: print "All checks GOOD - no problems detected."
# - Use --verbose to print every locale report
#
# Exit codes:
# - 0 = no problems
# - 1 = problems found (duplicates/missing/extra/marker/placeholder/parse errors)

import argparse
import re
from collections import Counter, defaultdict
from pathlib import Path
from typing import Dict, List, Optional, Tuple

RE_DICT_START = re.compile(
    r"return\s+new\s+Dictionary<string,\s*string>\s*\{",
    re.IGNORECASE,
)


def norm_ws(s: str) -> str:
    return re.sub(r"\s+", "", s)


def strip_comments_preserve_strings(text: str) -> str:
    """
    Remove // and /* */ comments without touching contents inside strings.
    Supports normal "..." and verbatim @"..." strings.
    """
    out: List[str] = []
    i = 0
    n = len(text)
    in_str = False
    in_verbatim = False

    while i < n:
        ch = text[i]

        if not in_str:
            # Start string?
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

            # Line comment //
            if ch == "/" and i + 1 < n and text[i + 1] == "/":
                i += 2
                while i < n and text[i] not in "\r\n":
                    i += 1
                continue

            # Block comment /* ... */
            if ch == "/" and i + 1 < n and text[i + 1] == "*":
                i += 2
                while i + 1 < n and not (text[i] == "*" and text[i + 1] == "/"):
                    i += 1
                i += 2 if i + 1 < n else 1
                continue

            out.append(ch)
            i += 1
            continue

        # Inside string
        if in_verbatim:
            # Verbatim string escapes quotes by doubling: ""
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

        # Normal string ends at unescaped "
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
    Return the contents inside the outer dictionary initializer braces { ... }
    for: return new Dictionary<string, string> { ... };
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

        # In string
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
    Extract top-level entry blobs inside the dictionary block:
    each entry looks like { keyExpr, valueExpr }
    Returns the inside of those braces (without the outer braces).
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
    Split "keyExpr, valueExpr" on the first comma at top level
    (ignores commas inside parentheses and strings).
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
    token = token.strip()
    if token.startswith('@"') and token.endswith('"'):
        body = token[2:-1]
        return body.replace('""', '"')
    if token.startswith('"') and token.endswith('"'):
        body = token[1:-1]
        body = body.replace(r"\\", "\\").replace(r"\"", '"')
        body = body.replace(r"\n", "\n").replace(r"\r", "\r").replace(r"\t", "\t")
        return body
    return ""


def extract_string_literals(expr: str) -> str:
    """
    Concatenate all string literals found in expr (handles "..." and @"...").
    If expr includes no literals, returns "" (only validates markers on literal text).
    """
    parts: List[str] = []
    i = 0
    n = len(expr)

    while i < n:
        ch = expr[i]

        # verbatim @"..."
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

        # normal "..."
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
    s2 = s.replace("{{", "").replace("}}", "")
    return re.findall(r"\{(\d+)\}", s2)


def _is_numberish(ch: str) -> bool:
    return ch.isdigit() or ch == "."


def count_markup_angle_brackets(s: str) -> Tuple[int, int]:
    """
    Count '<' and '>' intended as CS2 markup markers, ignoring numeric comparators like:
      - < 0.75
      - 1 < 2
      - 1.25 < some words
      - speed > 1.0
    Rule: if a '<' or '>' has a number immediately to its left or right (ignoring whitespace),
    treat it as a comparator and ignore it for markup balancing checks.
    """
    lt = 0
    gt = 0
    i = 0
    n = len(s)

    while i < n:
        ch = s[i]
        if ch == "<" or ch == ">":
            # look left for number-ish
            j = i - 1
            while j >= 0 and s[j].isspace():
                j -= 1
            left_num = j >= 0 and _is_numberish(s[j])

            # look right for number-ish
            k = i + 1
            while k < n and s[k].isspace():
                k += 1
            right_num = k < n and _is_numberish(s[k])

            if left_num or right_num:
                i += 1
                continue

            if ch == "<":
                lt += 1
            else:
                gt += 1

        i += 1

    return lt, gt


def marker_issues(s: str) -> List[str]:
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
    Returns:
      map_norm_key -> value_text (concatenated literals)
      raw_norm_keys list (for duplicates)
      map_norm_key -> pretty_key (first seen original expr)
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
        # last write wins; duplicates are separately reported
        values[k_norm] = extract_string_literals(val_expr)

    return values, keys_raw, pretty


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
    print("\n" + "=" * 70)
    print(filename)
    print(f"Keys: {key_count} | Duplicates: {len(dup)} | Missing vs baseline: {len(missing)} | Extra vs baseline: {len(extra)}")
    print(f"Marker warnings: {len(marker_warn)} | Placeholder warnings: {len(placeholder_warn)}")

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
    ap.add_argument("--loc-dir", default="Localization", help="Localization directory (default: Localization)")
    ap.add_argument("--baseline", default="LocaleEN.cs", help="Baseline file inside loc-dir (default: LocaleEN.cs)")
    ap.add_argument("--pattern", default="Locale*.cs", help="Glob pattern inside loc-dir (default: Locale*.cs)")
    ap.add_argument("--verbose", action="store_true", help="Print every locale result even if clean")
    args = ap.parse_args()

    repo_root = Path(__file__).resolve().parents[1]
    loc_dir = repo_root / args.loc_dir

    if not loc_dir.exists():
        print(f"ERROR: Localization dir not found: {loc_dir}")
        return 2

    base_path = loc_dir / args.baseline
    if not base_path.exists():
        print(f"ERROR: Baseline not found: {base_path}")
        return 2

    base_map, base_keys_raw, base_pretty = load_locale(base_path)
    base_keys = set(base_map.keys())

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
            if val:
                for iss in marker_issues(val):
                    marker_warn[k_norm].append(iss)

            base_val = base_map.get(k_norm, "")
            if base_val and val:
                pb = placeholders(base_val)
                ph = placeholders(val)
                if pb != ph:
                    placeholder_warn[k_norm].append(f"placeholders differ: baseline={pb} locale={ph}")

        has_problem = bool(dup or missing or extra or marker_warn or placeholder_warn)
        if has_problem:
            any_problem = True

        # Quiet mode: skip printing clean files unless --verbose
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
