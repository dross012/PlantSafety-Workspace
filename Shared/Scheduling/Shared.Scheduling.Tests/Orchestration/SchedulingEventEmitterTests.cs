// ============================================================================
//  Project:        Shared.Scheduling.Tests
//  File:           SchedulingEventEmitterTests.cs
//  Created:        2026-08-10
// ============================================================================
using FluentAssertions;

using Shared.Scheduling.Abstractions.Contracts;
using Shared.Scheduling.Abstractions.DueTime;
using Shared.Scheduling.Abstractions.Events;
using Shared.Scheduling.Abstractions.Rules;
using Shared.Scheduling.Abstractions.Schedule;
using Shared.Scheduling.Infrastructure.Orchestration;
using Shared.Telemetry.Abstractions.Events;
using Shared.WorkflowEngine.Abstractions.Actions;

using Xunit;

namespace Shared.Scheduling.Tests.Orchestration;

public sealed class SchedulingEventEmitterTests
{
    private sealed class CapturingLogEventWriter : ILogEventWriter
    {
        public ILogEvent? LastEvent { get; private set; }
        public void Write(ILogEvent logEvent) => LastEvent = logEvent;
    }

    private static readonly DateTime AsOfUtc = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Emit_PublishesScheduledEventPublishedLogEventWithMatchingCorrelation()
    {
        var writer = new CapturingLogEventWriter();
        var metadata = new SchedulingContractMetadata(SchedulingContractMetadata.CurrentSchemaVersion, "acme-plant-1", "corr-1", AsOfUtc);
        var actions = new[] { new RequiredAction("ACT-1", "Action", DueWithinHours: 6) };
        var context = new SchedulingEvaluationContext(actions, OverdueActionCount: 0, ActiveMachineGroupCount: 0);
        SchedulingSchedule schedule = SchedulingScheduleCalculator.Calculate(context, AsOfUtc);
        var result = new SchedulingEvaluationResultContract(metadata, schedule, SchedulingDueTimeStatus.OnTrack);

        SchedulingScheduledEvent scheduledEvent = SchedulingEventEmitter.Emit(result, writer);

        scheduledEvent.CorrelationId.Should().Be("corr-1");
        scheduledEvent.TenantId.Should().Be("acme-plant-1");
        writer.LastEvent.Should().NotBeNull();
        writer.LastEvent!.EventId.Should().Be("SCHEDULING_SCHEDULED_EVENT_PUBLISHED");
        writer.LastEvent.CorrelationId.Should().Be("corr-1");
    }

    [Fact]
    public void Emit_NullResult_Throws()
    {
        var writer = new CapturingLogEventWriter();

        Action act = () => SchedulingEventEmitter.Emit(null!, writer);

        act.Should().Throw<ArgumentNullException>();
    }
}
