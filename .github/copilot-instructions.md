# Copilot Instructions for CmdAssist

## Project Overview

CmdAssist is a PowerShell module (binary cmdlet) that provides AI-powered command line assistance. It translates natural language prompts into shell commands using multiple LLM providers (OpenAI, Azure OpenAI, Claude, Llama, Gemini, DeepSeek, Ollama).

- **Framework:** .NET 8.0 / C# (latest language version)
- **Target:** PowerShell 7.4+ binary module
- **Solution file:** `cmdassist.sln`

## Build & Test Commands

```powershell
# Build
dotnet build

# Run tests
dotnet test

# Build release
dotnet build -c Release

# Full build + import + test
.\build.ps1 -Clean -Import -Test

# Run tests with coverage
.\run-tests.ps1 -Coverage
```

## Project Structure

```
src/CmdAssist.PowerShell/
├── Cmdlets/          # PowerShell cmdlet (Invoke-CmdAssist / cmd-assist alias)
├── Services/         # AI provider implementations + supporting services
│   └── Interfaces/   # Service interfaces (IAiService, I<Provider>Service, etc.)
├── Models/           # Data models, DTOs, configuration classes, enums
├── Constants/        # Prompt templates and OS-specific guidance
├── CmdAssist.psd1    # PowerShell module manifest
└── CmdAssist.PowerShell.csproj

tests/CmdAssist.PowerShell.Tests/
├── AiServiceTests.cs
├── AiServiceExtendedTests.cs
└── GlobalUsings.cs
```

## Coding Conventions

### C# Style

- **Nullable reference types** are enabled globally — use `?` for nullable references and handle nullability properly.
- **Implicit usings** are enabled — do not add redundant `using` statements for System, System.Collections.Generic, System.Linq, System.Threading.Tasks, etc.
- **Warnings as errors** is enabled (`TreatWarningsAsErrors`). All code must compile without warnings.
- Use the **latest C# language features** (required members, file-scoped namespaces, pattern matching, switch expressions, etc.).
- Use `required` keyword on mandatory properties in request/response models.
- Initialize properties with sensible defaults where appropriate.

### Naming

- **Classes:** PascalCase (e.g., `OpenAiService`, `AiResponse`).
- **Interfaces:** Prefix with `I` (e.g., `IAiService`, `IOpenAiService`).
- **Test classes:** `<ClassUnderTest>Tests` (e.g., `AiServiceTests`).
- **Test methods:** `MethodName_Scenario_ExpectedResult` (e.g., `GetCommandSuggestionAsync_OpenAi_CallsOpenAiService`).
- **PowerShell cmdlets:** Verb-Noun format (e.g., `Invoke-CmdAssist`).
- **Configuration models:** `<ProviderName>Configuration` (e.g., `ClaudeConfiguration`).

### Architecture Patterns

- **Service routing:** `AiService` acts as a router, dispatching to provider-specific services using a switch expression on `AiProvider` enum.
- **Dependency injection:** Uses `Microsoft.Extensions.DependencyInjection`; services registered as singletons in the cmdlet's `BeginProcessing()`.
- **HTTP resilience:** Polly policies (retry + circuit breaker) applied via `HttpPolicyService`.
- **Async wrapping:** All AI service calls are async (`Task<AiResponse?>`); the cmdlet bridges to sync via `.GetAwaiter().GetResult()`.
- **Configuration:** Environment variables read via `ConfigurationService` into strongly-typed configuration objects.

### AI Service Implementation

Each AI provider follows a consistent pattern:

1. **Interface:** `I<ProviderName>Service` in `Services/Interfaces/` with `Task<AiResponse?> GetCommandSuggestionAsync(AiRequest request)`.
2. **Implementation:** `<ProviderName>Service` in `Services/` — uses `HttpClient` to call the provider API, serializes/deserializes with `System.Text.Json`.
3. **Configuration model:** `<ProviderName>Configuration` in `Models/` — holds API key, endpoint URL, model name, etc.
4. **Registration:** Added to `AiConfiguration`, DI container in the cmdlet, and the `AiService` switch expression.
5. **Enum value:** Added to `AiProvider` enum and the cmdlet's `ValidateSet` attribute.

### Error Handling

- AI services throw exceptions with descriptive messages (e.g., missing API key, failed HTTP request).
- The cmdlet catches exceptions and writes them via `PSCmdlet.WriteError()` with structured `ErrorRecord` objects.
- Use `WriteVerbose()` and `WriteDebug()` for diagnostics — never `Console.Write`.

### Prompt Engineering

- System prompts are defined in `Constants/PromptConstants.cs`.
- Prompts include OS-specific guidance and strongly prefer PowerShell cmdlets over native OS commands.
- The `CommandContext` model provides OS, shell version, and working directory to the AI.

## Testing Conventions

- **Framework:** MSTest (`[TestClass]`, `[TestMethod]`).
- **Mocking:** Moq library — mock all external dependencies (HTTP clients, services).
- **Data-driven tests:** Use `[DataRow]` for parameterized tests (e.g., testing each AI provider via the router).
- **Assertions:** Use `Assert.IsNotNull`, `Assert.AreEqual`, `Assert.ThrowsExceptionAsync`, and Moq's `Verify`.
- All new services and significant logic changes should have corresponding tests.

## Package Management

- **Central Package Management** is enabled via `Directory.Packages.props` — add package versions there, not in individual `.csproj` files.
- Key dependencies: `Microsoft.PowerShell.SDK`, `Microsoft.Extensions.*`, `System.Text.Json`, `Polly`.
- Test dependencies: `MSTest`, `Moq`, `coverlet.collector`.

## CI/CD

- **PR builds:** Triggered on PRs to `main` — restore, build, and test on `ubuntu-latest`.
- **Publishing:** Manual `workflow_dispatch` that builds and publishes to PSGallery via `publish.ps1`.
- The .NET SDK version is pinned in `global.json` with `rollForward: latestMajor`.

## Adding a New AI Provider

1. Add a new value to the `AiProvider` enum in `Models/`.
2. Create `<ProviderName>Configuration.cs` in `Models/`.
3. Create `I<ProviderName>Service.cs` in `Services/Interfaces/`.
4. Create `<ProviderName>Service.cs` in `Services/` implementing the interface.
5. Add the configuration property to `AiConfiguration`.
6. Register the service in `InvokeCmdAssistCmdlet.ConfigureServices()`.
7. Add the routing case to `AiService`'s switch expression.
8. Update the `ValidateSet` attribute on the cmdlet's `Provider` parameter.
9. Add environment variable documentation to `README.md`.
10. Add tests covering the new provider's routing and service logic.
