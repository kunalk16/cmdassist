// Copyright (c) 2026 Kunal Karmakar
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using CmdAssist.PowerShell.Models;
using CmdAssist.PowerShell.Services.Interfaces;
using Microsoft.Extensions.Configuration;

namespace CmdAssist.PowerShell.Services;

/// <summary>
/// Service for managing configuration from environment variables
/// </summary>
public class ConfigurationService : IConfigurationService
{
    private readonly IConfiguration _configuration;

    public ConfigurationService()
    {
        var configurationBuilder = new ConfigurationBuilder()
            .AddEnvironmentVariables();
        
        _configuration = configurationBuilder.Build();
    }

    public AiConfiguration GetAiConfiguration()
    {
        return new AiConfiguration
        {
            OpenAI = new OpenAiConfiguration
            {
                ApiKey = _configuration["OPENAI_API_KEY"] ?? string.Empty,
                ApiUrl = _configuration["OPENAI_API_URL"] ?? "https://api.openai.com/v1",
                Model = _configuration["OPENAI_MODEL"] ?? "gpt-4",
                Organization = _configuration["OPENAI_ORGANIZATION"] ?? string.Empty
            },
            AzureOpenAI = new AzureOpenAiConfiguration
            {
                ApiKey = _configuration["AZURE_OPENAI_API_KEY"] ?? string.Empty,
                Endpoint = _configuration["AZURE_OPENAI_ENDPOINT"] ?? string.Empty,
                DeploymentName = _configuration["AZURE_OPENAI_DEPLOYMENT_NAME"] ?? string.Empty,
                ApiVersion = _configuration["AZURE_OPENAI_API_VERSION"] ?? "2024-02-01"
            },
            Claude = new ClaudeConfiguration
            {
                ApiKey = _configuration["CLAUDE_API_KEY"] ?? string.Empty,
                ApiUrl = _configuration["CLAUDE_API_URL"] ?? "https://api.anthropic.com/v1",
                Model = _configuration["CLAUDE_MODEL"] ?? "claude-3-sonnet-20240229"
            },
            Llama = new LlamaConfiguration
            {
                ApiKey = _configuration["LLAMA_API_KEY"] ?? string.Empty,
                ApiUrl = _configuration["LLAMA_API_URL"] ?? string.Empty,
                Model = _configuration["LLAMA_MODEL"] ?? "llama2-70b-chat"
            }
        };
    }

    public T GetConfiguration<T>(string section) where T : class, new()
    {
        var config = new T();
        _configuration.GetSection(section).Bind(config);
        return config;
    }
}