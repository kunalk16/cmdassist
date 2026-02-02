// Copyright (c) 2026 Kunal Karmakar
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace CmdAssist.PowerShell.Models;

/// <summary>
/// Configuration for DeepSeek AI
/// </summary>
public class DeepSeekConfiguration
{
    /// <summary>
    /// API key for DeepSeek access
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;
    
    /// <summary>
    /// DeepSeek API endpoint URL
    /// </summary>
    public string ApiUrl { get; set; } = "https://api.deepseek.com/v1";
    
    /// <summary>
    /// Model name to use (e.g., deepseek-chat, deepseek-coder)
    /// </summary>
    public string Model { get; set; } = "deepseek-chat";
}