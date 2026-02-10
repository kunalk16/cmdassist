// Copyright (c) 2026 Kunal Karmakar
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Management.Automation;
using System.Management.Automation.Host;
using System.Collections.ObjectModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CmdAssist.PowerShell.Services;
using CmdAssist.PowerShell.Services.Interfaces;
using CmdAssist.PowerShell.Models;

namespace CmdAssist.PowerShell.Cmdlets;

/// <summary>
/// PowerShell cmdlet that provides AI-powered command assistance
/// </summary>
[Cmdlet(VerbsLifecycle.Invoke, "CmdAssist")]
[Alias("cmd-assist")]
[OutputType(typeof(string))]
public class InvokeCmdAssistCmdlet : PSCmdlet
{
    /// <summary>
    /// The prompt describing what the user wants to accomplish
    /// </summary>
    [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true)]
    [ValidateNotNullOrEmpty]
    public string Prompt { get; set; } = string.Empty;

    /// <summary>
    /// When specified, executes the suggested command without confirmation
    /// </summary>
    [Parameter(Mandatory = false)]
    [Alias("y")]
    public SwitchParameter Confirm { get; set; }

    /// <summary>
    /// Specifies which AI provider to use (OpenAI, AzureOpenAI, Claude, Llama, Gemini, DeepSeek)
    /// </summary>
    [Parameter(Mandatory = false)]
    [ValidateSet("OpenAI", "AzureOpenAI", "Claude", "Llama", "Gemini", "DeepSeek")]
    public string Provider { get; set; } = "OpenAI";

    private IServiceProvider? _serviceProvider;
    private IAiService? _aiService;
    private ICommandExecutionService? _commandExecutionService;
    private ILogger<InvokeCmdAssistCmdlet>? _logger;

    protected override void BeginProcessing()
    {
        base.BeginProcessing();
        
        // Initialize dependency injection
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();
        
        // Resolve services
        _aiService = _serviceProvider.GetRequiredService<IAiService>();
        _commandExecutionService = _serviceProvider.GetRequiredService<ICommandExecutionService>();
        _logger = _serviceProvider.GetRequiredService<ILogger<InvokeCmdAssistCmdlet>>();
        
        _logger.LogInformation("CmdAssist cmdlet initialized with provider: {Provider}", Provider);
    }

    protected override void ProcessRecord()
    {
        try
        {
            _logger?.LogInformation("Processing prompt: {Prompt}", Prompt);
            
            WriteVerbose($"Sending prompt to {Provider}...");
            
            // Get suggested command from AI service
            var osInfo = GetOperatingSystemInfo();
            var aiRequest = new AiRequest
            {
                Prompt = Prompt,
                Provider = Enum.Parse<AiProvider>(Provider, true),
                Context = new CommandContext
                {
                    OperatingSystem = osInfo.Name,
                    Shell = this.GetShellInfo(),
                    WorkingDirectory = SessionState.Path.CurrentLocation.Path,
                    EnvironmentVariables = GetRelevantEnvironmentVariables(),
                    IsWindows = osInfo.IsWindows,
                    IsLinux = osInfo.IsLinux,
                    IsMacOS = osInfo.IsMacOS
                }
            };

            var response = _aiService!.GetCommandSuggestionAsync(aiRequest).GetAwaiter().GetResult();
            
            if (response == null || string.IsNullOrWhiteSpace(response.Command))
            {
                WriteError(new ErrorRecord(
                    new InvalidOperationException("Failed to get command suggestion from AI service"),
                    "NoCommandSuggestion",
                    ErrorCategory.InvalidOperation,
                    Prompt));
                return;
            }

            // Display the suggested command
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Suggested command: {response.Command}");
            Console.ResetColor();
            
            if (!string.IsNullOrWhiteSpace(response.Explanation))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Explanation: {response.Explanation}");
                Console.ResetColor();
            }

            // Execute based on confirmation setting
            bool shouldExecute = Confirm.IsPresent;
            
            if (!shouldExecute)
            {
                var choice = Host.UI.PromptForChoice(
                    "Execute Command",
                    $"Do you want to execute this command: {response.Command}?",
                    new Collection<ChoiceDescription>
                    {
                        new ChoiceDescription("&Yes", "Execute the command"),
                        new ChoiceDescription("&No", "Do not execute the command"),
                        new ChoiceDescription("&Copy", "Copy command to clipboard only")
                    },
                    1); // Default to No

                switch (choice)
                {
                    case 0: // Yes
                        shouldExecute = true;
                        break;
                    case 2: // Copy
                        CopyToClipboard(response.Command);
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine("Command copied to clipboard.");
                        Console.ResetColor();
                        return;
                    default: // No
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.WriteLine("Command not executed.");
                        Console.ResetColor();
                        return;
                }
            }

            if (shouldExecute)
            {
                WriteVerbose($"Executing command: {response.Command}");
                _commandExecutionService!.ExecuteCommand(response.Command);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error processing command assistance request");
            WriteError(new ErrorRecord(
                ex,
                "CmdAssistError",
                ErrorCategory.NotSpecified,
                Prompt));
        }
    }

    protected override void EndProcessing()
    {
        if (_serviceProvider is IDisposable disposableProvider)
        {
            disposableProvider.Dispose();
        }
        base.EndProcessing();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Add logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Warning);
        });

        // Add HTTP client
        services.AddHttpClient();

        // Add AI services
        services.AddSingleton<IAiService, AiService>();
        services.AddSingleton<IOpenAiService, OpenAiService>();
        services.AddSingleton<IAzureOpenAiService, AzureOpenAiService>();
        services.AddSingleton<IClaudeService, ClaudeService>();
        services.AddSingleton<ILlamaService, LlamaService>();
        services.AddSingleton<IGeminiService, GeminiService>();
        services.AddSingleton<IDeepSeekService, DeepSeekService>();

        // Add command execution service
        services.AddSingleton<ICommandExecutionService, PowerShellCommandExecutionService>();

        // Add configuration
        services.AddSingleton<IConfigurationService, ConfigurationService>();
    }

    private void CopyToClipboard(string text)
    {
        try
        {
            var script = $"Set-Clipboard -Value '{text.Replace("'", "''")}'";
            using var ps = System.Management.Automation.PowerShell.Create();
            ps.AddScript(script);
            ps.Invoke();
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to copy command to clipboard");
            WriteWarning("Failed to copy command to clipboard. You can manually copy it from above.");
        }
    }

    private static (string Name, bool IsWindows, bool IsLinux, bool IsMacOS) GetOperatingSystemInfo()
    {
        if (OperatingSystem.IsWindows())
        {
            return ("Windows", true, false, false);
        }
        else if (OperatingSystem.IsLinux())
        {
            return ("Linux", false, true, false);
        }
        else if (OperatingSystem.IsMacOS())
        {
            return ("macOS", false, false, true);
        }
        else
        {
            return (Environment.OSVersion.Platform.ToString(), false, false, false);
        }
    }

    private string GetShellInfo()
    {
        var psVersion = Host.Version;
        var psEdition = Environment.GetEnvironmentVariable("PSEdition") ?? "Unknown";
        return $"PowerShell {psVersion.Major}.{psVersion.Minor} ({psEdition})";
    }

    private static Dictionary<string, string> GetRelevantEnvironmentVariables()
    {
        var envVars = new Dictionary<string, string>();
        
        // Add OS-specific relevant environment variables
        if (OperatingSystem.IsWindows())
        {
            AddEnvVarIfExists(envVars, "USERPROFILE");
            AddEnvVarIfExists(envVars, "PROGRAMFILES");
            AddEnvVarIfExists(envVars, "PROGRAMFILES(X86)");
            AddEnvVarIfExists(envVars, "WINDIR");
        }
        else
        {
            AddEnvVarIfExists(envVars, "HOME");
            AddEnvVarIfExists(envVars, "USER");
            AddEnvVarIfExists(envVars, "SHELL");
        }
        
        AddEnvVarIfExists(envVars, "PATH");
        AddEnvVarIfExists(envVars, "TEMP");
        AddEnvVarIfExists(envVars, "TMP");
        
        return envVars;
    }

    private static void AddEnvVarIfExists(Dictionary<string, string> envVars, string varName)
    {
        var value = Environment.GetEnvironmentVariable(varName);
        if (!string.IsNullOrEmpty(value))
        {
            envVars[varName] = value;
        }
    }
}