namespace CmdAssist.PowerShell.Models;

/// <summary>
/// Context information for command generation
/// </summary>
public class CommandContext
{
    public string OperatingSystem { get; set; } = string.Empty;
    public string Shell { get; set; } = "PowerShell";
    public string WorkingDirectory { get; set; } = string.Empty;
    public Dictionary<string, string> EnvironmentVariables { get; set; } = new();
    public bool IsWindows { get; set; }
    public bool IsLinux { get; set; }
    public bool IsMacOS { get; set; }
}