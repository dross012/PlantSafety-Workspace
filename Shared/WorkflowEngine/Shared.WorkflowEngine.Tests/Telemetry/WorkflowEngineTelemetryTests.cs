// ============================================================================
//  Project:        Shared.WorkflowEngine.Tests
//  File:           WorkflowEngineTelemetryTests.cs
//  Created:        2026-08-10
// ============================================================================
using FluentAssertions;

using Shared.Deterministic.Abstractions.Clock;
using Shared.FacilityDomain.Abstractions.Hazards;
using Shared.FacilityDomain.Abstractions.Severity;
using Shared.Telemetry.Abstractions.Events;
using Shared.WorkflowEngine.Abstractions.Engine;
using Shared.WorkflowEngine.Abstractions.Triggers;
using Shared.WorkflowEngine.Infrastructure.Telemetry;

using Xunit;

namespace Shared.WorkflowEngine.Tests.Telemetry;

public sealed class WorkflowEngineTelemetryTests
{
    private sealed class CapturingLogEventWriter : ILogEventWriter
    {
        public ILogEvent? LastEvent { get; private set; }
        public void Write(ILogEvent logEvent) => LastEvent = logEvent;
    }

    [Fact]
    public async Task EvaluateWithTelemetryAsync_ReturnsSameDecisionAsDirectEvaluate()
    {
        var clock = new FixedClock(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        var writer = new CapturingLogEventWriter();
        var context = new WorkflowContext(HazardType.Fire, SeverityLevel.Catastrophic, EvacuationScope.Building, 2, 1, 3);

        WorkflowDecision viaWrapper = await WorkflowEngineTelemetry.EvaluateWithTelemetryAsync(context, clock, writer, "corr-1");
        WorkflowDecision direct = Abstractions.Engine.WorkflowEngine.Evaluate(context);

        viaWrapper.Should().BeEquivalentTo(direct);
    }

    [Fact]
    public async Task EvaluateWithTelemetryAsync_EmitsWorkflowDecisionEvaluatedLogEvent()
    {
        var clock = new FixedClock(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        var writer = new CapturingLogEventWriter();
        var context = new WorkflowContext(HazardType.HotWork, SeverityLevel.Moderate, EvacuationScope.Zone, 0, 0, 0);

        await WorkflowEngineTelemetry.EvaluateWithTelemetryAsync(context, clock, writer, "corr-2");

        writer.LastEvent.Should().NotBeNull();
        writer.LastEvent!.EventId.Should().Be("WORKFLOW_DECISION_EVALUATED");
        writer.LastEvent.CorrelationId.Should().Be("corr-2");
        writer.LastEvent.Category.Should().Be(LogCategory.WorkflowEngine);
    }
}
