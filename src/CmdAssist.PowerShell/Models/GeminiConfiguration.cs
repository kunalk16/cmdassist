// Copyright (c) 2026 Kunal Karmakar
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace CmdAssist.PowerShell.Models;

/// <summary>
/// Configuration for Google Gemini AI
/// </summary>
public class GeminiConfiguration
{
    /// <summary>
    /// API key for Gemini access
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Gemini API endpoint URL
    /// </summary>
    public string ApiUrl { get; set; } = "https://generativelanguage.googleapis.com/v1";
    
    /// <summary>
    /// Model name to use (e.g., gemini-pro, gemini-pro-vision)
    /// </summary>
    public string Model { get; set; } = "gemini-pro";
}