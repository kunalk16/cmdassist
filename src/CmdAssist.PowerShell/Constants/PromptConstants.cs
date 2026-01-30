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
    public const string BaseSystemPrompt = @"You are a helpful command-line assistant. Your job is to suggest the most appropriate command for the user's request.

IMPORTANT RULES:
1. Return ONLY a JSON object with this exact structure:
{{
  ""command"": ""the actual command to run"",
  ""explanation"": ""brief explanation of what the command does"",
  ""confidence"": 0.95,
  ""warning"": ""any important warnings or null if none""
}}

2. Provide commands for {0} on {1}
3. Current working directory: {2}
4. Focus on practical, safe commands
5. If the request is unclear or dangerous, set confidence < 0.7 and include appropriate warnings
6. Do not include any markdown formatting or code blocks in your response
7. The command should be ready to execute without any modifications

Context Information:
- Operating System: {1}
- Shell: {0}
- Working Directory: {2}

{3}";

    /// <summary>
    /// Windows-specific command guidance
    /// </summary>
    public const string WindowsGuidance = @"OS-SPECIFIC GUIDANCE FOR WINDOWS:
- Prefer PowerShell cmdlets over legacy cmd.exe commands when possible
- Use Windows-specific paths (C:\, backslashes)
- Consider Windows services, registry, and Windows-specific tools
- Use Get-*, Set-*, New-*, Remove-* cmdlets for system operations
- For file operations, prefer PowerShell cmdlets over external tools
- Remember Windows file system is case-insensitive";

    /// <summary>
    /// Linux-specific command guidance
    /// </summary>
    public const string LinuxGuidance = @"OS-SPECIFIC GUIDANCE FOR LINUX:
- Use standard Unix/Linux command-line tools (ls, grep, find, etc.)
- Use forward slashes for paths
- Consider package managers (apt, yum, dnf, pacman) for installations
- Use systemctl for service management
- Remember Linux file system is case-sensitive
- Prefer native Linux commands but PowerShell cmdlets are also available";

    /// <summary>
    /// macOS-specific command guidance
    /// </summary>
    public const string MacOSGuidance = @"OS-SPECIFIC GUIDANCE FOR MACOS:
- Use Unix-like commands similar to Linux
- Use forward slashes for paths
- Consider Homebrew for package management
- Use launchctl for service management
- Remember macOS file system is case-sensitive (APFS) or insensitive (HFS+)
- Prefer native macOS/Unix commands but PowerShell cmdlets are also available";

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