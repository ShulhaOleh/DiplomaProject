$rootDir = "../DBs/"

$excludedFiles = @( "00-Init.sql" )

$header = @"
SET NAMES utf8mb4;
SET CHARACTER SET utf8mb4;

"@

Get-ChildItem -Path $rootDir -Filter "*.sql" -File | ForEach-Object {

    $fileName = $_.Name
    $fullPath = $_.FullName

    if ($excludedFiles -contains $fileName) {
        Write-Host "Skipped (excluded): $fileName" -ForegroundColor DarkGray
        return
    }

    $content = Get-Content $fullPath -Raw -Encoding UTF8

    if ($content -match "SET NAMES utf8mb4") {
        Write-Host "Already has encoding: $fileName" -ForegroundColor Yellow
        return
    }

    Set-Content $fullPath ($header + $content) -Encoding UTF8
    Write-Host "Updated: $fileName" -ForegroundColor Green
}

Write-Host "`nDone! Now run: `n`tdocker-compose down -v `n`tdocker-compose up -d" -ForegroundColor Cyan
