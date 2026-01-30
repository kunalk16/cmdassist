namespace CmdAssist.PowerShell.Models;

/// <summary>
/// Supported AI providers
/// </summary>
public enum AiProvider
{
    OpenAI,
    AzureOpenAI,
    Claude,
    Llama
}

/// <summary>
/// Request model for AI service
/// </summary>
public class AiRequest
{
    public required string Prompt { get; set; }
    public AiProvider Provider { get; set; } = AiProvider.OpenAI;
    public CommandContext? Context { get; set; }
    public double Temperature { get; set; } = 0.1;
    public int MaxTokens { get; set; } = 500;
}

/// <summary>
/// Response model from AI service
/// </summary>
public class AiResponse
{
    public required string Command { get; set; }
    public string? Explanation { get; set; }
    public double Confidence { get; set; }
    public string? Warning { get; set; }
}

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

/// <summary>
/// Configuration for AI providers
/// </summary>
public class AiConfiguration
{
    public OpenAiConfiguration OpenAI { get; set; } = new();
    public AzureOpenAiConfiguration AzureOpenAI { get; set; } = new();
    public ClaudeConfiguration Claude { get; set; } = new();
    public LlamaConfiguration Llama { get; set; } = new();
}

public class OpenAiConfiguration
{
    public string ApiKey { get; set; } = string.Empty;
    public string ApiUrl { get; set; } = "https://api.openai.com/v1";
    public string Model { get; set; } = "gpt-4";
    public string Organization { get; set; } = string.Empty;
}

public class AzureOpenAiConfiguration
{
    public string ApiKey { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public string DeploymentName { get; set; } = string.Empty;
    public string ApiVersion { get; set; } = "2024-02-01";
}

public class ClaudeConfiguration
{
    public string ApiKey { get; set; } = string.Empty;
    public string ApiUrl { get; set; } = "https://api.anthropic.com/v1";
    public string Model { get; set; } = "claude-3-sonnet-20240229";
}

public class LlamaConfiguration
{
    public string ApiKey { get; set; } = string.Empty;
    public string ApiUrl { get; set; } = string.Empty;
    public string Model { get; set; } = "llama2-70b-chat";
}