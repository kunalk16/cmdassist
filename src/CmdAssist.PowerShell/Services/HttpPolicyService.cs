// Copyright (c) 2026 Kunal Karmakar
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Net;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using Polly.Timeout;
using CmdAssist.PowerShell.Services.Interfaces;

namespace CmdAssist.PowerShell.Services;

/// <summary>
/// Service that provides HTTP resilience policies using Polly
/// </summary>
public class HttpPolicyService : IHttpPolicyService
{
    private readonly ILogger<HttpPolicyService> _logger;
    private readonly ResiliencePipeline<HttpResponseMessage> _aiHttpPolicy;

    public HttpPolicyService(ILogger<HttpPolicyService> logger)
    {
        _logger = logger;
        _aiHttpPolicy = CreateAiHttpPolicy();
    }

    public ResiliencePipeline<HttpResponseMessage> GetAiHttpPolicy()
    {
        return _aiHttpPolicy;
    }

    private ResiliencePipeline<HttpResponseMessage> CreateAiHttpPolicy()
    {
        var retryOptions = new RetryStrategyOptions<HttpResponseMessage>
        {
            ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                .Handle<HttpRequestException>()
                .Handle<TaskCanceledException>()
                .Handle<TimeoutRejectedException>()
                .HandleResult(response => IsTransientHttpFailure(response)),
            Delay = TimeSpan.FromSeconds(2),
            MaxRetryAttempts = 3,
            BackoffType = DelayBackoffType.Exponential,
            UseJitter = true,
            OnRetry = args =>
            {
                var exception = args.Outcome.Exception;
                var response = args.Outcome.Result;
                
                if (exception != null)
                {
                    _logger.LogWarning("HTTP request failed with exception on attempt {Attempt}: {Exception}", 
                        args.AttemptNumber + 1, exception.Message);
                }
                else if (response != null)
                {
                    _logger.LogWarning("HTTP request failed with status {StatusCode} on attempt {Attempt}", 
                        response.StatusCode, args.AttemptNumber + 1);
                }

                return default;
            }
        };

        var timeoutOptions = new TimeoutStrategyOptions
        {
            Timeout = TimeSpan.FromSeconds(60),
            OnTimeout = args =>
            {
                _logger.LogWarning("HTTP request timed out after {Timeout} seconds", 60);
                return default;
            }
        };

        return new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddRetry(retryOptions)
            .AddTimeout(timeoutOptions)
            .Build();
    }

    private static bool IsTransientHttpFailure(HttpResponseMessage response)
    {
        // Retry for 5xx server errors and specific 4xx errors that might be transient
        return response.StatusCode >= HttpStatusCode.InternalServerError ||
               response.StatusCode == HttpStatusCode.RequestTimeout ||
               response.StatusCode == HttpStatusCode.TooManyRequests;
    }
}