$outputFile = "structure.txt"
$excludedDirs = @("bin", "obj", ".git", "venv")
$excludedFiles = @("generate_structure.bat", "generate_structure.ps1", "structure.txt")

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

Set-Content -Path $outputFile -Value "Clinic/"
Show-Tree -Path "." -Prefix ""
