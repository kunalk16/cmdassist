# CmdAssist.PowerShell.Tests

This project contains comprehensive unit tests for the CmdAssist.PowerShell library, with focus on testing the AiService class.

## Test Structure

### AiServiceTests.cs
Core unit tests for the `AiService` class covering:
- Constructor initialization
- Provider routing for all supported AI providers (OpenAI, AzureOpenAI, Claude, Llama, Gemini, DeepSeek)
- Exception handling and error scenarios
- Logging verification
- Null response handling
- Parameter validation

### AiServiceExtendedTests.cs
Extended scenarios and integration-style tests covering:
- Real-world usage patterns with complete CommandContext
- Windows and Linux environment contexts
- Different temperature and token settings
- Edge cases like empty prompts
- Multiple sequential calls

## Test Framework and Libraries

- **MSTest**: Main testing framework with [TestClass] and [TestMethod] attributes
- **Moq**: Mocking framework for dependencies
- **Microsoft.NET.Test.Sdk**: Test SDK for .NET
- **coverlet.collector**: Code coverage collection

## Running Tests

From the solution root:
```powershell
dotnet test
```

From the test project directory:
```powershell
dotnet test tests/CmdAssist.PowerShell.Tests/
```

With coverage:
```powershell
dotnet test --collect:"XPlat Code Coverage"
```

## Test Coverage

The tests aim to provide comprehensive coverage of:
- ✅ All code paths in AiService
- ✅ All supported AI providers
- ✅ Error handling scenarios
- ✅ Logging functionality
- ✅ Parameter validation
- ✅ Context handling for different operating systems
- ✅ Edge cases and boundary conditions

## Mocking Strategy

All dependencies are mocked using Moq:
- `IOpenAiService`, `IAzureOpenAiService`, `IClaudeService`
- `ILlamaService`, `IGeminiService`, `IDeepSeekService`
- `ILogger<AiService>`

This ensures tests are isolated, fast, and focused on the `AiService` logic specifically.