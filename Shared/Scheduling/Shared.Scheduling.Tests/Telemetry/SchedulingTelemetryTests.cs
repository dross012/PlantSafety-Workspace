// ============================================================================
//  Project:        Shared.Scheduling.Tests
//  File:           SchedulingTelemetryTests.cs
//  Created:        2026-08-10
// ============================================================================
using FluentAssertions;

using Shared.Deterministic.Abstractions.Clock;
using Shared.Scheduling.Abstractions.Rules;
using Shared.Scheduling.Abstractions.Schedule;
using Shared.Scheduling.Infrastructure.Telemetry;
using Shared.Telemetry.Abstractions.Events;
using Shared.WorkflowEngine.Abstractions.Actions;

using Xunit;

namespace Shared.Scheduling.Tests.Telemetry;

public sealed class SchedulingTelemetryTests
{
    private sealed class CapturingLogEventWriter : ILogEventWriter
    {
        public ILogEvent? LastEvent { get; private set; }
        public void Write(ILogEvent logEvent) => LastEvent = logEvent;
    }

    private static readonly DateTime AsOfUtc = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task CalculateWithTelemetryAsync_ReturnsSameScheduleAsDirectCalculate()
    {
        var clock = new FixedClock(AsOfUtc);
        var writer = new CapturingLogEventWriter();
        var actions = new[] { new RequiredAction("ACT-1", "Action", DueWithinHours: 6) };
        var context = new SchedulingEvaluationContext(actions, OverdueActionCount: 0, ActiveMachineGroupCount: 0);

        SchedulingSchedule viaWrapper = await SchedulingTelemetry.CalculateWithTelemetryAsync(context, AsOfUtc, clock, writer, "corr-1");
        SchedulingSchedule direct = SchedulingScheduleCalculator.Calculate(context, AsOfUtc);

        viaWrapper.Should().BeEquivalentTo(direct);
    }

    [Fact]
    public async Task CalculateWithTelemetryAsync_EmitsScheduleComputedLogEvent()
    {
        var clock = new FixedClock(AsOfUtc);
        var writer = new CapturingLogEventWriter();
        var actions = new[] { new RequiredAction("ACT-1", "Action", DueWithinHours: 6) };
        var context = new SchedulingEvaluationContext(actions, OverdueActionCount: 0, ActiveMachineGroupCount: 0);

        await SchedulingTelemetry.CalculateWithTelemetryAsync(context, AsOfUtc, clock, writer, "corr-2");

        writer.LastEvent.Should().NotBeNull();
        writer.LastEvent!.EventId.Should().Be("SCHEDULING_SCHEDULE_COMPUTED");
        writer.LastEvent.CorrelationId.Should().Be("corr-2");
        writer.LastEvent.Category.Should().Be(LogCategory.Scheduling);
    }

    [Fact]
    public async Task CalculateWithTelemetryAsync_NullContext_Throws()
    {
        var clock = new FixedClock(AsOfUtc);
        var writer = new CapturingLogEventWriter();

        Func<Task> act = () => SchedulingTelemetry.CalculateWithTelemetryAsync(null!, AsOfUtc, clock, writer, "corr-3");

        await act.Should().ThrowAsync<ArgumentNullException>();
    }
}
