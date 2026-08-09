// ============================================================================
//  Project:        Shared.ErrorHandling
//  Module:         Abstractions.Policy
//  File:           ErrorRetryPolicy.cs
//  Created:        2026-08-09
//  Description:    Phase 2.3 deliverable: retry + backoff rules. Only
//                  ErrorCategory.Dependency errors are retryable by
//                  default (Validation/NotFound/Conflict/Unauthorized/
//                  Internal are never transient -- retrying them would
//                  just repeat the same failure).
// ============================================================================
namespace Shared.ErrorHandling.Abstractions.Policy;

/// <summary>Retry and backoff rules for a retryable error category.</summary>
public sealed record ErrorRetryPolicy(int MaxAttempts, RetryBackoffStrategy BackoffStrategy, TimeSpan BaseDelay)
{
    /// <summary>The standard retry policy: 3 attempts, exponential backoff starting at 200ms.</summary>
    public static ErrorRetryPolicy Standard { get; } = new(MaxAttempts: 3, RetryBackoffStrategy.Exponential, TimeSpan.FromMilliseconds(200));

    /// <summary>Computes the delay before the given attempt number (1-based; attempt 1 is the first retry, after the original call).</summary>
    public TimeSpan DelayForAttempt(int attemptNumber)
    {
        if (attemptNumber < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(attemptNumber), attemptNumber, "Attempt number must be >= 1.");
        }

        return BackoffStrategy switch
        {
            RetryBackoffStrategy.Fixed => BaseDelay,
            RetryBackoffStrategy.Exponential => BaseDelay * Math.Pow(2, attemptNumber - 1),
            _ => BaseDelay,
        };
    }
}
