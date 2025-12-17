param(
    [string]$RootPath = "."
)

$pattern = Read-Host "Enter text or regex to search for"

Write-Host "Root: $RootPath"
Write-Host "Pattern: $pattern"
Write-Host ""

# Get all files first so we know the total count
$files = Get-ChildItem -Path $RootPath -Recurse -File -ErrorAction SilentlyContinue
$total = $files.Count

if ($total -eq 0) {
    Write-Host "No files found."
    return
}

$matches = @()
$index = 0

foreach ($file in $files) {
    $index++

    $scanned = $index
    $left    = $total - $scanned
    $percent = [int](($scanned / $total) * 100)

    # Live progress in the console title-like bar
    Write-Progress -Activity "Searching files" `
                   -Status   "Scanned: $scanned / $total, Left: $left" `
                   -PercentComplete $percent

    # Search current file
    $fileMatches = Select-String -Path $file.FullName -Pattern $pattern -SimpleMatch -ErrorAction SilentlyContinue
    if ($fileMatches) {
        $matches += $fileMatches
        foreach ($m in $fileMatches) {
            "{0}:{1}: {2}" -f $m.Path, $m.LineNumber, $m.Line
        }
    }
}

Write-Progress -Activity "Searching files" -Completed

Write-Host ""
Write-Host "Total files scanned: $total"
Write-Host "Total matches: $($matches.Count)"