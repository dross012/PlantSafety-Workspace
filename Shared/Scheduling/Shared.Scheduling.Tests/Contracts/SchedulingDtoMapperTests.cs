// ============================================================================
//  Project:        Shared.Scheduling.Tests
//  File:           SchedulingDtoMapperTests.cs
//  Created:        2026-08-10
// ============================================================================
using FluentAssertions;

using Shared.Scheduling.Abstractions.Contracts;
using Shared.Scheduling.Abstractions.DueTime;
using Shared.Scheduling.Abstractions.Rules;
using Shared.Scheduling.Abstractions.Schedule;
using Shared.WorkflowEngine.Abstractions.Actions;

using Xunit;

namespace Shared.Scheduling.Tests.Contracts;

public sealed class SchedulingDtoMapperTests
{
    private static readonly DateTime AsOfUtc = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void ToDto_ProjectsScheduleFieldsAndStatus()
    {
        var actions = new[] { new RequiredAction("ACT-1", "Action", DueWithinHours: 4) };
        var context = new SchedulingEvaluationContext(actions, OverdueActionCount: 0, ActiveMachineGroupCount: 0);
        SchedulingSchedule schedule = SchedulingScheduleCalculator.Calculate(context, AsOfUtc);
        SchedulingDueTimeStatus status = SchedulingDueTimeEvaluator.EvaluateStatus(schedule.DueTimes, AsOfUtc);

        SchedulingScheduleDto dto = SchedulingDtoMapper.ToDto(schedule, status);

        dto.DueTimes.Should().HaveCount(1);
        dto.TotalRequiredActionCount.Should().Be(schedule.RuleDecision.TotalRequiredActionCount);
        dto.Status.Should().Be(status.ToString());
        dto.EarliestDueAtUtc.Should().Be(schedule.EarliestDueAtUtc);
    }

    [Fact]
    public void ToDto_NullSchedule_Throws()
    {
        Action act = () => SchedulingDtoMapper.ToDto(null!, SchedulingDueTimeStatus.OnTrack);

        act.Should().Throw<ArgumentNullException>();
    }
}
