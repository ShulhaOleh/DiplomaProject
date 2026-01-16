# Right click and press "Run with PowerShell" to create/update structure in structure.txt
# If you want to double-click it every time, use .bat file

$outputFile = "structure.txt"
$excludedDirs = @("bin", "obj", ".git", "venv", ".gitignore", "Clinic.sln")
$excludedFiles = @("structure.txt")

function Show-Tree {
    param (
        [string]$Path,
        [string]$Prefix = ""
    )

    $items = Get-ChildItem -LiteralPath $Path | Where-Object {
        $excludedDirs -notcontains $_.Name -and $excludedFiles -notcontains $_.Name
    }

    $count = $items.Count
    for ($i = 0; $i -lt $count; $i++) {
        $item = $items[$i]
        $isLast = ($i -eq $count - 1)
        $connector = if ($isLast) { "└── " } else { "├── " }

        $line = "$Prefix$connector$item"
        Add-Content -Path $outputFile -Value $line

        if ($item.PSIsContainer) {
            $newPrefix = if ($isLast) { "$Prefix    " } else { "$Prefix│   " }
            Show-Tree -Path $item.FullName -Prefix $newPrefix
        }
    }
}

$rootPath = Split-Path -Parent $PSScriptRoot
$rootFolderName = Split-Path -Leaf $rootPath

Set-Content -Path $outputFile -Value "$rootFolderName/"
Show-Tree -Path $rootPath -Prefix ""

Write-Host "Structure saved in $outputFile"
