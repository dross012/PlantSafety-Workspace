// ============================================================================
//  Project:        Shared.ErrorHandling.Tests
//  File:           ErrorRetryPolicyTests.cs
//  Created:        2026-08-09
// ============================================================================
using AwesomeAssertions;

using Shared.ErrorHandling.Abstractions.Policy;

using Xunit;

namespace Shared.ErrorHandling.Tests.Policy;

public sealed class ErrorRetryPolicyTests
{
    [Fact]
    public void DelayForAttempt_Fixed_ReturnsTheSameDelayEveryAttempt()
    {
        var policy = new ErrorRetryPolicy(MaxAttempts: 3, RetryBackoffStrategy.Fixed, TimeSpan.FromMilliseconds(100));

        policy.DelayForAttempt(1).Should().Be(TimeSpan.FromMilliseconds(100));
        policy.DelayForAttempt(3).Should().Be(TimeSpan.FromMilliseconds(100));
    }

    [Fact]
    public void DelayForAttempt_Exponential_Doubles()
    {
        var policy = new ErrorRetryPolicy(MaxAttempts: 4, RetryBackoffStrategy.Exponential, TimeSpan.FromMilliseconds(100));

        policy.DelayForAttempt(1).Should().Be(TimeSpan.FromMilliseconds(100));
        policy.DelayForAttempt(2).Should().Be(TimeSpan.FromMilliseconds(200));
        policy.DelayForAttempt(3).Should().Be(TimeSpan.FromMilliseconds(400));
    }

    [Fact]
    public void DelayForAttempt_AttemptZero_Throws()
    {
        var policy = ErrorRetryPolicy.Standard;

        Action act = () => policy.DelayForAttempt(0);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
