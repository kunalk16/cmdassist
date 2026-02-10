# Create the module folder
$moduleFolder = "$PSScriptRoot\CmdAssist"
$sourceFolder = "$PSScriptRoot\src\CmdAssist.PowerShell\bin\Release\net8.0"

# Clean and create module directory
if (Test-Path $moduleFolder) {
    Remove-Item $moduleFolder -Recurse -Force
}
New-Item -ItemType Directory -Path $moduleFolder -Force | Out-Null

# Copy all contents from build output to module folder
Copy-Item "$sourceFolder\*" -Destination $moduleFolder -Recurse -Force

# Publish the module
Publish-Module -Path $moduleFolder -Repository "PSGallery" -NuGetApiKey $env:PSGALLERY_API_KEY -Verbose