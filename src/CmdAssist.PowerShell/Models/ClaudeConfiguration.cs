namespace CmdAssist.PowerShell.Models;

public class ClaudeConfiguration
{
    public string ApiKey { get; set; } = string.Empty;
    public string ApiUrl { get; set; } = "https://api.anthropic.com/v1";
    public string Model { get; set; } = "claude-3-sonnet-20240229";
}