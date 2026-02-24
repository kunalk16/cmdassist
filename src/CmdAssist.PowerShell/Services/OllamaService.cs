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
/// Ollama service implementation
/// </summary>
public class OllamaService : IOllamaService
{
    private readonly HttpClient _httpClient;
    private readonly IConfigurationService _configurationService;
    private readonly ILogger<OllamaService> _logger;
    private readonly IHttpPolicyService _httpPolicyService;

    public OllamaService(
        HttpClient httpClient,
        IConfigurationService configurationService,
        ILogger<OllamaService> logger,
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
            var config = _configurationService.GetAiConfiguration().Ollama;
            
            if (string.IsNullOrEmpty(config.Endpoint))
            {
                throw new InvalidOperationException("Ollama endpoint not configured. Set the OLLAMA_ENDPOINT environment variable.");
            }

            if (string.IsNullOrEmpty(config.Model))
            {
                throw new InvalidOperationException("Ollama model not configured. Set the OLLAMA_MODEL environment variable.");
            }

            var systemPrompt = CreateSystemPrompt(request.Context);
            var userPrompt = CreateUserPrompt(request.Prompt, request.Context);

            // Try chat completion format first
            var chatResponse = await TryChatCompletion(config, systemPrompt, userPrompt, request);
            if (chatResponse != null)
            {
                return chatResponse;
            }

            // Fallback to generate format
            return await TryGenerate(config, systemPrompt, userPrompt, request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Ollama API");
            throw;
        }
    }

    private async Task<AiResponse?> TryChatCompletion(OllamaConfiguration config, string systemPrompt, string userPrompt, AiRequest request)
    {
        try
        {
            var requestBody = new
            {
                model = config.Model,
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userPrompt }
                },
                stream = false,
                options = new
                {
                    temperature = request.Temperature,
                    num_predict = request.MaxTokens
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var url = $"{config.Endpoint.TrimEnd('/')}/api/chat";

            _logger.LogDebug("Sending chat request to Ollama: {Url}", url);

            var policy = _httpPolicyService.GetAiHttpPolicy();
            var response = await policy.ExecuteAsync(async _ =>
                await _httpClient.PostAsync(url, content));

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Ollama chat completion failed with status: {StatusCode}", response.StatusCode);
                return null; // Will try generate format
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            var ollamaResponse = JsonSerializer.Deserialize<JsonElement>(responseJson);

            var commandText = ollamaResponse
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? string.Empty;

            return ParseCommandResponse(commandText);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error with Ollama chat completion, will try generate format");
            return null;
        }
    }

    private async Task<AiResponse?> TryGenerate(OllamaConfiguration config, string systemPrompt, string userPrompt, AiRequest request)
    {
        try
        {
            var combinedPrompt = $"{systemPrompt}\n\nUser Request: {userPrompt}";

            var requestBody = new
            {
                model = config.Model,
                prompt = combinedPrompt,
                stream = false,
                options = new
                {
                    temperature = request.Temperature,
                    num_predict = request.MaxTokens
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var url = $"{config.Endpoint.TrimEnd('/')}/api/generate";

            _logger.LogDebug("Sending generate request to Ollama: {Url}", url);

            var policy = _httpPolicyService.GetAiHttpPolicy();
            var response = await policy.ExecuteAsync(async _ =>
                await _httpClient.PostAsync(url, content));

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Ollama API request failed: {response.StatusCode} - {errorContent}");
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            var ollamaResponse = JsonSerializer.Deserialize<JsonElement>(responseJson);

            var commandText = ollamaResponse.GetProperty("response").GetString() ?? string.Empty;

            return ParseCommandResponse(commandText);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error with Ollama generate");
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
                Explanation = "Command suggested by Ollama",
                Confidence = 0.6,
                Warning = "Response format was not as expected"
            };
        }
    }
}