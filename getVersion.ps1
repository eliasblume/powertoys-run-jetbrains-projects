$jsonContent = Get-Content -Path "Community.PowerToys.Run.Plugin.JetbrainsProjects/plugin.json" -Raw

$jsonObject = $jsonContent | ConvertFrom-Json

$version = $jsonObject.Version

