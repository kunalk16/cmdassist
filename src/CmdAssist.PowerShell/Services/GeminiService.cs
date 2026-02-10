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
/// Google Gemini AI service implementation
/// </summary>
public class GeminiService : IGeminiService
{
    private readonly HttpClient _httpClient;
    private readonly IConfigurationService _configurationService;
    private readonly ILogger<GeminiService> _logger;
    private readonly IHttpPolicyService _httpPolicyService;

    public GeminiService(
        HttpClient httpClient,
        IConfigurationService configurationService,
        ILogger<GeminiService> logger,
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
            var config = _configurationService.GetAiConfiguration().Gemini;
            
            if (string.IsNullOrEmpty(config.ApiKey))
            {
                throw new InvalidOperationException("Gemini API key not configured. Set the GEMINI_API_KEY environment variable.");
            }

            var systemPrompt = PromptConstants.CreateSystemPrompt(request.Context);
            var userPrompt = PromptConstants.CreateUserPrompt(request.Prompt, request.Context);

            // Gemini API request format
            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = $"{systemPrompt}\n\n{userPrompt}" }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = request.Temperature,
                    maxOutputTokens = request.MaxTokens,
                    topK = 1,
                    topP = 0.8
                }
            };

            var jsonContent = JsonSerializer.Serialize(requestBody);
            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var requestUrl = $"{config.ApiUrl.TrimEnd('/')}/models/{config.Model}:generateContent?key={config.ApiKey}";
            
            _logger.LogInformation("Sending request to Gemini API");
            
            var policy = _httpPolicyService.GetAiHttpPolicy();
            var response = await policy.ExecuteAsync(async _ => 
                await _httpClient.PostAsync(requestUrl, httpContent));
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Gemini API request failed with status {StatusCode}: {ErrorContent}", 
                    response.StatusCode, errorContent);
                throw new HttpRequestException($"Gemini API request failed: {response.StatusCode}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("Received response from Gemini API: {Response}", responseContent);

            using var document = JsonDocument.Parse(responseContent);
            var root = document.RootElement;

            if (root.TryGetProperty("candidates", out var candidates) && 
                candidates.GetArrayLength() > 0)
            {
                var firstCandidate = candidates[0];
                if (firstCandidate.TryGetProperty("content", out var content) &&
                    content.TryGetProperty("parts", out var parts) && 
                    parts.GetArrayLength() > 0)
                {
                    var textPart = parts[0];
                    if (textPart.TryGetProperty("text", out var textElement))
                    {
                        var fullResponse = textElement.GetString() ?? string.Empty;
                        return ParseAiResponse(fullResponse);
                    }
                }
            }

            _logger.LogWarning("No valid response found in Gemini API response");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Gemini API");
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
                Explanation = "Response from Gemini AI",
                Confidence = 0.5,
                Warning = "Could not parse structured response from Gemini API"
            };
        }
    }
}