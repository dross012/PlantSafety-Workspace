// ============================================================================
//  Project:        Shared.Simulation.Tests
//  File:           SimulationGovernanceRulesTests.cs
//  Created:        2026-08-10
// ============================================================================
using FluentAssertions;

using Shared.PredictiveSafety.Abstractions.Common;
using Shared.Simulation.Abstractions.Governance;
using Shared.Simulation.Abstractions.Simulation;

using Xunit;

namespace Shared.Simulation.Tests.Governance;

public sealed class SimulationGovernanceRulesTests
{
    [Fact]
    public void ValidateContractMetadata_ValidMetadata_ReturnsNoViolations()
    {
        var metadata = new SimulationMetadata(SimulationMetadata.CurrentSchemaVersion, "acme-plant-1", "corr-1", DateTime.UtcNow);

        SimulationGovernanceRules.ValidateContractMetadata(metadata).Should().BeEmpty();
    }

    [Fact]
    public void ValidateContractMetadata_MissingTenantId_ReturnsViolation()
    {
        var metadata = new SimulationMetadata(SimulationMetadata.CurrentSchemaVersion, string.Empty, "corr-1", DateTime.UtcNow);

        SimulationGovernanceRules.ValidateContractMetadata(metadata).Should().NotBeEmpty();
    }

    [Fact]
    public void ValidateScenario_EmptySteps_ReturnsViolation()
    {
        var scenario = new SimulationScenario("scenario-1", "Test", Array.Empty<SimulationStep>());

        SimulationGovernanceRules.ValidateScenario(scenario).Should().NotBeEmpty();
    }

    [Fact]
    public void ValidateScenario_MissingScenarioId_ReturnsViolation()
    {
        var scenario = new SimulationScenario(string.Empty, "Test", new[] { new SimulationStep(0, SimulationStepType.Hazard, "x") });

        SimulationGovernanceRules.ValidateScenario(scenario).Should().NotBeEmpty();
    }

    [Fact]
    public void ValidateScenario_ValidScenario_ReturnsNoViolations()
    {
        var scenario = new SimulationScenario("scenario-1", "Test", new[] { new SimulationStep(0, SimulationStepType.Hazard, "x") });

        SimulationGovernanceRules.ValidateScenario(scenario).Should().BeEmpty();
    }

    [Fact]
    public void ValidateEscalationOutcome_InconsistentOutcome_ReturnsViolation()
    {
        var outcome = new PredictiveEscalationOutcome(true, Array.Empty<string>());

        SimulationGovernanceRules.ValidateEscalationOutcome(outcome).Should().NotBeEmpty();
    }
}
