Push-Location
Set-Location $PSScriptRoot

Start-Job { Stop-Process -Name PowerToys* } | Wait-Job > $null

Start-Sleep -Seconds 1



# change this to your PowerToys installation path
$ptPath = 'C:\Users\elias\AppData\Local\PowerToys'
$projectName = 'JetbrainsProjects'
$safeProjectName = 'JetbrainsProjects'
$debug = '.\bin\ARM64\Debug\net8.0-windows'
$dest = "$env:LOCALAPPDATA\Microsoft\PowerToys\PowerToys Run\Plugins\$projectName"
$files = @(
    "Community.PowerToys.Run.Plugin.$safeProjectName.deps.json",
    "Community.PowerToys.Run.Plugin.$safeProjectName.dll",
    'plugin.json',
    'Images'
)

Set-Location $debug
mkdir $dest -Force -ErrorAction Ignore | Out-Null
Copy-Item $files $dest -Force -Recurse

& "$ptPath\PowerToys.exe"


Pop-Location