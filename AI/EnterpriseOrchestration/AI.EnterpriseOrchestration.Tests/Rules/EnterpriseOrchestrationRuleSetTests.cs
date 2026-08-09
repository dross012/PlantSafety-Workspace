// ============================================================================
//  Project:        AI.EnterpriseOrchestration.Tests
//  File:           EnterpriseOrchestrationRuleSetTests.cs
//  Created:        2026-08-09
//  Description:    Tests for EnterpriseOrchestrationRuleSet.Standard,
//                  evaluated through the real EnterpriseOrchestrationEngine.Orchestrate.
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
using AI.WorkflowAutomation.Abstractions.Rules;

using AI.EnterpriseOrchestration.Abstractions.Engine;
using AI.EnterpriseOrchestration.Abstractions.Orchestration;
using AI.EnterpriseOrchestration.Abstractions.Rules;

using Xunit;

namespace AI.EnterpriseOrchestration.Tests.Rules;

public sealed class EnterpriseOrchestrationRuleSetTests
{
    private static FacilityOrchestrationContext CreateFacility(HazardType hazardType, SeverityLevel severity, bool requiresShutdown = false) =>
        new(
            Guid.NewGuid(),
            WorkflowAutomationEngine.Automate(
                new WorkflowAutomationInput(new WorkflowContext(hazardType, severity, EvacuationScope.Facility, 0, 0, 1), requiresShutdown, false),
                WorkflowAutomationRuleSet.Standard),
            HazardUnderstandingEngine.Understand(new HazardUnderstandingInput(hazardType, PropagationStage.Fire, 0, 0, 1)),
            RegulatoryInterpretationEngine.Interpret(new RegulatoryInterpretationInput(hazardType, severity)),
            PredictiveOptimizationEngine.Optimize(new PredictiveOptimizationInput(
                new Dictionary<OptimizationTarget, IReadOnlyList<double>> { [OptimizationTarget.HazardLikelihood] = [1, 1, 1, 1] },
                new Dictionary<OptimizationTarget, double> { [OptimizationTarget.HazardLikelihood] = 1.0 },
                WindowSize: 4)),
            severity);

    [Fact]
    public void Orchestrate_TwoCatastrophicFacilities_ContributesCrisisCommandAlert()
    {
        var input = new EnterpriseOrchestrationInput(
        [
            CreateFacility(HazardType.Fire, SeverityLevel.Catastrophic),
            CreateFacility(HazardType.ChemicalSpill, SeverityLevel.Catastrophic),
        ]);

        EnterpriseOrchestrationOutput output = EnterpriseOrchestrationEngine.Orchestrate(input, EnterpriseOrchestrationRuleSet.Standard);

        output.EnterpriseAlerts.Should().Contain(a => a.Contains("crisis command"));
    }

    [Fact]
    public void Orchestrate_TwoConcurrentShutdowns_ContributesSinglePointOfFailureAlert()
    {
        var input = new EnterpriseOrchestrationInput(
        [
            CreateFacility(HazardType.Fire, SeverityLevel.Severe, requiresShutdown: true),
            CreateFacility(HazardType.LOTOFailure, SeverityLevel.Severe, requiresShutdown: true),
        ]);

        EnterpriseOrchestrationOutput output = EnterpriseOrchestrationEngine.Orchestrate(input, EnterpriseOrchestrationRuleSet.Standard);

        output.EnterpriseAlerts.Should().Contain(a => a.Contains("shared supplier, shared utility"));
    }

    [Fact]
    public void Orchestrate_SingleMinorFacility_NoAlerts()
    {
        var input = new EnterpriseOrchestrationInput([CreateFacility(HazardType.Other, SeverityLevel.Minor)]);

        EnterpriseOrchestrationOutput output = EnterpriseOrchestrationEngine.Orchestrate(input, EnterpriseOrchestrationRuleSet.Standard);

        output.EnterpriseAlerts.Should().BeEmpty();
    }
}
