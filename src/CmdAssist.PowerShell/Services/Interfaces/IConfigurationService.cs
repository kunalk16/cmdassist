// Copyright (c) 2026 Kunal Karmakar
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using CmdAssist.PowerShell.Models;

namespace CmdAssist.PowerShell.Services.Interfaces;

/// <summary>
/// Configuration service interface
/// </summary>
public interface IConfigurationService
{
    AiConfiguration GetAiConfiguration();
    T GetConfiguration<T>(string section) where T : class, new();
}