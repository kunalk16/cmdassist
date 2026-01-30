using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;

namespace CmdAssist.PowerShell.Services;

/// <summary>
/// Service for executing PowerShell commands
/// </summary>
public class PowerShellCommandExecutionService : ICommandExecutionService
{
    private readonly ILogger<PowerShellCommandExecutionService> _logger;

    public PowerShellCommandExecutionService(ILogger<PowerShellCommandExecutionService> logger)
    {
        _logger = logger;
    }

    public void ExecuteCommand(string command)
    {
        try
        {
            _logger.LogInformation("Executing command: {Command}", command);

            using var powerShell = System.Management.Automation.PowerShell.Create();
            powerShell.AddScript(command);

            var results = powerShell.Invoke();

            // Display results
            foreach (var result in results)
            {
                Console.WriteLine(result?.ToString());
            }

            // Display errors if any
            if (powerShell.HadErrors)
            {
                foreach (var error in powerShell.Streams.Error)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Error: {error}");
                    Console.ResetColor();
                }
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

            foreach (var result in results)
            {
                output.AppendLine(result?.ToString());
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