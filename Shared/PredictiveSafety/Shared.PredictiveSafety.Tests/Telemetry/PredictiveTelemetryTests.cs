// ============================================================================
//  Project:        Shared.PredictiveSafety.Tests
//  File:           PredictiveTelemetryTests.cs
//  Created:        2026-08-10
// ============================================================================
using FluentAssertions;

using Shared.Deterministic.Abstractions.Clock;
using Shared.PredictiveSafety.Abstractions.Hazard;
using Shared.PredictiveSafety.Infrastructure.Telemetry;
using Shared.Telemetry.Abstractions.Events;

using Xunit;

namespace Shared.PredictiveSafety.Tests.Telemetry;

public sealed class PredictiveTelemetryTests
{
    private sealed class CapturingLogEventWriter : ILogEventWriter
    {
        public ILogEvent? LastEvent { get; private set; }
        public void Write(ILogEvent logEvent) => LastEvent = logEvent;
    }

    private static readonly FixedClock Clock = new(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));

    [Fact]
    public async Task InferHazardWithTelemetryAsync_ReturnsSameDecisionAsDirectInfer()
    {
        var writer = new CapturingLogEventWriter();
        var signals = new PredictiveHazardSignals("Fire", null, 0, 0, 0, 0, 0, false, 0);

        PredictiveHazardDecision viaWrapper = await PredictiveTelemetry.InferHazardWithTelemetryAsync(signals, Clock, writer, "corr-1");
        PredictiveHazardDecision direct = PredictiveHazardInferenceEngine.Infer(signals);

        viaWrapper.Should().BeEquivalentTo(direct);
    }

    [Fact]
    public async Task InferHazardWithTelemetryAsync_EmitsEvaluationComputedLogEvent()
    {
        var writer = new CapturingLogEventWriter();
        var signals = new PredictiveHazardSignals("Fire", null, 0, 0, 0, 0, 0, false, 0);

        await PredictiveTelemetry.InferHazardWithTelemetryAsync(signals, Clock, writer, "corr-2");

        writer.LastEvent.Should().NotBeNull();
        writer.LastEvent!.EventId.Should().Be("PREDICTIVE_EVALUATION_COMPUTED");
        writer.LastEvent.CorrelationId.Should().Be("corr-2");
        writer.LastEvent.Category.Should().Be(LogCategory.PredictiveSafety);
    }
}
