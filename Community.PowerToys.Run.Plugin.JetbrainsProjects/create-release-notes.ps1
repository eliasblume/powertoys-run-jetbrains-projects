## load vars from params
param (
    [string]$name,
    [string]$version,
    [string]$projectUri,
    [string]$result
)



[xml]$props = Get-Content -Path "*.csproj"

function Write-Line {
    param ([string]$line)
    $line | Add-Content -Path $result
}

function Get-Platform {
    param ([string]$filename)
    if ($filename -Match $platforms[0]) {$platforms[0]}
    else { $platforms[1]}
}

$platforms = "$($props.Project.PropertyGroup.Platforms)".Trim() -split ";"
$files = Get-ChildItem -Path bin/ -File -Include "$name-$version*.zip" -Recurse

Set-Content -Path $result -Value ""

Write-Line "## $name"
Write-Line ""
Write-Line "| Platform | Filename | Downloads"
Write-Line "| --- | --- | ---"
foreach ($file in $files) {
    $zip = $file.Name
    $platform = Get-Platform $zip
    $url = "$projectUri/releases/download/v$version/$zip"
    $badge = "https://img.shields.io/github/downloads/$($projectUri.Replace('https://github.com/', ''))/v$version/$zip"
    Write-Line "| ``$platform`` | [$zip]($url) | [![$zip]($badge)]($url)"
}
Write-Line ""

Write-Line "### Installer Hashes"
Write-Line ""
Write-Line "| Filename | SHA256 Hash"
Write-Line "| --- | ---"
foreach ($file in $files) {
    $zip = $file.Name
    $hash = Get-FileHash $file -Algorithm SHA256 | Select-Object -ExpandProperty Hash

    Write-Line "| ``$zip`` | ``$hash``"
}

