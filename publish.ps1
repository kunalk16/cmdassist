$modulePath = "$PSScriptRoot\src\CmdAssist.PowerShell\bin\Release\net8.0"
Publish-Module -Path $modulePath -Repository "PSGallery" -NuGetApiKey $env:PSGALLERY_API_KEY -Verbose