// Copyright (c) 2026 Kunal Karmakar
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace CmdAssist.PowerShell.Tests.Services;

/// <summary>
/// Unit tests for AiService
/// </summary>
[TestClass]
public class AiServiceTests
{
    private readonly Mock<IOpenAiService> _mockOpenAiService;
    private readonly Mock<IAzureOpenAiService> _mockAzureOpenAiService;
    private readonly Mock<IClaudeService> _mockClaudeService;
    private readonly Mock<ILlamaService> _mockLlamaService;
    private readonly Mock<IGeminiService> _mockGeminiService;
    private readonly Mock<IDeepSeekService> _mockDeepSeekService;
    private readonly Mock<ILogger<AiService>> _mockLogger;
    private readonly AiService _aiService;

    public AiServiceTests()
    {
        _mockOpenAiService = new Mock<IOpenAiService>();
        _mockAzureOpenAiService = new Mock<IAzureOpenAiService>();
        _mockClaudeService = new Mock<IClaudeService>();
        _mockLlamaService = new Mock<ILlamaService>();
        _mockGeminiService = new Mock<IGeminiService>();
        _mockDeepSeekService = new Mock<IDeepSeekService>();
        _mockLogger = new Mock<ILogger<AiService>>();

        _aiService = new AiService(
            _mockOpenAiService.Object,
            _mockAzureOpenAiService.Object,
            _mockClaudeService.Object,
            _mockLlamaService.Object,
            _mockGeminiService.Object,
            _mockDeepSeekService.Object,
            _mockLogger.Object);
    }

    [TestMethod]
    public void Constructor_WithAllDependencies_ShouldCreateInstance()
    {
        // Arrange & Act
        var service = new AiService(
            _mockOpenAiService.Object,
            _mockAzureOpenAiService.Object,
            _mockClaudeService.Object,
            _mockLlamaService.Object,
            _mockGeminiService.Object,
            _mockDeepSeekService.Object,
            _mockLogger.Object);

        // Assert
        Assert.IsNotNull(service);
    }

    [TestMethod]
    [DataRow(AiProvider.OpenAI)]
    [DataRow(AiProvider.AzureOpenAI)]
    [DataRow(AiProvider.Claude)]
    [DataRow(AiProvider.Llama)]
    [DataRow(AiProvider.Gemini)]
    [DataRow(AiProvider.DeepSeek)]
    public async Task GetCommandSuggestionAsync_WithValidProvider_ShouldRouteToCorrectService(AiProvider provider)
    {
        // Arrange
        var request = new AiRequest 
        { 
            Prompt = "Test prompt", 
            Provider = provider 
        };

        var expectedResponse = new AiResponse
        {
            Command = "test command",
            Explanation = "test explanation",
            Confidence = 0.9
        };

        // Setup the specific service mock based on provider
        switch (provider)
        {
            case AiProvider.OpenAI:
                _mockOpenAiService.Setup(x => x.GetCommandSuggestionAsync(request))
                    .ReturnsAsync(expectedResponse);
                break;
            case AiProvider.AzureOpenAI:
                _mockAzureOpenAiService.Setup(x => x.GetCommandSuggestionAsync(request))
                    .ReturnsAsync(expectedResponse);
                break;
            case AiProvider.Claude:
                _mockClaudeService.Setup(x => x.GetCommandSuggestionAsync(request))
                    .ReturnsAsync(expectedResponse);
                break;
            case AiProvider.Llama:
                _mockLlamaService.Setup(x => x.GetCommandSuggestionAsync(request))
                    .ReturnsAsync(expectedResponse);
                break;
            case AiProvider.Gemini:
                _mockGeminiService.Setup(x => x.GetCommandSuggestionAsync(request))
                    .ReturnsAsync(expectedResponse);
                break;
            case AiProvider.DeepSeek:
                _mockDeepSeekService.Setup(x => x.GetCommandSuggestionAsync(request))
                    .ReturnsAsync(expectedResponse);
                break;
        }

        // Act
        var result = await _aiService.GetCommandSuggestionAsync(request);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedResponse.Command, result.Command);
        Assert.AreEqual(expectedResponse.Explanation, result.Explanation);
        Assert.AreEqual(expectedResponse.Confidence, result.Confidence);

