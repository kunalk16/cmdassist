using CmdAssist.PowerShell.Models;

namespace CmdAssist.PowerShell.Services;

/// <summary>
/// Main AI service interface
/// </summary>
public interface IAiService
{
    Task<AiResponse?> GetCommandSuggestionAsync(AiRequest request);
}

/// <summary>
/// OpenAI service interface
/// </summary>
public interface IOpenAiService
{
    Task<AiResponse?> GetCommandSuggestionAsync(AiRequest request);
}

/// <summary>
/// Azure OpenAI service interface
/// </summary>
public interface IAzureOpenAiService
{
    Task<AiResponse?> GetCommandSuggestionAsync(AiRequest request);
}

/// <summary>
/// Claude service interface
/// </summary>
public interface IClaudeService
{
    Task<AiResponse?> GetCommandSuggestionAsync(AiRequest request);
}

/// <summary>
/// Llama service interface
/// </summary>
public interface ILlamaService
{
    Task<AiResponse?> GetCommandSuggestionAsync(AiRequest request);
}

/// <summary>
/// Command execution service interface
/// </summary>
public interface ICommandExecutionService
{
    void ExecuteCommand(string command);
    Task<string> ExecuteCommandAsync(string command);
}

/// <summary>
/// Configuration service interface
/// </summary>
public interface IConfigurationService
{
    AiConfiguration GetAiConfiguration();
    T GetConfiguration<T>(string section) where T : class, new();
}