// ============================================================================
//  Project:        Shared.Scheduling.Tests
//  File:           SchedulingScheduleCalculatorTests.cs
//  Created:        2026-08-10
// ============================================================================
using FluentAssertions;

using Shared.Scheduling.Abstractions.Rules;
using Shared.Scheduling.Abstractions.Schedule;
using Shared.WorkflowEngine.Abstractions.Actions;

using Xunit;

namespace Shared.Scheduling.Tests.Schedule;

public sealed class SchedulingScheduleCalculatorTests
{
    private static readonly DateTime AsOfUtc = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Calculate_EmptyRequiredActions_EarliestDueAtUtcIsMaxValue()
    {
        var context = new SchedulingEvaluationContext(Array.Empty<RequiredAction>(), OverdueActionCount: 0, ActiveMachineGroupCount: 0);

        SchedulingSchedule schedule = SchedulingScheduleCalculator.Calculate(context, AsOfUtc);

        schedule.DueTimes.Should().BeEmpty();
        schedule.EarliestDueAtUtc.Should().Be(DateTime.MaxValue);
        schedule.RuleDecision.TotalRequiredActionCount.Should().Be(0);
    }

    [Fact]
    public void Calculate_EarliestDueAtUtcIsWorstCaseWinsSoonestDueTime()
    {
        var actions = new[]
        {
            new RequiredAction("ACT-1", "Later action", DueWithinHours: 20),
            new RequiredAction("ACT-2", "Sooner action", DueWithinHours: 3),
        };
        var context = new SchedulingEvaluationContext(actions, OverdueActionCount: 0, ActiveMachineGroupCount: 0);

        SchedulingSchedule schedule = SchedulingScheduleCalculator.Calculate(context, AsOfUtc);

        schedule.EarliestDueAtUtc.Should().Be(AsOfUtc.AddHours(3));
    }

    [Fact]
    public void Calculate_ComposesRuleDecisionAndDueTimesConsistently()
    {
        var actions = new[] { new RequiredAction("ACT-1", "Action", DueWithinHours: 8) };
        var context = new SchedulingEvaluationContext(actions, OverdueActionCount: 0, ActiveMachineGroupCount: 0);

        SchedulingSchedule schedule = SchedulingScheduleCalculator.Calculate(context, AsOfUtc);

        schedule.DueTimes.Should().HaveCount(schedule.RuleDecision.TotalRequiredActionCount);
    }

    [Fact]
    public void Calculate_SameInputTwice_ProducesIdenticalResult()
    {
        var actions = new[] { new RequiredAction("ACT-1", "Action", DueWithinHours: 4) };
        var context = new SchedulingEvaluationContext(actions, OverdueActionCount: 1, ActiveMachineGroupCount: 1);

        SchedulingSchedule first = SchedulingScheduleCalculator.Calculate(context, AsOfUtc);
        SchedulingSchedule second = SchedulingScheduleCalculator.Calculate(context, AsOfUtc);

        second.Should().BeEquivalentTo(first);
    }

    [Fact]
    public void Calculate_NullContext_Throws()
    {
        Action act = () => SchedulingScheduleCalculator.Calculate(null!, AsOfUtc);

        act.Should().Throw<ArgumentNullException>();
    }
}