        // Verify the correct service was called
        switch (provider)
        {
            case AiProvider.OpenAI:
                _mockOpenAiService.Verify(x => x.GetCommandSuggestionAsync(request), Times.Once);
                break;
            case AiProvider.AzureOpenAI:
                _mockAzureOpenAiService.Verify(x => x.GetCommandSuggestionAsync(request), Times.Once);
                break;
            case AiProvider.Claude:
                _mockClaudeService.Verify(x => x.GetCommandSuggestionAsync(request), Times.Once);
                break;
            case AiProvider.Llama:
                _mockLlamaService.Verify(x => x.GetCommandSuggestionAsync(request), Times.Once);
                break;
            case AiProvider.Gemini:
                _mockGeminiService.Verify(x => x.GetCommandSuggestionAsync(request), Times.Once);
                break;
            case AiProvider.DeepSeek:
                _mockDeepSeekService.Verify(x => x.GetCommandSuggestionAsync(request), Times.Once);
                break;
        }

        // Verify logging
        VerifyInformationLog($"Processing AI request for provider: {provider}");
    }

    [TestMethod]
    public async Task GetCommandSuggestionAsync_WithUnsupportedProvider_ShouldThrowArgumentException()
    {
        // Arrange
        var request = new AiRequest 
        { 
            Prompt = "Test prompt", 
            Provider = (AiProvider)999 // Invalid provider
        };

        // Act & Assert
        var exception = await Assert.ThrowsExceptionAsync<ArgumentException>(() => 
            _aiService.GetCommandSuggestionAsync(request));

        Assert.IsTrue(exception.Message.Contains("Unsupported AI provider"));
    }

    [TestMethod]
    public async Task GetCommandSuggestionAsync_WhenServiceThrowsException_ShouldLogErrorAndRethrow()
    {
        // Arrange
        var request = new AiRequest 
        { 
            Prompt = "Test prompt", 
            Provider = AiProvider.OpenAI 
        };

        var expectedException = new InvalidOperationException("Service error");
        _mockOpenAiService.Setup(x => x.GetCommandSuggestionAsync(request))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var actualException = await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => 
            _aiService.GetCommandSuggestionAsync(request));

        Assert.AreSame(expectedException, actualException);

        // Verify error logging
        VerifyErrorLog("Error getting command suggestion from OpenAI", expectedException);
    }

    [TestMethod]
    public async Task GetCommandSuggestionAsync_WithNullResponse_ShouldReturnNull()
    {
        // Arrange
        var request = new AiRequest 
        { 
            Prompt = "Test prompt", 
            Provider = AiProvider.OpenAI 
        };

        _mockOpenAiService.Setup(x => x.GetCommandSuggestionAsync(request))
            .ReturnsAsync((AiResponse?)null);

        // Act
        var result = await _aiService.GetCommandSuggestionAsync(request);

        // Assert
        Assert.IsNull(result);
        _mockOpenAiService.Verify(x => x.GetCommandSuggestionAsync(request), Times.Once);
    }

    [TestMethod]
    public async Task GetCommandSuggestionAsync_WithCompleteRequest_ShouldPassAllProperties()
    {
        // Arrange
        var request = new AiRequest 
        { 
            Prompt = "Test prompt",
            Provider = AiProvider.Claude,
            Temperature = 0.5,
            MaxTokens = 1000,
            Context = new CommandContext()
        };

        var expectedResponse = new AiResponse
        {
            Command = "test command",
            Explanation = "test explanation",
            Confidence = 0.8,
            Warning = "test warning"
        };

        _mockClaudeService.Setup(x => x.GetCommandSuggestionAsync(It.Is<AiRequest>(r => 
            r.Prompt == request.Prompt &&
            r.Provider == request.Provider &&
            r.Temperature == request.Temperature &&
            r.MaxTokens == request.MaxTokens &&
            r.Context == request.Context)))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _aiService.GetCommandSuggestionAsync(request);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedResponse.Command, result.Command);
        Assert.AreEqual(expectedResponse.Explanation, result.Explanation);
        Assert.AreEqual(expectedResponse.Confidence, result.Confidence);
        Assert.AreEqual(expectedResponse.Warning, result.Warning);
    }

    private void VerifyInformationLog(string expectedMessage)
    {
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(expectedMessage)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    private void VerifyErrorLog(string expectedMessage, Exception expectedException)
    {
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(expectedMessage)),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}