// Copyright (c) 2026 Kunal Karmakar
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Text;
using System.Text.Json;
using CmdAssist.PowerShell.Models;
using CmdAssist.PowerShell.Constants;
using Microsoft.Extensions.Logging;

namespace CmdAssist.PowerShell.Services;

/// <summary>
/// Llama AI service implementation
/// </summary>
public class LlamaService : ILlamaService
{
    private readonly HttpClient _httpClient;
    private readonly IConfigurationService _configurationService;
    private readonly ILogger<LlamaService> _logger;

    public LlamaService(
        HttpClient httpClient,
        IConfigurationService configurationService,
        ILogger<LlamaService> logger)
    {
        _httpClient = httpClient;
        _configurationService = configurationService;
        _logger = logger;
    }

    public async Task<AiResponse?> GetCommandSuggestionAsync(AiRequest request)
    {
        try
        {
            var config = _configurationService.GetAiConfiguration().Llama;
            
            if (string.IsNullOrEmpty(config.ApiUrl))
            {
                throw new InvalidOperationException(
                    "Llama API URL not configured. Set the LLAMA_API_URL environment variable to your Llama endpoint.");
            }

            var systemPrompt = CreateSystemPrompt(request.Context);
            var userPrompt = CreateUserPrompt(request.Prompt, request.Context);
            var combinedPrompt = $"{systemPrompt}\n\nUser Request: {userPrompt}";

            // Note: Llama API formats may vary depending on the hosting service
            // This implementation assumes an OpenAI-compatible endpoint
            var requestBody = new
            {
                model = config.Model,
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userPrompt }
                },
                temperature = request.Temperature,
                max_tokens = request.MaxTokens
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            
            // Add API key if provided
            if (!string.IsNullOrEmpty(config.ApiKey))
            {
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {config.ApiKey}");
            }

            // Try OpenAI-compatible format first
            var url = config.ApiUrl.EndsWith("/chat/completions") ? config.ApiUrl : $"{config.ApiUrl.TrimEnd('/')}/chat/completions";
            
            _logger.LogDebug("Sending request to Llama: {Url}", url);

            var response = await _httpClient.PostAsync(url, content);
            
            if (!response.IsSuccessStatusCode)
            {
                // Fallback: try a simple completion endpoint
                return await TrySimpleCompletion(config, combinedPrompt, request);
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            var llamaResponse = JsonSerializer.Deserialize<JsonElement>(responseJson);

            string commandText;
            
            // Handle different response formats
            if (llamaResponse.TryGetProperty("choices", out var choices))
            {
                // OpenAI-compatible format
                commandText = choices[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString() ?? string.Empty;
            }
            else if (llamaResponse.TryGetProperty("response", out var responseText))
            {
                // Simple response format
                commandText = responseText.GetString() ?? string.Empty;
            }
            else
            {
                throw new InvalidOperationException("Unsupported Llama response format");
            }

            return ParseCommandResponse(commandText);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Llama API");
            throw;
        }
    }

    private async Task<AiResponse?> TrySimpleCompletion(LlamaConfiguration config, string prompt, AiRequest request)
    {
        try
        {
            var requestBody = new
            {
                prompt = prompt,
                max_tokens = request.MaxTokens,
                temperature = request.Temperature
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var url = config.ApiUrl.TrimEnd('/');
            if (!url.EndsWith("/completions") && !url.EndsWith("/generate"))
            {
                url += "/completions";
            }

            var response = await _httpClient.PostAsync(url, content);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Llama API request failed: {response.StatusCode} - {errorContent}");
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            var llamaResponse = JsonSerializer.Deserialize<JsonElement>(responseJson);

            var commandText = llamaResponse.TryGetProperty("choices", out var choices) 
                ? choices[0].GetProperty("text").GetString() ?? string.Empty
                : llamaResponse.GetProperty("response").GetString() ?? string.Empty;

            return ParseCommandResponse(commandText);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error with Llama simple completion");
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