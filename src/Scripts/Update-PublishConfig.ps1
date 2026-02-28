# File: tools/Update-PublishConfig.ps1
# Purpose:
#   - Sync <ModVersion Value="..."/> in PublishConfiguration.xml to csproj <Version>.
#   - Enforce consistent line endings (CRLF or LF) to prevent VS "MIXED" + popup.
#   - Optional: left-align inner text of <LongDescription> and <ChangeLog> only.

param(
  # Full path to PublishConfiguration.xml
  [Parameter(Mandatory = $true)][string]$Path,

  # Version string from csproj <Version>
  [Parameter(Mandatory = $true)][string]$Version,

  # Enforced line ending style == PICK ONE ==
  [ValidateSet('crlf','lf')][string]$Eol = 'lf',

  # Optional flag: if present, strip leading spaces/tabs *inside* LongDescription + ChangeLog blocks only
  [switch]$LeftAlignBlocks
)

# Fail fast + loud rather than silently
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# Guard: file must exist
if (-not (Test-Path -LiteralPath $Path)) {
  throw "PublishConfiguration.xml not found: $Path"
}

# Read using UTF-8 without BOM (keeps output consistent)
$utf8NoBom = New-Object System.Text.UTF8Encoding($false)
$original  = [System.IO.File]::ReadAllText($Path, $utf8NoBom)

# Guard: refuse to touch an empty file (prevents catastrophic wipes)
if ([string]::IsNullOrWhiteSpace($original)) {
  throw "Refusing to update because file is empty: $Path (restore it first)"
}

function Normalize-Eol([string]$s, [string]$eolKind) {
  # Collapse ALL newline variants to LF (cleans MIXED file reliably)
  $s = $s.Replace("`r`n", "`n").Replace("`r", "`n")
  $s = $s.Replace([char]0x85,  "`n")   # NEL
  $s = $s.Replace([char]0x2028, "`n")  # LS
  $s = $s.Replace([char]0x2029, "`n")  # PS

  # Then convert to CRLF if requested at the top.
  if ($eolKind -eq 'crlf') {
    $s = $s.Replace("`n", "`r`n")
  }
  return $s
}

function LeftAlignInnerBlock([string]$s, [string]$tagName) {
  # Only affects text *inside* <tagName>...</tagName>, not the tags themselves.
  $opts = [System.Text.RegularExpressions.RegexOptions]::IgnoreCase -bor
          [System.Text.RegularExpressions.RegexOptions]::Singleline

  # Build a regex that captures: open tag (with attributes), inner text, and the close tag separately.
  $rx = [System.Text.RegularExpressions.Regex]::new(
    "(<$tagName\b[^>]*>)(.*?)(</$tagName>)",
    $opts
  )

  # No tag match, return early.
  if (-not $rx.IsMatch($s)) { return $s }

  # Replace the matching inner text block with a scriptblock that strips indents + keeps blank lines.
  return $rx.Replace($s, { param($m)
    $openTag   = $m.Groups[1].Value
    $innerText = $m.Groups[2].Value
    $closeTag  = $m.Groups[3].Value

    # Strip indents at the start of each line; blank lines remain blank.
    $inner2 = [System.Text.RegularExpressions.Regex]::Replace($innerText, '(?m)^[\t ]+', '')

    $openTag + $inner2 + $closeTag
  }, 1)
}

# Work on a normalized copy first (removes existing MIXED now)
$text = Normalize-Eol $original $Eol

# Optional formatting fix for Paradox markdown quirks
if ($LeftAlignBlocks) {
  $text = LeftAlignInnerBlock $text 'LongDescription'
  $text = LeftAlignInnerBlock $text 'ChangeLog'
}

# Replace ONLY the Value="..." part of ModVersion (works for empty/space/weird values).
# Anchored to line-start so it won't accidentally match a commented-out ModVersion.
$rxMod = [System.Text.RegularExpressions.Regex]::new(
  '(?m)^(?<prefix>[\t ]*<ModVersion\b[^>]*\bValue=")[^"]*(?<suffix>")',
  [System.Text.RegularExpressions.RegexOptions]::IgnoreCase
)

if ($rxMod.IsMatch($text)) {
  $text = $rxMod.Replace($text, { param($m)
    $m.Groups['prefix'].Value + $Version + $m.Groups['suffix'].Value
  }, 1)
} else {
  throw "Could not find <ModVersion ... Value=""..."" ...> in: $Path"
}

# Final EOL normalization AFTER edits (so output is guaranteed clean)
$text = Normalize-Eol $text $Eol

# Sanity checks: if CRLF requested, refuse to write if any bare LF or bare CR exists
if ($Eol -eq 'crlf') {
  if ($text -match "(?<!`r)`n") { throw "Internal error: bare LF found (would create MIXED): $Path" }
  if ($text -match "`r(?!`n)")  { throw "Internal error: bare CR found (would create MIXED): $Path" }
}

# If nothing changed, exit without touching the file
if ($text -eq $original) {
  Write-Host ("No change needed: {0} (ModVersion already {1})." -f (Split-Path -Leaf $Path), $Version)
  exit 0
}

# Backup + "atomic-ish" replace:
# - Save original to .bak => Write new content to .tmp
# - Move .tmp to replace original in one step
$bak = "$Path.bak"
[System.IO.File]::WriteAllText($bak, $original, $utf8NoBom)

$tmp = "$Path.tmp"
[System.IO.File]::WriteAllText($tmp, $text, $utf8NoBom)
Move-Item -Force -LiteralPath $tmp -Destination $Path

# Short backup path: Properties\PublishConfiguration.xml.bak
$repoRoot = Split-Path -Parent $PSScriptRoot   # parent of tools/ = repo/project root
$bakShort = $bak

if ($bak.StartsWith($repoRoot, [System.StringComparison]::OrdinalIgnoreCase)) {
  $bakShort = $bak.Substring($repoRoot.Length).TrimStart('\','/')
}

# Output summary
Write-Host ("ModVersion updated to [{0}] in: {1} (EOL={2}, LeftAlignBlocks={3}).`nBACKUP in: {4}" -f `
  $Version, (Split-Path -Leaf $Path), $Eol, $LeftAlignBlocks.IsPresent, $bakShort)
