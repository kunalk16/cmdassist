using System.Text;
using System.Text.Json;
using CmdAssist.PowerShell.Models;
using CmdAssist.PowerShell.Constants;
using Microsoft.Extensions.Logging;

namespace CmdAssist.PowerShell.Services;

/// <summary>
/// Azure OpenAI service implementation
/// </summary>
public class AzureOpenAiService : IAzureOpenAiService
{
    private readonly HttpClient _httpClient;
    private readonly IConfigurationService _configurationService;
    private readonly ILogger<AzureOpenAiService> _logger;

    public AzureOpenAiService(
        HttpClient httpClient,
        IConfigurationService configurationService,
        ILogger<AzureOpenAiService> logger)
    {
        _httpClient = httpClient;
        _configurationService = configurationService;
        _logger = logger;
    }

    public async Task<AiResponse?> GetCommandSuggestionAsync(AiRequest request)
    {
        try
        {
            var config = _configurationService.GetAiConfiguration().AzureOpenAI;
            
            if (string.IsNullOrEmpty(config.ApiKey) || string.IsNullOrEmpty(config.Endpoint) || string.IsNullOrEmpty(config.DeploymentName))
            {
                throw new InvalidOperationException(
                    "Azure OpenAI configuration incomplete. Set AZURE_OPENAI_API_KEY, AZURE_OPENAI_ENDPOINT, and AZURE_OPENAI_DEPLOYMENT_NAME environment variables.");
            }

            var systemPrompt = CreateSystemPrompt(request.Context);
            var userPrompt = CreateUserPrompt(request.Prompt, request.Context);

            var requestBody = new
            {
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userPrompt }
                },
                temperature = 1,
                max_completion_tokens = request.MaxTokens
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("api-key", config.ApiKey);

            var url = $"{config.Endpoint.TrimEnd('/')}/openai/deployments/{config.DeploymentName}/chat/completions?api-version={config.ApiVersion}";
            
            _logger.LogDebug("Sending request to Azure OpenAI: {Url}", url);

            var response = await _httpClient.PostAsync(url, content);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Azure OpenAI API request failed: {response.StatusCode} - {errorContent}");
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            var azureResponse = JsonSerializer.Deserialize<JsonElement>(responseJson);

            var commandText = azureResponse
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? string.Empty;

            return ParseCommandResponse(commandText);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Azure OpenAI API");
            throw;
        }
    }

    private static string CreateSystemPrompt(CommandContext? context)
    {
        return PromptConstants.CreateSystemPrompt(context);
    }

    private static string CreateUserPrompt(string prompt, CommandContext? context)
    {
        return PromptConstants.CreateUserPrompt(prompt, context);
    }

    private static AiResponse ParseCommandResponse(string response)
    {
        try
        {
            var jsonResponse = JsonSerializer.Deserialize<JsonElement>(response);
            
            return new AiResponse
            {
                Command = jsonResponse.GetProperty("command").GetString() ?? string.Empty,
                Explanation = jsonResponse.TryGetProperty("explanation", out var exp) ? exp.GetString() : null,
                Confidence = jsonResponse.TryGetProperty("confidence", out var conf) ? conf.GetDouble() : 0.8,
                Warning = jsonResponse.TryGetProperty("warning", out var warn) ? warn.GetString() : null
            };
        }
        catch (JsonException)
        {
            // Fallback: treat the entire response as a command
            return new AiResponse
            {
                Command = response.Trim(),
                Explanation = "Command suggested by AI",
                Confidence = 0.6,
                Warning = "Response format was not as expected"
            };
        }
    }
}