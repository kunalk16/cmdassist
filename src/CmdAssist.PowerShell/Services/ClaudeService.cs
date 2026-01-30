using System.Text;
using System.Text.Json;
using CmdAssist.PowerShell.Models;
using CmdAssist.PowerShell.Constants;
using Microsoft.Extensions.Logging;

namespace CmdAssist.PowerShell.Services;

/// <summary>
/// Claude AI service implementation
/// </summary>
public class ClaudeService : IClaudeService
{
    private readonly HttpClient _httpClient;
    private readonly IConfigurationService _configurationService;
    private readonly ILogger<ClaudeService> _logger;

    public ClaudeService(
        HttpClient httpClient,
        IConfigurationService configurationService,
        ILogger<ClaudeService> logger)
    {
        _httpClient = httpClient;
        _configurationService = configurationService;
        _logger = logger;
    }

    public async Task<AiResponse?> GetCommandSuggestionAsync(AiRequest request)
    {
        try
        {
            var config = _configurationService.GetAiConfiguration().Claude;
            
            if (string.IsNullOrEmpty(config.ApiKey))
            {
                throw new InvalidOperationException("Claude API key not configured. Set the CLAUDE_API_KEY environment variable.");
            }

            var systemPrompt = CreateSystemPrompt(request.Context);
            var userPrompt = CreateUserPrompt(request.Prompt, request.Context);

            var requestBody = new
            {
                model = config.Model,
                max_tokens = request.MaxTokens,
                temperature = request.Temperature,
                system = systemPrompt,
                messages = new[]
                {
                    new { role = "user", content = userPrompt }
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("x-api-key", config.ApiKey);
            _httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

            _logger.LogDebug("Sending request to Claude: {Url}", $"{config.ApiUrl}/messages");

            var response = await _httpClient.PostAsync($"{config.ApiUrl}/messages", content);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Claude API request failed: {response.StatusCode} - {errorContent}");
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            var claudeResponse = JsonSerializer.Deserialize<JsonElement>(responseJson);

            var commandText = claudeResponse
                .GetProperty("content")[0]
                .GetProperty("text")
                .GetString() ?? string.Empty;

            return ParseCommandResponse(commandText);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Claude API");
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