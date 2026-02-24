// Copyright (c) 2026 Kunal Karmakar
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace CmdAssist.PowerShell.Tests.Services;

/// <summary>
/// Extended unit tests for AiService with realistic scenarios
/// </summary>
[TestClass]
public class AiServiceExtendedTests
{
    private readonly Mock<IOpenAiService> _mockOpenAiService;
    private readonly Mock<IAzureOpenAiService> _mockAzureOpenAiService;
    private readonly Mock<IClaudeService> _mockClaudeService;
    private readonly Mock<ILlamaService> _mockLlamaService;
    private readonly Mock<IGeminiService> _mockGeminiService;
    private readonly Mock<IDeepSeekService> _mockDeepSeekService;
    private readonly Mock<IOllamaService> _mockOllamaService;
    private readonly Mock<ILogger<AiService>> _mockLogger;
    private readonly AiService _aiService;

    public AiServiceExtendedTests()
    {
        _mockOpenAiService = new Mock<IOpenAiService>();
        _mockAzureOpenAiService = new Mock<IAzureOpenAiService>();
        _mockClaudeService = new Mock<IClaudeService>();
        _mockLlamaService = new Mock<ILlamaService>();
        _mockGeminiService = new Mock<IGeminiService>();
        _mockDeepSeekService = new Mock<IDeepSeekService>();
        _mockOllamaService = new Mock<IOllamaService>();
        _mockLogger = new Mock<ILogger<AiService>>();

        _aiService = new AiService(
            _mockOpenAiService.Object,
            _mockAzureOpenAiService.Object,
            _mockClaudeService.Object,
            _mockLlamaService.Object,
            _mockGeminiService.Object,
            _mockDeepSeekService.Object,
            _mockOllamaService.Object,
            _mockLogger.Object);
    }

    [TestMethod]
    public async Task GetCommandSuggestionAsync_WithWindowsContext_ShouldPassCorrectContext()
    {
        // Arrange
        var windowsContext = new CommandContext
        {
            OperatingSystem = "Windows 11",
            Shell = "PowerShell",
            WorkingDirectory = @"C:\Users\Test",
            IsWindows = true,
            IsLinux = false,
            IsMacOS = false,
            EnvironmentVariables = new Dictionary<string, string>
            {
                ["PATH"] = @"C:\Windows\System32;C:\Windows",
                ["USERPROFILE"] = @"C:\Users\Test"
            }
        };

        var request = new AiRequest
        {
            Prompt = "List all files in current directory",
            Provider = AiProvider.AzureOpenAI,
            Context = windowsContext,
            Temperature = 0.2,
            MaxTokens = 300
        };

        var expectedResponse = new AiResponse
        {
            Command = "Get-ChildItem",
            Explanation = "Lists all files and folders in the current directory",
            Confidence = 0.95
        };

        _mockAzureOpenAiService.Setup(x => x.GetCommandSuggestionAsync(It.Is<AiRequest>(r =>
            r.Context != null && 
            r.Context.IsWindows &&
            r.Context.OperatingSystem == "Windows 11" &&
            r.Context.Shell == "PowerShell")))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _aiService.GetCommandSuggestionAsync(request);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Get-ChildItem", result.Command);
        Assert.AreEqual(0.95, result.Confidence);
        _mockAzureOpenAiService.Verify(x => x.GetCommandSuggestionAsync(It.IsAny<AiRequest>()), Times.Once);
    }

    [TestMethod]
    public async Task GetCommandSuggestionAsync_WithLinuxContext_ShouldPassCorrectContext()
    {
        // Arrange
        var linuxContext = new CommandContext
        {
            OperatingSystem = "Ubuntu 22.04",
            Shell = "bash",
            WorkingDirectory = "/home/user",
            IsWindows = false,
            IsLinux = true,
            IsMacOS = false,
            EnvironmentVariables = new Dictionary<string, string>
            {
                ["PATH"] = "/usr/local/bin:/usr/bin:/bin",
                ["HOME"] = "/home/user"
            }
        };

        var request = new AiRequest
        {
            Prompt = "Find large files",
            Provider = AiProvider.Claude,
            Context = linuxContext
        };

        var expectedResponse = new AiResponse
        {
            Command = "find / -type f -size +100M 2>/dev/null",
            Explanation = "Finds files larger than 100MB",
            Confidence = 0.88,
            Warning = "This command may take a long time to complete"
        };

        _mockClaudeService.Setup(x => x.GetCommandSuggestionAsync(It.Is<AiRequest>(r =>
            r.Context != null &&
            r.Context.IsLinux &&
            r.Context.OperatingSystem == "Ubuntu 22.04" &&
            r.Context.Shell == "bash")))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _aiService.GetCommandSuggestionAsync(request);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("find / -type f -size +100M 2>/dev/null", result.Command);
        Assert.AreEqual("This command may take a long time to complete", result.Warning);
        _mockClaudeService.Verify(x => x.GetCommandSuggestionAsync(It.IsAny<AiRequest>()), Times.Once);
    }

