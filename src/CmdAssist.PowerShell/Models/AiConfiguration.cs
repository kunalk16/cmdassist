// Copyright (c) 2026 Kunal Karmakar
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace CmdAssist.PowerShell.Models;

/// <summary>
/// Configuration for AI providers
/// </summary>
public class AiConfiguration
{
    public OpenAiConfiguration OpenAI { get; set; } = new();
    public AzureOpenAiConfiguration AzureOpenAI { get; set; } = new();
    public ClaudeConfiguration Claude { get; set; } = new();
    public LlamaConfiguration Llama { get; set; } = new();
}