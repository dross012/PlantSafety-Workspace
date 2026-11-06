// ============================================================================
//  Project:        Shared.Scheduling.Tests
//  File:           SchedulingDueTimeEvaluatorTests.cs
//  Created:        2026-08-10
// ============================================================================
using FluentAssertions;

using Shared.Scheduling.Abstractions.DueTime;

using Xunit;

namespace Shared.Scheduling.Tests.DueTime;

public sealed class SchedulingDueTimeEvaluatorTests
{
    private static readonly DateTime AsOfUtc = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void EvaluateStatus_NoDueTimes_ReturnsOnTrack()
    {
        SchedulingDueTimeStatus status = SchedulingDueTimeEvaluator.EvaluateStatus(Array.Empty<ScheduledDueTime>(), AsOfUtc);

        status.Should().Be(SchedulingDueTimeStatus.OnTrack);
    }

    [Fact]
    public void EvaluateStatus_AllFarInFuture_ReturnsOnTrack()
    {
        var dueTimes = new[] { new ScheduledDueTime("ACT-1", "Action", AsOfUtc.AddHours(48), IsOverdue: false) };

        SchedulingDueTimeEvaluator.EvaluateStatus(dueTimes, AsOfUtc).Should().Be(SchedulingDueTimeStatus.OnTrack);
    }

    [Fact]
    public void EvaluateStatus_WithinDueSoonThreshold_ReturnsDueSoon()
    {
        var dueTimes = new[] { new ScheduledDueTime("ACT-1", "Action", AsOfUtc.AddHours(2), IsOverdue: false) };

        SchedulingDueTimeEvaluator.EvaluateStatus(dueTimes, AsOfUtc).Should().Be(SchedulingDueTimeStatus.DueSoon);
    }

    [Fact]
    public void EvaluateStatus_AnyOverdue_ReturnsOverdueRegardlessOfOthers()
    {
        var dueTimes = new[]
        {
            new ScheduledDueTime("ACT-1", "On track", AsOfUtc.AddHours(48), IsOverdue: false),
            new ScheduledDueTime("ACT-2", "Overdue", AsOfUtc.AddHours(-1), IsOverdue: true),
        };

        SchedulingDueTimeEvaluator.EvaluateStatus(dueTimes, AsOfUtc).Should().Be(SchedulingDueTimeStatus.Overdue);
    }

    [Fact]
    public void EvaluateStatus_OverdueBeatsWorstCaseDueSoon()
    {
        var dueTimes = new[]
        {
            new ScheduledDueTime("ACT-1", "Due soon", AsOfUtc.AddHours(1), IsOverdue: false),
            new ScheduledDueTime("ACT-2", "Overdue", AsOfUtc.AddHours(-5), IsOverdue: true),
        };

        SchedulingDueTimeEvaluator.EvaluateStatus(dueTimes, AsOfUtc).Should().Be(SchedulingDueTimeStatus.Overdue);
    }

    [Fact]
    public void EvaluateStatus_NullDueTimes_Throws()
    {
        Action act = () => SchedulingDueTimeEvaluator.EvaluateStatus(null!, AsOfUtc);

        act.Should().Throw<ArgumentNullException>();
    }
}
