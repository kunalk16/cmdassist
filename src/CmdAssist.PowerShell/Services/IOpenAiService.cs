// Copyright (c) 2026 Kunal Karmakar
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using CmdAssist.PowerShell.Models;

namespace CmdAssist.PowerShell.Services;

/// <summary>
/// OpenAI service interface
/// </summary>
public interface IOpenAiService
{
    Task<AiResponse?> GetCommandSuggestionAsync(AiRequest request);
}