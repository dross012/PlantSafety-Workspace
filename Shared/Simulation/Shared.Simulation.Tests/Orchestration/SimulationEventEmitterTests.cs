// ============================================================================
//  Project:        Shared.Simulation.Tests
//  File:           SimulationEventEmitterTests.cs
//  Created:        2026-08-10
// ============================================================================
using FluentAssertions;

using Shared.PredictiveSafety.Abstractions.Common;
using Shared.Simulation.Abstractions.Replay;
using Shared.Simulation.Abstractions.Scenario;
using Shared.Simulation.Abstractions.Simulation;
using Shared.Simulation.Infrastructure.Orchestration;
using Shared.Telemetry.Abstractions.Events;

using Xunit;

namespace Shared.Simulation.Tests.Orchestration;

public sealed class SimulationEventEmitterTests
{
    private sealed class CapturingLogEventWriter : ILogEventWriter
    {
        public ILogEvent? LastEvent { get; private set; }
        public void Write(ILogEvent logEvent) => LastEvent = logEvent;
    }

    [Fact]
    public void Emit_PublishesEvaluationEventPublishedLogEventWithMatchingCorrelation()
    {
        var writer = new CapturingLogEventWriter();
        var metadata = new SimulationMetadata(SimulationMetadata.CurrentSchemaVersion, "acme-plant-1", "corr-1", DateTime.UtcNow);
        var replay = new SimulationReplayResult("scenario-1", new[] { 0 }, true, Array.Empty<string>());
        var scenario = new SimulationScenarioResult("scenario-1", Array.Empty<SimulationStepOutcome>(), false, PredictivePriority.Low, Array.Empty<string>());
        var outcome = new SimulationOutcome(replay, scenario, false, PredictivePriority.Low);
        var result = new SimulationEvaluationResultContract(metadata, outcome);

        SimulationEventEmitter.Emit(result, writer);

        writer.LastEvent.Should().NotBeNull();
        writer.LastEvent!.EventId.Should().Be("SIMULATION_EVALUATION_EVENT_PUBLISHED");
        writer.LastEvent.CorrelationId.Should().Be("corr-1");
    }

    [Fact]
    public void Emit_NullResult_Throws()
    {
        var writer = new CapturingLogEventWriter();

        Action act = () => SimulationEventEmitter.Emit(null!, writer);

        act.Should().Throw<ArgumentNullException>();
    }
}
