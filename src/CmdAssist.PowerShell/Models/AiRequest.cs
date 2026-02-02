// Copyright (c) 2026 Kunal Karmakar
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace CmdAssist.PowerShell.Models;

/// <summary>
/// Request model for AI service
/// </summary>
public class AiRequest
{
    public required string Prompt { get; set; }
    public AiProvider Provider { get; set; } = AiProvider.AzureOpenAI;
    public CommandContext? Context { get; set; }
    public double Temperature { get; set; } = 0.1;
    public int MaxTokens { get; set; } = 500;
}