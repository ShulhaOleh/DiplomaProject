# Generates the project structure tree, writes it to structure.txt,
# and updates the "Project structure" section in readme.md and readme.uk.md.
#
# Usage:
#   - Double-click generate_structure.bat, or
#   - Right-click this file and "Run with PowerShell", or
#   - Called automatically by the pre-commit git hook

$scriptDir  = $PSScriptRoot
$rootPath   = Split-Path -Parent $scriptDir
$outputFile = Join-Path $scriptDir "structure.txt"
$readmeEn   = Join-Path $rootPath "readme.md"
$readmeUk   = Join-Path $rootPath "readme.uk.md"

$excluded = @("bin", "obj", ".git", ".githooks", ".vs", "venv")

$treeLines = [System.Collections.Generic.List[string]]::new()

function Add-Tree {
    param([string]$Path, [string]$Prefix = "")

    $items = Get-ChildItem -LiteralPath $Path |
             Where-Object { $excluded -notcontains $_.Name }

    $count = $items.Count
    for ($i = 0; $i -lt $count; $i++) {
        $item   = $items[$i]
        $isLast = $i -eq ($count - 1)
        $conn   = if ($isLast) { "└── " } else { "├── " }

        $treeLines.Add("$Prefix$conn$($item.Name)")

        if ($item.PSIsContainer) {
            $childPrefix = if ($isLast) { "$Prefix    " } else { "$Prefix│   " }
            Add-Tree -Path $item.FullName -Prefix $childPrefix
        }
    }
}

$rootFolderName = Split-Path -Leaf $rootPath
$treeLines.Add("$rootFolderName/")
Add-Tree -Path $rootPath

# Write structure.txt
Set-Content -Path $outputFile -Value $treeLines -Encoding UTF8
Write-Host "structure.txt updated"

# Update the Project structure block in a README file.
# Replaces everything between the opening and closing ``` fences
# that follow the section heading.
function Update-ReadmeStructure {
    param([string]$Path, [string[]]$TreeLines)

    if (-not (Test-Path $Path)) {
        Write-Warning "Not found: $Path"
        return
    }

    $lines     = Get-Content -Path $Path -Encoding UTF8
    $result    = [System.Collections.Generic.List[string]]::new()
    $inSection = $false
    $inFence   = $false

    foreach ($line in $lines) {
        # Detect the section heading (EN and UK)
        if ($line -match '^## (Project structure|Структура проєкту)') {
            $inSection = $true
            $result.Add($line)
            continue
        }

        # Opening fence — replace old content with fresh tree
        if ($inSection -and -not $inFence -and $line -match '^```\s*$') {
            $inFence = $true
            $result.Add('```')
            foreach ($treeLine in $TreeLines) {
                $result.Add($treeLine)
            }
            continue
        }

        # Closing fence — stop skipping
        if ($inFence -and $line -match '^```\s*$') {
            $inFence   = $false
            $inSection = $false
            $result.Add('```')
            continue
        }

        # Skip old tree lines while inside the fence
        if ($inFence) { continue }

        $result.Add($line)
    }

    Set-Content -Path $Path -Value $result -Encoding UTF8
    Write-Host "Updated: $(Split-Path -Leaf $Path)"
}

$treeArray = $treeLines.ToArray()
Update-ReadmeStructure -Path $readmeEn -TreeLines $treeArray
Update-ReadmeStructure -Path $readmeUk -TreeLines $treeArray

Write-Host "Done."
