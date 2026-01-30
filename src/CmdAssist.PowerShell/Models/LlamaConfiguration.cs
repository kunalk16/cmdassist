namespace CmdAssist.PowerShell.Models;

public class LlamaConfiguration
{
    public string ApiKey { get; set; } = string.Empty;
    public string ApiUrl { get; set; } = string.Empty;
    public string Model { get; set; } = "llama2-70b-chat";
}