@{
    # Script module or binary module file associated with this manifest
    RootModule = 'CmdAssist.PowerShell.dll'
    
    # Version number of this module
    ModuleVersion = '1.0.0'
    
    # ID used to uniquely identify this module
    GUID = 'adc41e35-80bd-482b-ad3b-7e876b571cb2'
    
    # Author of this module
    Author = 'CmdAssist'
    
    # Company or vendor of this module
    CompanyName = 'CmdAssist'
    
    # Copyright statement for this module
    Copyright = '© CmdAssist. All rights reserved.'
    
    # Description of the functionality provided by this module
    Description = 'AI-powered command line assistant for PowerShell'
    
    # Minimum version of the PowerShell engine required by this module
    PowerShellVersion = '7.4'
    
    # Functions to export from this module
    FunctionsToExport = @()
    
    # Cmdlets to export from this module
    CmdletsToExport = @('Invoke-CmdAssist')
    
    # Variables to export from this module
    VariablesToExport = @()
    
    # Aliases to export from this module
    AliasesToExport = @('cmd-assist')
    
    # Private data to pass to the module specified in RootModule/ModuleToProcess
    PrivateData = @{
        PSData = @{
            # Tags applied to this module. These help with module discovery in online galleries.
            Tags = @('AI', 'CommandLine', 'Assistant', 'PowerShell')
            
            # A URL to the license for this module.
            LicenseUri = 'https://opensource.org/licenses/MIT'
            
            # A URL to the main website for this project.
            ProjectUri = 'https://github.com/kunalk16/cmdassist'
            
            # ReleaseNotes of this module
            ReleaseNotes = 'Initial release of CmdAssist PowerShell module'
        }
    }
}