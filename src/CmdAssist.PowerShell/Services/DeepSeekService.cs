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
/// DeepSeek AI service implementation
/// </summary>
public class DeepSeekService : IDeepSeekService
{
    private readonly HttpClient _httpClient;
    private readonly IConfigurationService _configurationService;
    private readonly ILogger<DeepSeekService> _logger;
    private readonly IHttpPolicyService _httpPolicyService;

    public DeepSeekService(
        HttpClient httpClient,
        IConfigurationService configurationService,
        ILogger<DeepSeekService> logger,
        IHttpPolicyService httpPolicyService)
    {
        _httpClient = httpClient;
        _configurationService = configurationService;
        _logger = logger;
        _httpPolicyService = httpPolicyService;
    }

    public async Task<AiResponse?> GetCommandSuggestionAsync(AiRequest request)
    {
        try
        {
            var config = _configurationService.GetAiConfiguration().DeepSeek;
            
            if (string.IsNullOrEmpty(config.ApiKey))
            {
                throw new InvalidOperationException("DeepSeek API key not configured. Set the DEEPSEEK_API_KEY environment variable.");
            }

            var systemPrompt = PromptConstants.CreateSystemPrompt(request.Context);
            var userPrompt = PromptConstants.CreateUserPrompt(request.Prompt, request.Context);

            // DeepSeek uses OpenAI-compatible API format
            var requestBody = new
            {
                model = config.Model,
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userPrompt }
                },
                temperature = request.Temperature,
                max_tokens = request.MaxTokens,
                top_p = 0.8,
                frequency_penalty = 0,
                presence_penalty = 0,
                stream = false
            };

            var jsonContent = JsonSerializer.Serialize(requestBody);
            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            // Set headers
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {config.ApiKey}");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "CmdAssist-PowerShell/1.0");

            var requestUrl = $"{config.ApiUrl.TrimEnd('/')}/chat/completions";
            
            _logger.LogInformation("Sending request to DeepSeek API");
            
            var policy = _httpPolicyService.GetAiHttpPolicy();
            var response = await policy.ExecuteAsync(async _ => 
                await _httpClient.PostAsync(requestUrl, httpContent));
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("DeepSeek API request failed with status {StatusCode}: {ErrorContent}", 
                    response.StatusCode, errorContent);
                throw new HttpRequestException($"DeepSeek API request failed: {response.StatusCode}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("Received response from DeepSeek API: {Response}", responseContent);

            using var document = JsonDocument.Parse(responseContent);
            var root = document.RootElement;

            if (root.TryGetProperty("choices", out var choices) && 
                choices.GetArrayLength() > 0)
            {
                var firstChoice = choices[0];
                if (firstChoice.TryGetProperty("message", out var message) &&
                    message.TryGetProperty("content", out var content))
                {
                    var fullResponse = content.GetString() ?? string.Empty;
                    return ParseAiResponse(fullResponse);
                }
            }

            _logger.LogWarning("No valid response found in DeepSeek API response");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling DeepSeek API");
            throw;
        }
    }

    private static AiResponse? ParseAiResponse(string responseText)
    {
        try
        {
            // Try to parse as JSON first
            using var document = JsonDocument.Parse(responseText);
            var root = document.RootElement;

            return new AiResponse
            {
                Command = root.GetProperty("command").GetString() ?? string.Empty,
                Explanation = root.TryGetProperty("explanation", out var explanationElement) ? 
                    explanationElement.GetString() : null,
                Confidence = root.TryGetProperty("confidence", out var confidenceElement) ? 
                    confidenceElement.GetDouble() : 0.7,
                Warning = root.TryGetProperty("warning", out var warningElement) ? 
                    warningElement.GetString() : null
            };
        }
        catch
        {
            // If JSON parsing fails, return a basic response with the full text as explanation
            return new AiResponse
            {
                Command = responseText.Split('\n').FirstOrDefault(line => !string.IsNullOrWhiteSpace(line)) ?? responseText,
                Explanation = "Response from DeepSeek AI",
                Confidence = 0.5,
                Warning = "Could not parse structured response from DeepSeek API"
            };
        }
    }
}