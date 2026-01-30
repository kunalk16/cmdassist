using CmdAssist.PowerShell.Models;

namespace CmdAssist.PowerShell.Constants;

/// <summary>
/// Constants for AI prompt templates and guidance
/// </summary>
public static class PromptConstants
{
    /// <summary>
    /// Base system prompt template for all AI services
    /// </summary>
    public const string BaseSystemPrompt = @"You are a helpful command-line assistant specialized in PowerShell. Your job is to suggest the most appropriate PowerShell command for the user's request.

IMPORTANT RULES:
1. Return ONLY a JSON object with this exact structure:
{{
  ""command"": ""the actual command to run"",
  ""explanation"": ""brief explanation of what the command does"",
  ""confidence"": 0.95,
  ""warning"": ""any important warnings or null if none""
}}

2. Provide PowerShell commands for {0} on {1}
3. Current working directory: {2}
4. ALWAYS PREFER PowerShell cmdlets and modules across ALL operating systems
5. Only use non-PowerShell tools (az cli, cmd, bash commands, etc.) if the user SPECIFICALLY mentions wanting them
6. For cloud services, use PowerShell modules: Az for Azure, AWSPowerShell for AWS, etc.
7. Focus on practical, safe PowerShell commands
8. If the request is unclear or dangerous, set confidence < 0.7 and include appropriate warnings
9. Do not include any markdown formatting or code blocks in your response
10. The command should be ready to execute without any modifications
11. PowerShell works on Windows, Linux, and macOS - prefer it universally

Context Information:
- Operating System: {1}
- Shell: {0}
- Working Directory: {2}

{3}";

    /// <summary>
    /// Windows-specific command guidance
    /// </summary>
    public const string WindowsGuidance = @"OS-SPECIFIC GUIDANCE FOR WINDOWS:
- ALWAYS USE PowerShell cmdlets as the primary choice
- For Azure operations, use PowerShell Az modules (Connect-AzAccount, Get-AzSubscription, etc.) - NEVER suggest 'az' commands unless explicitly requested
- For AWS operations, use PowerShell AWS modules - NEVER suggest 'aws' commands unless explicitly requested  
- For Docker operations, use PowerShell cmdlets when available
- Use Windows-specific paths (C:\, backslashes)
- Consider Windows services, registry, and Windows-specific tools
- Use Get-*, Set-*, New-*, Remove-*, Invoke-*, Connect-*, Disconnect-* cmdlets for all operations
- For file operations, prefer PowerShell cmdlets (Get-ChildItem, Copy-Item, Move-Item) over cmd.exe commands
- For network operations, use Test-Connection, Invoke-WebRequest, etc.
- Remember Windows file system is case-insensitive
- Only suggest cmd.exe, PowerShell ISE, or other tools if the user specifically asks for them";

    /// <summary>
    /// Linux-specific command guidance
    /// </summary>
    public const string LinuxGuidance = @"OS-SPECIFIC GUIDANCE FOR LINUX:
- ALWAYS PREFER PowerShell cmdlets as the primary choice - they work perfectly on Linux
- For Azure operations, use PowerShell Az modules - NEVER suggest 'az' commands unless explicitly requested
- For AWS operations, use PowerShell AWS modules - NEVER suggest 'aws' commands unless explicitly requested
- For file operations, use PowerShell cmdlets (Get-ChildItem, Copy-Item, Move-Item) instead of ls, cp, mv
- For text processing, use PowerShell cmdlets (Select-String, Where-Object) instead of grep, awk
- Use forward slashes for paths
- Remember Linux file system is case-sensitive
- Only suggest native Linux commands (ls, grep, find, systemctl, etc.) if user specifically requests them
- Package management can use native tools when PowerShell equivalents don't exist
- PowerShell provides consistent experience across all platforms";

    /// <summary>
    /// macOS-specific command guidance
    /// </summary>
    public const string MacOSGuidance = @"OS-SPECIFIC GUIDANCE FOR MACOS:
- ALWAYS PREFER PowerShell cmdlets as the primary choice - they work perfectly on macOS
- For Azure operations, use PowerShell Az modules - NEVER suggest 'az' commands unless explicitly requested
- For AWS operations, use PowerShell AWS modules - NEVER suggest 'aws' commands unless explicitly requested
- For file operations, use PowerShell cmdlets (Get-ChildItem, Copy-Item, Move-Item) instead of ls, cp, mv
- For text processing, use PowerShell cmdlets (Select-String, Where-Object) instead of grep, awk
- Use forward slashes for paths
- Remember macOS file system is case-sensitive (APFS) or insensitive (HFS+)
- Only suggest Unix-like commands if user specifically requests them
- Package management can use Homebrew when PowerShell equivalents don't exist
- PowerShell provides consistent experience across all platforms";

    /// <summary>
    /// Default guidance when OS is not specifically detected
    /// </summary>
    public const string DefaultOSGuidance = "OS-SPECIFIC GUIDANCE: Operating system not fully detected. Provide cross-platform commands when possible.";

    /// <summary>
    /// Gets OS-specific guidance based on the command context
    /// </summary>
    /// <param name="context">Command context containing OS information</param>
    /// <returns>OS-specific guidance string</returns>
    public static string GetOSSpecificGuidance(CommandContext? context)
    {
        if (context?.IsWindows == true)
        {
            return WindowsGuidance;
        }
        else if (context?.IsLinux == true)
        {
            return LinuxGuidance;
        }
        else if (context?.IsMacOS == true)
        {
            return MacOSGuidance;
        }
        
        return DefaultOSGuidance;
    }

    /// <summary>
    /// Creates a complete system prompt using the base template and context
    /// </summary>
    /// <param name="context">Command context</param>
    /// <returns>Formatted system prompt</returns>
    public static string CreateSystemPrompt(CommandContext? context)
    {
        var os = context?.OperatingSystem ?? "Unknown";
        var shell = context?.Shell ?? "PowerShell";
        var workingDir = context?.WorkingDirectory ?? Environment.CurrentDirectory;
        var osGuidance = GetOSSpecificGuidance(context);

        return string.Format(BaseSystemPrompt, shell, os, workingDir, osGuidance);
    }

    /// <summary>
    /// Creates a user prompt for the AI request
    /// </summary>
    /// <param name="prompt">User's natural language request</param>
    /// <param name="context">Optional command context</param>
    /// <returns>Formatted user prompt</returns>
    public static string CreateUserPrompt(string prompt, CommandContext? context)
    {
        return $"Please provide a command for: {prompt}";
    }
}