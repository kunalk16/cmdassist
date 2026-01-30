namespace CmdAssist.PowerShell.Models;

public class ClaudeConfiguration
{
    public string ApiKey { get; set; } = string.Empty;
    public string ApiUrl { get; set; } = "https://api.anthropic.com/v1";
    public string Model { get; set; } = "claude-sonnet-4-20250514";
}