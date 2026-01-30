# CmdAssist - AI-Powered Command Line Assistant

CmdAssist is a PowerShell module that provides AI-powered command line assistance. It helps users by suggesting appropriate commands based on natural language prompts using various Large Language Models (LLMs).

## Features

- 🤖 **Multiple AI Providers**: Support for OpenAI, Azure OpenAI, Claude, and Llama
- ⚡ **PowerShell Integration**: Native PowerShell cmdlet with proper parameter support
- 🛡️ **Safety First**: Confirmation prompts before executing potentially dangerous commands
- 📋 **Clipboard Support**: Copy commands to clipboard without execution
- 🔧 **Context Aware**: Uses current working directory and system information
- 📊 **Confidence Scoring**: AI responses include confidence levels

## Installation

### Prerequisites

- .NET 8.0 SDK or later
- PowerShell 5.1 or later

### Build from Source

1. Clone the repository:
```powershell
git clone https://github.com/cmdassist/cmdassist.git
cd cmdassist
```

2. Build the project:
```powershell
dotnet build
```

3. Import the module:
```powershell
Import-Module ./src/CmdAssist.PowerShell/bin/Debug/net8.0/CmdAssist.psd1
```

## Configuration

CmdAssist uses environment variables for API configuration. Set the appropriate variables for your chosen AI provider:

### OpenAI
```powershell
$env:OPENAI_API_KEY = "your-openai-api-key"
$env:OPENAI_MODEL = "gpt-4"  # Optional, defaults to gpt-4
$env:OPENAI_ORGANIZATION = "your-org-id"  # Optional
```

### Azure OpenAI
```powershell
$env:AZURE_OPENAI_API_KEY = "your-azure-openai-key"
$env:AZURE_OPENAI_ENDPOINT = "https://your-resource.openai.azure.com/"
$env:AZURE_OPENAI_DEPLOYMENT_NAME = "your-deployment-name"
$env:AZURE_OPENAI_API_VERSION = "2024-02-01"  # Optional
```

### Claude
```powershell
$env:CLAUDE_API_KEY = "your-claude-api-key"
$env:CLAUDE_MODEL = "claude-3-sonnet-20240229"  # Optional
```

### Llama
```powershell
$env:LLAMA_API_URL = "https://your-llama-endpoint.com"
$env:LLAMA_API_KEY = "your-api-key"  # Optional, depending on your setup
$env:LLAMA_MODEL = "llama2-70b-chat"  # Optional
```

## Usage

### Basic Usage

```powershell
# Get a command suggestion with confirmation prompt
cmd-assist "list all .txt files in the current directory"

# Execute directly without confirmation
cmd-assist "create a new folder called 'documents'" -Confirm

# Use a specific AI provider
cmd-assist "show disk usage" -Provider Claude

# Enable verbose output
cmd-assist "find large files" -Verbose
```

### Examples

```powershell
# File operations
cmd-assist "copy all .pdf files to a backup folder"
cmd-assist "find files modified in the last 7 days"
cmd-assist "compress the logs folder"

# System administration
cmd-assist "check system memory usage"
cmd-assist "restart the print spooler service"
cmd-assist "show network connections"

# Development tasks
cmd-assist "initialize a git repository"
cmd-assist "build a .NET project in release mode"
cmd-assist "run unit tests and show coverage"
```

### Interactive Mode

When you run a command without the `-Confirm` switch, CmdAssist will:

1. Display the suggested command
2. Show an explanation of what it does
3. Ask for confirmation with options:
   - **Yes**: Execute the command
   - **No**: Cancel execution
   - **Copy**: Copy the command to clipboard without executing

## Command Line Parameters

- `Prompt` (Position 0, Mandatory): The natural language description of what you want to accomplish
- `Confirm` (Switch): Execute the command without confirmation prompt
- `Provider` (Optional): Specify AI provider (OpenAI, AzureOpenAI, Claude, Llama)
- `Verbose` (Switch): Enable verbose output for debugging

## Safety Features

- **Confirmation Prompts**: By default, all commands require user confirmation
- **Confidence Scoring**: Low-confidence responses trigger additional warnings
- **Context Awareness**: Commands are generated with awareness of your current environment
- **Error Handling**: Comprehensive error handling with helpful messages

## Troubleshooting

### Common Issues

1. **API Key Not Set**
   ```
   Error: OpenAI API key not configured. Set the OPENAI_API_KEY environment variable.
   ```
   Solution: Set the appropriate environment variable for your chosen provider.

2. **Network Connection Issues**
   ```
   Error: API request failed: Unauthorized
   ```
   Solution: Verify your API key and network connection.

3. **Module Import Errors**
   ```
   Import-Module: Could not load file or assembly
   ```
   Solution: Ensure .NET 8.0 runtime is installed and try rebuilding the project.

### Enable Debug Logging

```powershell
cmd-assist "your prompt" -Verbose
```

## Development

### Project Structure

```
cmdassist/
├── src/
│   └── CmdAssist.PowerShell/
│       ├── Cmdlets/                 # PowerShell cmdlet implementations
│       ├── Services/                # AI service implementations
│       ├── Models/                  # Data models and DTOs
│       ├── CmdAssist.psd1          # PowerShell module manifest
│       └── CmdAssist.PowerShell.csproj
├── Directory.Build.props            # Common build properties
├── Directory.Packages.props         # Package version management
├── global.json                      # .NET SDK version
└── cmdassist.sln                   # Solution file
```

### Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

### Running Tests

```powershell
dotnet test
```

### Building Release Version

```powershell
dotnet build -c Release
```

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## Support

- 📚 [Documentation](https://github.com/cmdassist/cmdassist/wiki)
- 🐛 [Issues](https://github.com/cmdassist/cmdassist/issues)
- 💬 [Discussions](https://github.com/cmdassist/cmdassist/discussions)

---

**⚠️ Important**: Always review generated commands before execution, especially when using the `-Confirm` switch. CmdAssist is a tool to assist with command generation, but you remain responsible for the commands you choose to execute.