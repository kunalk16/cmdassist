// Copyright (c) 2026 Kunal Karmakar
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using CmdAssist.PowerShell.Models;
using Microsoft.Extensions.Logging;

namespace CmdAssist.PowerShell.Services;

/// <summary>
/// Main AI service that routes requests to specific providers
/// </summary>
public class AiService : IAiService
{
    private readonly IOpenAiService _openAiService;
    private readonly IAzureOpenAiService _azureOpenAiService;
    private readonly IClaudeService _claudeService;
    private readonly ILlamaService _llamaService;
    private readonly ILogger<AiService> _logger;

    public AiService(
        IOpenAiService openAiService,
        IAzureOpenAiService azureOpenAiService,
        IClaudeService claudeService,
        ILlamaService llamaService,
        ILogger<AiService> logger)
    {
        _openAiService = openAiService;
        _azureOpenAiService = azureOpenAiService;
        _claudeService = claudeService;
        _llamaService = llamaService;
        _logger = logger;
    }

    public async Task<AiResponse?> GetCommandSuggestionAsync(AiRequest request)
    {
        try
        {
            _logger.LogInformation("Processing AI request for provider: {Provider}", request.Provider);

            return request.Provider switch
            {
                AiProvider.OpenAI => await _openAiService.GetCommandSuggestionAsync(request),
                AiProvider.AzureOpenAI => await _azureOpenAiService.GetCommandSuggestionAsync(request),
                AiProvider.Claude => await _claudeService.GetCommandSuggestionAsync(request),
                AiProvider.Llama => await _llamaService.GetCommandSuggestionAsync(request),
                _ => throw new ArgumentException($"Unsupported AI provider: {request.Provider}")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting command suggestion from {Provider}", request.Provider);
            throw;
        }
    }
}