// ============================================================================
//  Project:        Shared.ErrorHandling.Tests
//  File:           StandardErrorPolicyTests.cs
//  Created:        2026-08-09
// ============================================================================
using AwesomeAssertions;

using Shared.Deterministic.Abstractions.Clock;
using Shared.ErrorHandling.Abstractions.Envelope;
using Shared.ErrorHandling.Abstractions.Policy;

using Xunit;

namespace Shared.ErrorHandling.Tests.Policy;

public sealed class StandardErrorPolicyTests
{
    private static ErrorEnvelope EnvelopeFor(ErrorCategory category) =>
        new("MOD-" + category, category, "user message", "dev message", "corr-1", "Module", DateTime.UtcNow);

    private static StandardErrorPolicy CreatePolicy(out CircuitBreaker breaker, ErrorRetryPolicy? retryPolicy = null)
    {
        breaker = new CircuitBreaker(CircuitBreakerPolicy.Standard, new FixedClock(DateTime.UtcNow));
        return new StandardErrorPolicy(retryPolicy ?? ErrorRetryPolicy.Standard, breaker);
    }

    [Fact]
    public void Evaluate_DependencyErrorFirstAttempt_RetriesWithBackoffDelay()
    {
        StandardErrorPolicy policy = CreatePolicy(out _);

        ErrorPolicyDecision decision = policy.Evaluate(EnvelopeFor(ErrorCategory.Dependency), attemptNumber: 1);

        decision.ShouldRetry.Should().BeTrue();
        decision.ShouldEscalate.Should().BeFalse();
        decision.RetryDelay.Should().BeGreaterThan(TimeSpan.Zero);
    }

    [Fact]
    public void Evaluate_DependencyErrorAttemptsExhausted_Escalates()
    {
        StandardErrorPolicy policy = CreatePolicy(out _, new ErrorRetryPolicy(2, RetryBackoffStrategy.Fixed, TimeSpan.FromMilliseconds(10)));

        ErrorPolicyDecision decision = policy.Evaluate(EnvelopeFor(ErrorCategory.Dependency), attemptNumber: 2);

        decision.ShouldRetry.Should().BeFalse();
        decision.ShouldEscalate.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_ValidationError_NeverRetriedAndNotEscalated()
    {
        StandardErrorPolicy policy = CreatePolicy(out _);

        ErrorPolicyDecision decision = policy.Evaluate(EnvelopeFor(ErrorCategory.Validation), attemptNumber: 1);

        decision.ShouldRetry.Should().BeFalse();
        decision.ShouldEscalate.Should().BeFalse();
    }

    [Fact]
    public void Evaluate_InternalError_NeverRetriedButAlwaysEscalated()
    {
        StandardErrorPolicy policy = CreatePolicy(out _);

        ErrorPolicyDecision decision = policy.Evaluate(EnvelopeFor(ErrorCategory.Internal), attemptNumber: 1);

        decision.ShouldRetry.Should().BeFalse();
        decision.ShouldEscalate.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_UnauthorizedError_NeverRetriedButAlwaysEscalated()
    {
        StandardErrorPolicy policy = CreatePolicy(out _);

        ErrorPolicyDecision decision = policy.Evaluate(EnvelopeFor(ErrorCategory.Unauthorized), attemptNumber: 1);

        decision.ShouldRetry.Should().BeFalse();
        decision.ShouldEscalate.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_CircuitOpen_FailsFastAndEscalatesRegardlessOfCategory()
    {
        StandardErrorPolicy policy = CreatePolicy(out CircuitBreaker breaker, new ErrorRetryPolicy(5, RetryBackoffStrategy.Fixed, TimeSpan.FromMilliseconds(10)));
        for (var i = 0; i < CircuitBreakerPolicy.Standard.FailureThreshold; i++)
        {
            breaker.RecordFailure();
        }

        ErrorPolicyDecision decision = policy.Evaluate(EnvelopeFor(ErrorCategory.Dependency), attemptNumber: 1);

        decision.ShouldRetry.Should().BeFalse();
        decision.ShouldEscalate.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_AttemptNumberZero_Throws()
    {
        StandardErrorPolicy policy = CreatePolicy(out _);

        Action act = () => policy.Evaluate(EnvelopeFor(ErrorCategory.Dependency), attemptNumber: 0);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
