// Copyright (c) 2026 Kunal Karmakar
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace CmdAssist.PowerShell.Models;

/// <summary>
/// Response model from AI service
/// </summary>
public class AiResponse
{
    public required string Command { get; set; }
    public string? Explanation { get; set; }
    public double Confidence { get; set; }
    public string? Warning { get; set; }
}