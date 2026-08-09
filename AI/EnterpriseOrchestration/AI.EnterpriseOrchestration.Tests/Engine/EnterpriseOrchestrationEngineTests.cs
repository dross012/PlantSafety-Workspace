// ============================================================================
//  Project:        AI.EnterpriseOrchestration.Tests
//  File:           EnterpriseOrchestrationEngineTests.cs
//  Created:        2026-08-08
//  Description:    Phase E.9 unit tests for EnterpriseOrchestrationEngine.Orchestrate,
//                  exercised end-to-end against real per-facility outputs
//                  of Path E's other engines: evacuation/regulatory/
//                  anomaly aggregation across facilities, worst-case
//                  severity, and rule-contributed enterprise alerts.
// ============================================================================
using FluentAssertions;

using Shared.FacilityDomain.Abstractions.Hazards;
using Shared.FacilityDomain.Abstractions.Propagation;
using Shared.FacilityDomain.Abstractions.Severity;

using Shared.WorkflowEngine.Abstractions.Engine;
using Shared.WorkflowEngine.Abstractions.Triggers;

using AI.HazardUnderstanding.Abstractions.Engine;
using AI.HazardUnderstanding.Abstractions.Semantics;
using AI.PredictiveOptimization.Abstractions.Engine;
using AI.PredictiveOptimization.Abstractions.Models;
using AI.PredictiveOptimization.Abstractions.Optimization;
using AI.RegulatoryInterpretation.Abstractions.Engine;
using AI.RegulatoryInterpretation.Abstractions.Interpretation;
using AI.WorkflowAutomation.Abstractions.Automation;
using AI.WorkflowAutomation.Abstractions.Engine;

using AI.EnterpriseOrchestration.Abstractions.Engine;
using AI.EnterpriseOrchestration.Abstractions.Orchestration;
using AI.EnterpriseOrchestration.Abstractions.Rules;

using Xunit;

namespace AI.EnterpriseOrchestration.Tests.Engine;

public sealed class EnterpriseOrchestrationEngineTests
{
    private static FacilityOrchestrationContext CreateFacility(Guid facilityId, HazardType hazardType, SeverityLevel severity, IReadOnlyList<double> predictiveHistory)
    {
        WorkflowAutomationOutput workflow = WorkflowAutomationEngine.Automate(
            new WorkflowAutomationInput(new WorkflowContext(hazardType, severity, EvacuationScope.Facility, 0, 0, 1), false, false), []);
        HazardUnderstandingOutput hazard = HazardUnderstandingEngine.Understand(new HazardUnderstandingInput(hazardType, PropagationStage.Fire, 0, 0, 1));
        RegulatoryInterpretationOutput regulatory = RegulatoryInterpretationEngine.Interpret(new RegulatoryInterpretationInput(hazardType, severity));
        PredictiveOptimizationOutput predictive = PredictiveOptimizationEngine.Optimize(
            new PredictiveOptimizationInput(
                new Dictionary<OptimizationTarget, IReadOnlyList<double>> { [OptimizationTarget.HazardLikelihood] = predictiveHistory },
                new Dictionary<OptimizationTarget, double> { [OptimizationTarget.HazardLikelihood] = 1.0 },
                WindowSize: 4));

        return new FacilityOrchestrationContext(facilityId, workflow, hazard, regulatory, predictive, severity);
    }

    [Fact]
    public void Orchestrate_AggregatesFacilitiesRequiringEvacuation()
    {
        Guid severeFacility = Guid.NewGuid();
        Guid calmFacility = Guid.NewGuid();
        var input = new EnterpriseOrchestrationInput(
        [
            CreateFacility(severeFacility, HazardType.Fire, SeverityLevel.Severe, [1, 1, 1, 1]),
            CreateFacility(calmFacility, HazardType.Other, SeverityLevel.Minor, [1, 1, 1, 1]),
        ]);

        EnterpriseOrchestrationOutput output = EnterpriseOrchestrationEngine.Orchestrate(input, []);

        output.TotalFacilities.Should().Be(2);
        output.FacilitiesRequiringEvacuation.Should().ContainSingle().Which.Should().Be(severeFacility);
    }

    [Fact]
    public void Orchestrate_AggregatesFacilitiesWithRegulatoryExposure()
    {
        Guid fireFacility = Guid.NewGuid();
        var input = new EnterpriseOrchestrationInput([CreateFacility(fireFacility, HazardType.Fire, SeverityLevel.Severe, [1, 1, 1, 1])]);

        EnterpriseOrchestrationOutput output = EnterpriseOrchestrationEngine.Orchestrate(input, []);

        output.FacilitiesWithRegulatoryExposure.Should().Contain(fireFacility);
    }

    [Fact]
    public void Orchestrate_AggregatesFacilitiesWithAnomalousPredictions()
    {
        Guid spikeFacility = Guid.NewGuid();
        Guid steadyFacility = Guid.NewGuid();
        var input = new EnterpriseOrchestrationInput(
        [
            CreateFacility(spikeFacility, HazardType.Other, SeverityLevel.Minor, [0, 0, 0, 0, 50]),
            CreateFacility(steadyFacility, HazardType.Other, SeverityLevel.Minor, [1, 1, 1, 1, 1]),
        ]);

        EnterpriseOrchestrationOutput output = EnterpriseOrchestrationEngine.Orchestrate(input, []);

        output.FacilitiesWithAnomalousPredictions.Should().ContainSingle().Which.Should().Be(spikeFacility);
    }

    [Fact]
    public void Orchestrate_HighestSeverityAcrossFacilities_IsWorstCase()
    {
        var input = new EnterpriseOrchestrationInput(
        [
            CreateFacility(Guid.NewGuid(), HazardType.Other, SeverityLevel.Minor, [1, 1, 1, 1]),
            CreateFacility(Guid.NewGuid(), HazardType.Fire, SeverityLevel.Catastrophic, [1, 1, 1, 1]),
        ]);

        EnterpriseOrchestrationOutput output = EnterpriseOrchestrationEngine.Orchestrate(input, []);

        output.HighestSeverityAcrossFacilities.Should().Be(SeverityLevel.Catastrophic);
    }

    [Fact]
    public void Orchestrate_NoFacilities_ReturnsMinorSeverityAndEmptyLists()
    {
        var input = new EnterpriseOrchestrationInput([]);

        EnterpriseOrchestrationOutput output = EnterpriseOrchestrationEngine.Orchestrate(input, []);

        output.TotalFacilities.Should().Be(0);
        output.HighestSeverityAcrossFacilities.Should().Be(SeverityLevel.Minor);
        output.FacilitiesRequiringEvacuation.Should().BeEmpty();
    }

    [Fact]
    public void Orchestrate_MatchedRule_ContributesEnterpriseAlert()
    {
        var input = new EnterpriseOrchestrationInput([CreateFacility(Guid.NewGuid(), HazardType.Fire, SeverityLevel.Catastrophic, [1, 1, 1, 1])]);
        var rule = new EnterpriseOrchestrationRule(
            "R1", "Catastrophic anywhere",
            facilities => facilities.Any(f => f.DigitalTwinSimulatedSeverity == SeverityLevel.Catastrophic),
            "Enterprise-wide catastrophic event detected.");

        EnterpriseOrchestrationOutput output = EnterpriseOrchestrationEngine.Orchestrate(input, [rule]);

        output.EnterpriseAlerts.Should().ContainSingle().Which.Should().Be("Enterprise-wide catastrophic event detected.");
    }
}
