// ============================================================================
//  Project:        Shared.Scheduling.Tests
//  File:           SchedulingDueTimeEngineTests.cs
//  Created:        2026-08-10
// ============================================================================
using FluentAssertions;

using Shared.Scheduling.Abstractions.DueTime;
using Shared.WorkflowEngine.Abstractions.Actions;

using Xunit;

namespace Shared.Scheduling.Tests.DueTime;

public sealed class SchedulingDueTimeEngineTests
{
    private static readonly DateTime AsOfUtc = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void CalculateDueTimes_AddsDueWithinHoursToAsOfUtc()
    {
        var actions = new[] { new RequiredAction("ACT-1", "Action one", DueWithinHours: 5) };

        IReadOnlyList<ScheduledDueTime> result = SchedulingDueTimeEngine.CalculateDueTimes(actions, AsOfUtc);

        result.Should().ContainSingle();
        result[0].DueAtUtc.Should().Be(AsOfUtc.AddHours(5));
        result[0].IsOverdue.Should().BeFalse();
    }

    [Fact]
    public void CalculateDueTimes_ZeroDueWithinHours_MarksOverdue()
    {
        var actions = new[] { new RequiredAction("ACT-1", "Immediate action", DueWithinHours: 0) };

        IReadOnlyList<ScheduledDueTime> result = SchedulingDueTimeEngine.CalculateDueTimes(actions, AsOfUtc);

        result[0].IsOverdue.Should().BeTrue();
        result[0].DueAtUtc.Should().Be(AsOfUtc);
    }

    [Fact]
    public void CalculateDueTimes_PreservesInputOrder()
    {
        var actions = new[]
        {
            new RequiredAction("ACT-A", "First", DueWithinHours: 10),
            new RequiredAction("ACT-B", "Second", DueWithinHours: 1),
            new RequiredAction("ACT-C", "Third", DueWithinHours: 5),
        };

        IReadOnlyList<ScheduledDueTime> result = SchedulingDueTimeEngine.CalculateDueTimes(actions, AsOfUtc);

        result.Select(r => r.ActionCode).Should().ContainInOrder("ACT-A", "ACT-B", "ACT-C");
    }

    [Fact]
    public void CalculateDueTimes_EmptyInput_ReturnsEmpty()
    {
        IReadOnlyList<ScheduledDueTime> result = SchedulingDueTimeEngine.CalculateDueTimes(Array.Empty<RequiredAction>(), AsOfUtc);

        result.Should().BeEmpty();
    }

    [Fact]
    public void CalculateDueTimes_SameInputTwice_ProducesIdenticalResult()
    {
        var actions = new[] { new RequiredAction("ACT-1", "Action", DueWithinHours: 7) };

        IReadOnlyList<ScheduledDueTime> first = SchedulingDueTimeEngine.CalculateDueTimes(actions, AsOfUtc);
        IReadOnlyList<ScheduledDueTime> second = SchedulingDueTimeEngine.CalculateDueTimes(actions, AsOfUtc);

        second.Should().BeEquivalentTo(first);
    }

    [Fact]
    public void CalculateDueTimes_NullRequiredActions_Throws()
    {
        Action act = () => SchedulingDueTimeEngine.CalculateDueTimes(null!, AsOfUtc);

        act.Should().Throw<ArgumentNullException>();
    }
}
