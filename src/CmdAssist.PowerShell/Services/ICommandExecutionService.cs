// Copyright (c) 2026 Kunal Karmakar
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace CmdAssist.PowerShell.Services;

/// <summary>
/// Command execution service interface
/// </summary>
public interface ICommandExecutionService
{
    void ExecuteCommand(string command);
    Task<string> ExecuteCommandAsync(string command);
}