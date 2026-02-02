// Copyright (c) 2026 Kunal Karmakar
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Text;
using System.Text.Json;
using CmdAssist.PowerShell.Models;
using CmdAssist.PowerShell.Constants;
using CmdAssist.PowerShell.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace CmdAssist.PowerShell.Services;

/// <summary>
/// OpenAI service implementation
/// </summary>
public class OpenAiService : IOpenAiService
{
    private readonly HttpClient _httpClient;
    private readonly IConfigurationService _configurationService;
    private readonly ILogger<OpenAiService> _logger;

    public OpenAiService(
        HttpClient httpClient,
        IConfigurationService configurationService,
        ILogger<OpenAiService> logger)
    {
        _httpClient = httpClient;
        _configurationService = configurationService;
        _logger = logger;
    }

    public async Task<AiResponse?> GetCommandSuggestionAsync(AiRequest request)
    {
        try
        {
            var config = _configurationService.GetAiConfiguration().OpenAI;
            
            if (string.IsNullOrEmpty(config.ApiKey))
            {
                throw new InvalidOperationException("OpenAI API key not configured. Set the OPENAI_API_KEY environment variable.");
            }

            var systemPrompt = CreateSystemPrompt(request.Context);
            var userPrompt = CreateUserPrompt(request.Prompt, request.Context);

            var requestBody = new
            {
                model = config.Model,
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userPrompt }
                },
                temperature = request.Temperature,
                max_completion_tokens = request.MaxTokens
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {config.ApiKey}");
            
            if (!string.IsNullOrEmpty(config.Organization))
            {
                _httpClient.DefaultRequestHeaders.Add("OpenAI-Organization", config.Organization);
            }

            _logger.LogDebug("Sending request to OpenAI: {Url}", $"{config.ApiUrl}/chat/completions");

            var response = await _httpClient.PostAsync($"{config.ApiUrl}/chat/completions", content);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"OpenAI API request failed: {response.StatusCode} - {errorContent}");
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            var openAiResponse = JsonSerializer.Deserialize<JsonElement>(responseJson);

            var commandText = openAiResponse
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? string.Empty;

            return ParseCommandResponse(commandText);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling OpenAI API");
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