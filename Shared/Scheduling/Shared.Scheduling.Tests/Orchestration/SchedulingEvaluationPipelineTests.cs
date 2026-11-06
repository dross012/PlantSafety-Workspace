// ============================================================================
//  Project:        Shared.Scheduling.Tests
//  File:           SchedulingEvaluationPipelineTests.cs
//  Created:        2026-08-10
// ============================================================================
using FluentAssertions;

using Shared.Scheduling.Abstractions.Contracts;
using Shared.Scheduling.Abstractions.DueTime;
using Shared.Scheduling.Abstractions.Rules;
using Shared.Scheduling.Abstractions.Schedule;
using Shared.Scheduling.Infrastructure.Orchestration;
using Shared.WorkflowEngine.Abstractions.Actions;

using Xunit;

namespace Shared.Scheduling.Tests.Orchestration;

public sealed class SchedulingEvaluationPipelineTests
{
    private static readonly DateTime AsOfUtc = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Compose_OverdueAction_ProducesOverdueStatusResult()
    {
        var metadata = new SchedulingContractMetadata(SchedulingContractMetadata.CurrentSchemaVersion, "acme-plant-1", "corr-1", AsOfUtc);
        var actions = new[] { new RequiredAction("ACT-1", "Action", DueWithinHours: 0) };
        var context = new SchedulingEvaluationContext(actions, OverdueActionCount: 0, ActiveMachineGroupCount: 0);
        SchedulingSchedule schedule = SchedulingScheduleCalculator.Calculate(context, AsOfUtc);

        SchedulingEvaluationResultContract result = SchedulingEvaluationPipeline.Compose(schedule, metadata, AsOfUtc);

        result.Status.Should().Be(SchedulingDueTimeStatus.Overdue);
        result.Metadata.Should().Be(metadata);
        result.Schedule.Should().Be(schedule);
    }

    [Fact]
    public void Compose_NullSchedule_Throws()
    {
        var metadata = new SchedulingContractMetadata(SchedulingContractMetadata.CurrentSchemaVersion, "acme-plant-1", "corr-1", AsOfUtc);

        Action act = () => SchedulingEvaluationPipeline.Compose(null!, metadata, AsOfUtc);

        act.Should().Throw<ArgumentNullException>();
    }
}
