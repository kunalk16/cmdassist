// Copyright (c) 2026 Kunal Karmakar
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Polly;

namespace CmdAssist.PowerShell.Services.Interfaces;

/// <summary>
/// Service for providing HTTP resilience policies
/// </summary>
public interface IHttpPolicyService
{
    /// <summary>
    /// Gets a combined retry and timeout policy for AI HTTP requests
    /// </summary>
    /// <returns>A Polly resilience pipeline for HTTP requests</returns>
    ResiliencePipeline<HttpResponseMessage> GetAiHttpPolicy();
}