// Copyright (c) 2026 Kunal Karmakar
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Management.Automation;
using System.Text;
using CmdAssist.PowerShell.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace CmdAssist.PowerShell.Services;

/// <summary>
/// Service for executing PowerShell commands
/// </summary>
public class PowerShellCommandExecutionService : ICommandExecutionService
{
    private readonly ILogger<PowerShellCommandExecutionService> _logger;
    private readonly PSCmdlet _psCmdlet;

    public PowerShellCommandExecutionService(ILogger<PowerShellCommandExecutionService> logger,
        PSCmdlet psCmdlet)
    {
        _logger = logger;
        _psCmdlet = psCmdlet;
    }

    public void ExecuteCommand(string command)
    {
        try
        {
            _logger.LogInformation("Executing command: {Command}", command);

            var results = _psCmdlet.SessionState.InvokeCommand.InvokeScript(command);

            foreach (var result in results)
            {
                _psCmdlet.WriteObject(result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing command: {Command}", command);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Failed to execute command: {ex.Message}");
            Console.ResetColor();
        }
    }

    public async Task<string> ExecuteCommandAsync(string command)
    {
        try
        {
            _logger.LogInformation("Executing command asynchronously: {Command}", command);

            using var powerShell = System.Management.Automation.PowerShell.Create();
            powerShell.AddScript(command);

            var results = await Task.Run(() => powerShell.Invoke());
            var output = new StringBuilder();

            // Format results using PowerShell's default formatting
            if (results.Any())
            {
                using var formatPowerShell = System.Management.Automation.PowerShell.Create();
                formatPowerShell.AddCommand("Out-String")
                    .AddParameter("InputObject", results)
                    .AddParameter("Width", 120);
                
                var formattedResults = formatPowerShell.Invoke();
                foreach (var formatted in formattedResults)
                {
                    var formattedOutput = formatted?.ToString()?.TrimEnd('\r', '\n');
                    if (!string.IsNullOrEmpty(formattedOutput))
                    {
                        output.AppendLine(formattedOutput);
                    }
                }
            }

            if (powerShell.HadErrors)
            {
                foreach (var error in powerShell.Streams.Error)
                {
                    output.AppendLine($"Error: {error}");
                }
            }

            return output.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing command asynchronously: {Command}", command);
            return $"Failed to execute command: {ex.Message}";
        }
    }
}