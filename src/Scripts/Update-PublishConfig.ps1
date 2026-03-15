# File: Update-PublishConfig.ps1
# Version: 0.5.0
# Purpose:
#   - Sync <ModVersion Value="..."/> in PublishConfiguration.xml to csproj <Version>.
#   - Enforce consistent line endings (CRLF or LF) to prevent VS "MIXED" + popup.
#   - Optional: left-align inner text of <LongDescription> and <ChangeLog> only.
#   - Also update any mod.json found recursively under detected repo root:
#       "version": "x.y.z" -> match csproj <Version>
#     If mod.json is not found, no changes and no failure.
# Notes:
#   - Repo root is auto-detected by walking upward from the script folder.
#   - Markers used to detect repo root:
#       .git, .gitignore, README.md / readme.md
#   - If no marker is found, fallback is the parent of the Scripts folder.

param(
  # Full path to PublishConfiguration.xml
  [Parameter(Mandatory = $true)][string]$Path,

  # Version string from csproj <Version>
  [Parameter(Mandatory = $true)][string]$Version,

  # Enforced line ending style for PublishConfiguration.xml
  [ValidateSet('crlf','lf')][string]$Eol = 'crlf',

  # Enforced line ending style for mod.json files
  [ValidateSet('crlf','lf')][string]$ModJsonEol = 'lf',

  # Optional flag: strip leading spaces/tabs inside LongDescription + ChangeLog only
  [switch]$LeftAlignBlocks,

  # Optional override if auto repo-root detection ever guesses wrong
  [string]$RepoRoot
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# UTF-8 without BOM for all writes.
$utf8NoBom = New-Object System.Text.UTF8Encoding($false)

# --------------------------
# Basic guards
# --------------------------

if (-not (Test-Path -LiteralPath $Path)) {
  throw "PublishConfiguration.xml not found: $Path"
}

# --------------------------
# Helpers
# --------------------------

function Normalize-Eol([string]$s, [string]$eolKind) {
  # First collapse every newline flavor to LF.
  $s = $s.Replace("`r`n", "`n").Replace("`r", "`n")
  $s = $s.Replace([char]0x85,   "`n")  # NEL
  $s = $s.Replace([char]0x2028, "`n")  # LS
  $s = $s.Replace([char]0x2029, "`n")  # PS

  # Then expand to CRLF if requested.
  if ($eolKind -eq 'crlf') {
    $s = $s.Replace("`n", "`r`n")
  }

  return $s
}

function LeftAlignInnerBlock([string]$s, [string]$tagName) {
  # Only touches text inside <tagName>...</tagName>, not the tags themselves.
  $opts = [System.Text.RegularExpressions.RegexOptions]::IgnoreCase -bor
          [System.Text.RegularExpressions.RegexOptions]::Singleline

  $rx = [System.Text.RegularExpressions.Regex]::new(
    "(<$tagName\b[^>]*>)(.*?)(</$tagName>)",
    $opts
  )

  if (-not $rx.IsMatch($s)) { return $s }

  return $rx.Replace($s, { param($m)
    $openTag   = $m.Groups[1].Value
    $innerText = $m.Groups[2].Value
    $closeTag  = $m.Groups[3].Value

    # Remove only leading spaces/tabs from each line inside the block.
    $inner2 = [System.Text.RegularExpressions.Regex]::Replace($innerText, '(?m)^[\t ]+', '')

    $openTag + $inner2 + $closeTag
  }, 1)
}

function Strip-LeadingBomChar([string]$text) {
  # If the file decoded with a leading BOM char, strip it before further work.
  if ($text.Length -gt 0 -and $text[0] -eq [char]0xFEFF) {
    return $text.Substring(1)
  }
  return $text
}

function Test-RepoMarker([string]$dir) {
  # These markers are good enough for the repos being used here.
  if (Test-Path -LiteralPath (Join-Path $dir '.git'))       { return $true }
  if (Test-Path -LiteralPath (Join-Path $dir '.gitignore')) { return $true }
  if (Test-Path -LiteralPath (Join-Path $dir 'README.md'))  { return $true }
  if (Test-Path -LiteralPath (Join-Path $dir 'readme.md'))  { return $true }

  return $false
}

function Resolve-RepoRoot([string]$startDir, [string]$explicitRoot) {
  # Allow manual override first.
  if (-not [string]::IsNullOrWhiteSpace($explicitRoot)) {
    if (-not (Test-Path -LiteralPath $explicitRoot)) {
      throw "RepoRoot override does not exist: $explicitRoot"
    }
    return (Resolve-Path -LiteralPath $explicitRoot).Path
  }

  # Walk upward from the script folder until a repo marker is found.
  $current = Get-Item -LiteralPath $startDir

  while ($null -ne $current) {
    if (Test-RepoMarker $current.FullName) {
      return $current.FullName
    }
    $current = $current.Parent
  }

  # Fallback: parent of Scripts folder.
  # This keeps behavior sane even if markers are missing.
  return (Split-Path -Parent $startDir)
}

function Get-DisplayRelativePath([string]$basePath, [string]$fullPath) {
  # Make console output shorter and easier to read.
  $baseNorm = [System.IO.Path]::GetFullPath($basePath).TrimEnd('\','/')
  $fullNorm = [System.IO.Path]::GetFullPath($fullPath)

  if ($fullNorm.StartsWith($baseNorm, [System.StringComparison]::OrdinalIgnoreCase)) {
    return $fullNorm.Substring($baseNorm.Length).TrimStart('\','/')
  }

  return $fullNorm
}

function Should-SkipModJsonPath([string]$fullPath) {
  # Skip obvious build/output/dependency folders.
  $p = $fullPath.ToLowerInvariant()

  return (
    ($p -match '[\\/]\.git([\\/]|$)')         -or
    ($p -match '[\\/]node_modules([\\/]|$)')  -or
    ($p -match '[\\/]bin([\\/]|$)')           -or
    ($p -match '[\\/]obj([\\/]|$)')           -or
    ($p -match '[\\/]dist([\\/]|$)')          -or
    ($p -match '[\\/]build([\\/]|$)')         -or
    ($p -match '[\\/]out([\\/]|$)')
  )
}

function Update-ModJsonVersions(
  [string]$searchRoot,
  [string]$versionValue,
  [string]$jsonEol,
  [System.Text.UTF8Encoding]$utf8Encoding
) {
  # Look for every mod.json under repo root, regardless of folder name.
  $files = @(
    Get-ChildItem -LiteralPath $searchRoot -Recurse -Force -File -Filter 'mod.json' -ErrorAction SilentlyContinue |
      Where-Object { -not (Should-SkipModJsonPath $_.FullName) }
  )

  $found = $files.Count

  if ($found -eq 0) {
    Write-Host "mod.json: none found under repo root (skip)."
    return [pscustomobject]@{
      Found = 0
      Updated = 0
      SkippedNoVersionKey = 0
    }
  }

  Write-Host ("mod.json: found {0} file(s) under repo root." -f $found)

  # Replace only the first "version": "..." occurrence.
  $rxVersion = [System.Text.RegularExpressions.Regex]::new(
    '(?m)(?<prefix>"version"\s*:\s*")(?<val>[^"]*)(?<suffix>")',
    [System.Text.RegularExpressions.RegexOptions]::IgnoreCase
  )

  $changedCount = 0
  $skippedNoKey = 0

  foreach ($f in $files) {
    $jsonOriginal = [System.IO.File]::ReadAllText($f.FullName, $utf8Encoding)
    $jsonOriginal = Strip-LeadingBomChar $jsonOriginal

    if (-not $rxVersion.IsMatch($jsonOriginal)) {
      $relSkip = Get-DisplayRelativePath $searchRoot $f.FullName
      Write-Host ("mod.json: skipped (no ""version"" key): {0}" -f $relSkip)
      $skippedNoKey++
      continue
    }

    $jsonUpdated = $rxVersion.Replace($jsonOriginal, { param($m)
      $m.Groups['prefix'].Value + $versionValue + $m.Groups['suffix'].Value
    }, 1)

    # Keep JSON line endings consistent.
    $jsonUpdated = Normalize-Eol $jsonUpdated $jsonEol

    if ($jsonUpdated -eq $jsonOriginal) {
      $relNoChange = Get-DisplayRelativePath $searchRoot $f.FullName
      Write-Host ("mod.json: already {0}: {1}" -f $versionValue, $relNoChange)
      continue
    }

    $tmp = ($f.FullName + ".tmp")
    [System.IO.File]::WriteAllText($tmp, $jsonUpdated, $utf8Encoding)
    Move-Item -Force -LiteralPath $tmp -Destination $f.FullName

    $rel = Get-DisplayRelativePath $searchRoot $f.FullName
    Write-Host ("mod.json updated to [{0}]: {1}" -f $versionValue, $rel)
    $changedCount++
  }

  return [pscustomobject]@{
    Found = $found
    Updated = $changedCount
    SkippedNoVersionKey = $skippedNoKey
  }
}

# -----------------------------------------------------
# Step 1: read and validate PublishConfiguration.xml
# -----------------------------------------------------

$original = [System.IO.File]::ReadAllText($Path, $utf8NoBom)
$original = Strip-LeadingBomChar $original

if ([string]::IsNullOrWhiteSpace($original)) {
  throw "Refusing to update because file is empty: $Path (restore it first)"
}

# --------------------------------------------------------------
# Step 2: normalize, format, update PublishConfiguration.xml
# --------------------------------------------------------------

# Normalize first so edits start from a clean EOL state.
$text = Normalize-Eol $original $Eol

# Optional formatting fix for Paradox markdown quirks.
if ($LeftAlignBlocks) {
  $text = LeftAlignInnerBlock $text 'LongDescription'
  $text = LeftAlignInnerBlock $text 'ChangeLog'
}

# Replace only the Value="..." part of <ModVersion ...>.
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

# Final normalize so output is guaranteed clean.
$text = Normalize-Eol $text $Eol

# Safety check: if CRLF requested, refuse to write a mixed file.
if ($Eol -eq 'crlf') {
  if ($text -match "(?<!`r)`n") { throw "Internal error: bare LF found (would create MIXED): $Path" }
  if ($text -match "`r(?!`n)")  { throw "Internal error: bare CR found (would create MIXED): $Path" }
}

$publishChanged = ($text -ne $original)

# -----------------------------------------------------------
# Step 3: write PublishConfiguration.xml only if changed
# -----------------------------------------------------------

if ($publishChanged) {
  $bak = "$Path.bak"
  [System.IO.File]::WriteAllText($bak, $original, $utf8NoBom)

  $tmp = "$Path.tmp"
  [System.IO.File]::WriteAllText($tmp, $text, $utf8NoBom)
  Move-Item -Force -LiteralPath $tmp -Destination $Path

  Write-Host ("PublishConfiguration.xml updated: ModVersion=[{0}] EOL=[{1}] LeftAlignBlocks=[{2}] BACKUP=[{3}]" -f `
    $Version, $Eol, $LeftAlignBlocks.IsPresent, (Split-Path -Leaf $bak))
} else {
  Write-Host ("No PublishConfiguration.xml change needed (ModVersion already [{0}] and formatting already clean)." -f $Version)
}

# ---------------------------------------------------------
# Step 4: find repo root and update any mod.json
# ---------------------------------------------------------

$detectedRepoRoot = Resolve-RepoRoot $PSScriptRoot $RepoRoot
Write-Host ("Repo root for mod.json search: {0}" -f $detectedRepoRoot)

$modJsonResult = Update-ModJsonVersions `
  -searchRoot $detectedRepoRoot `
  -versionValue $Version `
  -jsonEol $ModJsonEol `
  -utf8Encoding $utf8NoBom

# ----------------------------------------------------------------------
# Step 5: summary
# ----------------------------------------------------------------------

Write-Host ("Done. PublishConfigChanged={0}, ModJsonFound={1}, ModJsonUpdated={2}, ModJsonSkippedNoVersionKey={3}." -f `
  $publishChanged, $modJsonResult.Found, $modJsonResult.Updated, $modJsonResult.SkippedNoVersionKey)

exit 0
