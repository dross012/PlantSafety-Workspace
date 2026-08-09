// ============================================================================
//  Project:        Shared.ErrorHandling
//  Module:         Abstractions.Policy
//  File:           StandardErrorPolicy.cs
//  Created:        2026-08-09
//  Description:    Phase 2.3 deliverable: the standard error policy
//                  engine. Rules, in order:
//                    1. Circuit open -> fail fast, escalate, no retry.
//                    2. Category isn't Dependency (the only transient
//                       category) -> never retried; Internal/Unauthorized
//                       escalate immediately, the rest don't.
//                    3. Retry attempts exhausted -> escalate.
//                    4. Otherwise -> retry, with backoff-computed delay.
// ============================================================================
using Shared.ErrorHandling.Abstractions.Envelope;

namespace Shared.ErrorHandling.Abstractions.Policy;

/// <summary>The standard error policy: combines retry/backoff/circuit-breaker/escalation rules.</summary>
public sealed class StandardErrorPolicy(ErrorRetryPolicy retryPolicy, CircuitBreaker circuitBreaker) : IErrorPolicy
{
    /// <inheritdoc />
    public ErrorPolicyDecision Evaluate(ErrorEnvelope error, int attemptNumber)
    {
        if (attemptNumber < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(attemptNumber), attemptNumber, "Attempt number must be >= 1.");
        }

        if (circuitBreaker.State == CircuitState.Open)
        {
            return new ErrorPolicyDecision(false, TimeSpan.Zero, true, "Circuit breaker is open; failing fast and escalating without retry.");
        }

        if (error.Category != ErrorCategory.Dependency)
        {
            bool escalateImmediately = error.Category is ErrorCategory.Internal or ErrorCategory.Unauthorized;
            string reason = escalateImmediately
                ? $"{error.Category} errors are never retried and are always escalated."
                : $"{error.Category} errors are not retried.";
            return new ErrorPolicyDecision(false, TimeSpan.Zero, escalateImmediately, reason);
        }

        if (attemptNumber >= retryPolicy.MaxAttempts)
        {
            return new ErrorPolicyDecision(false, TimeSpan.Zero, true, $"Retry attempts exhausted ({attemptNumber}/{retryPolicy.MaxAttempts}); escalating.");
        }

        return new ErrorPolicyDecision(true, retryPolicy.DelayForAttempt(attemptNumber), false, $"Retrying (attempt {attemptNumber + 1} of {retryPolicy.MaxAttempts}).");
    }
}