    [TestMethod]
    [DataRow(0.0)]
    [DataRow(0.5)]
    [DataRow(1.0)]
    public async Task GetCommandSuggestionAsync_WithDifferentTemperatures_ShouldPreserveTemperature(double temperature)
    {
        // Arrange
        var request = new AiRequest
        {
            Prompt = "Test prompt",
            Provider = AiProvider.Gemini,
            Temperature = temperature
        };

        var expectedResponse = new AiResponse
        {
            Command = "test-command",
            Confidence = 0.9
        };

        _mockGeminiService.Setup(x => x.GetCommandSuggestionAsync(It.Is<AiRequest>(r =>
            Math.Abs(r.Temperature - temperature) < 0.001)))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _aiService.GetCommandSuggestionAsync(request);

        // Assert
        Assert.IsNotNull(result);
        _mockGeminiService.Verify(x => x.GetCommandSuggestionAsync(It.IsAny<AiRequest>()), Times.Once);
    }

    [TestMethod]
    [DataRow(100)]
    [DataRow(500)]
    [DataRow(1000)]
    [DataRow(2000)]
    public async Task GetCommandSuggestionAsync_WithDifferentMaxTokens_ShouldPreserveMaxTokens(int maxTokens)
    {
        // Arrange
        var request = new AiRequest
        {
            Prompt = "Test prompt",
            Provider = AiProvider.DeepSeek,
            MaxTokens = maxTokens
        };

        var expectedResponse = new AiResponse
        {
            Command = "test-command",
            Confidence = 0.85
        };

        _mockDeepSeekService.Setup(x => x.GetCommandSuggestionAsync(It.Is<AiRequest>(r =>
            r.MaxTokens == maxTokens)))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _aiService.GetCommandSuggestionAsync(request);

        // Assert
        Assert.IsNotNull(result);
        _mockDeepSeekService.Verify(x => x.GetCommandSuggestionAsync(It.IsAny<AiRequest>()), Times.Once);
    }

    [TestMethod]
    public async Task GetCommandSuggestionAsync_WithEmptyPrompt_ShouldStillCallService()
    {
        // Arrange
        var request = new AiRequest
        {
            Prompt = "",
            Provider = AiProvider.Llama
        };

        var expectedResponse = new AiResponse
        {
            Command = "# No command generated",
            Explanation = "Prompt was empty",
            Confidence = 0.0,
            Warning = "Please provide a more specific prompt"
        };

        _mockLlamaService.Setup(x => x.GetCommandSuggestionAsync(It.Is<AiRequest>(r =>
            string.IsNullOrEmpty(r.Prompt))))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _aiService.GetCommandSuggestionAsync(request);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("# No command generated", result.Command);
        Assert.IsTrue(result.Explanation!.Contains("empty"));
        _mockLlamaService.Verify(x => x.GetCommandSuggestionAsync(It.IsAny<AiRequest>()), Times.Once);
    }

    [TestMethod]
    public async Task GetCommandSuggestionAsync_MultipleCallsWithSameProvider_ShouldCallServiceMultipleTimes()
    {
        // Arrange
        var request1 = new AiRequest { Prompt = "Test 1", Provider = AiProvider.OpenAI };
        var request2 = new AiRequest { Prompt = "Test 2", Provider = AiProvider.OpenAI };

        var response1 = new AiResponse { Command = "command1", Confidence = 0.9 };
        var response2 = new AiResponse { Command = "command2", Confidence = 0.8 };

        _mockOpenAiService.SetupSequence(x => x.GetCommandSuggestionAsync(It.IsAny<AiRequest>()))
            .ReturnsAsync(response1)
            .ReturnsAsync(response2);

        // Act
        var result1 = await _aiService.GetCommandSuggestionAsync(request1);
        var result2 = await _aiService.GetCommandSuggestionAsync(request2);

        // Assert
        Assert.IsNotNull(result1);
        Assert.IsNotNull(result2);
        Assert.AreEqual("command1", result1.Command);
        Assert.AreEqual("command2", result2.Command);
        _mockOpenAiService.Verify(x => x.GetCommandSuggestionAsync(It.IsAny<AiRequest>()), Times.Exactly(2));
    }
}