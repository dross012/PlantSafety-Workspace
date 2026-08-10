// ============================================================================
//  Project:        Shared.Simulation.Tests
//  File:           SimulationGovernanceEvaluatorTests.cs
//  Created:        2026-08-10
// ============================================================================
using FluentAssertions;

using Shared.Simulation.Abstractions.Simulation;
using Shared.Simulation.Infrastructure.Governance;
using Shared.Telemetry.Abstractions.Events;

using Xunit;

namespace Shared.Simulation.Tests.Governance;

public sealed class SimulationGovernanceEvaluatorTests
{
    private sealed class CapturingLogEventWriter : ILogEventWriter
    {
        public ILogEvent? LastEvent { get; private set; }
        public void Write(ILogEvent logEvent) => LastEvent = logEvent;
    }

    private static readonly DateTime OccurredAtUtc = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void EvaluateContractMetadataAndReport_ValidMetadata_ReturnsEmptyAndDoesNotWrite()
    {
        var writer = new CapturingLogEventWriter();
        var metadata = new SimulationMetadata(SimulationMetadata.CurrentSchemaVersion, "acme-plant-1", "corr-1", OccurredAtUtc);

        SimulationGovernanceEvaluator.EvaluateContractMetadataAndReport(metadata, OccurredAtUtc, writer).Should().BeEmpty();
        writer.LastEvent.Should().BeNull();
    }

    [Fact]
    public void EvaluateContractMetadataAndReport_InvalidMetadata_WritesGovernanceViolationEvent()
    {
        var writer = new CapturingLogEventWriter();
        var metadata = new SimulationMetadata(SimulationMetadata.CurrentSchemaVersion, string.Empty, "corr-2", OccurredAtUtc);

        SimulationGovernanceEvaluator.EvaluateContractMetadataAndReport(metadata, OccurredAtUtc, writer).Should().NotBeEmpty();
        writer.LastEvent!.EventId.Should().Be("SIMULATION_GOVERNANCE_VIOLATION");
    }

    [Fact]
    public void EvaluateScenarioAndReport_EmptySteps_WritesGovernanceViolationEvent()
    {
        var writer = new CapturingLogEventWriter();
        var scenario = new SimulationScenario("scenario-1", "Test", Array.Empty<SimulationStep>());

        SimulationGovernanceEvaluator.EvaluateScenarioAndReport(scenario, "corr-3", OccurredAtUtc, writer).Should().NotBeEmpty();
        writer.LastEvent!.EventId.Should().Be("SIMULATION_GOVERNANCE_VIOLATION");
    }
}
