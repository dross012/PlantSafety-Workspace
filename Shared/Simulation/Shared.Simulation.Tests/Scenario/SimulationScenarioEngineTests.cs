// ============================================================================
//  Project:        Shared.Simulation.Tests
//  File:           SimulationScenarioEngineTests.cs
//  Created:        2026-08-10
// ============================================================================
using FluentAssertions;

using Shared.FacilityDomain.Abstractions.Hazards;
using Shared.FacilityDomain.Abstractions.Regulatory;
using Shared.FacilityDomain.Abstractions.Severity;
using Shared.PredictiveSafety.Abstractions.Audit;
using Shared.PredictiveSafety.Abstractions.Common;
using Shared.PredictiveSafety.Abstractions.Contracts;
using Shared.PredictiveSafety.Abstractions.Hazard;
using Shared.PredictiveSafety.Abstractions.Maintenance;
using Shared.PredictiveSafety.Abstractions.Routing;
using Shared.PredictiveSafety.Abstractions.Scheduling;
using Shared.SafetyGraph.Abstractions.Graph;
using Shared.Simulation.Abstractions.Scenario;
using Shared.Simulation.Abstractions.Simulation;

using Xunit;

namespace Shared.Simulation.Tests.Scenario;

public sealed class SimulationScenarioEngineTests
{
    private static PredictiveSafetyEvaluationResultContract MakePredictiveResult(
        bool hazardEscalates = false, bool maintenanceEscalates = false, bool routingEscalates = false, bool schedulingEscalates = false,
        PredictivePriority overallPriority = PredictivePriority.Low)
    {
        var metadata = new PredictiveSafetyContractMetadata(PredictiveSafetyContractMetadata.CurrentSchemaVersion, "acme", "corr-1", DateTime.UtcNow);
        var hazard = new PredictiveHazardDecision(HazardType.Fire, SeverityLevel.Minor, Array.Empty<RegulatoryJurisdiction>(), hazardEscalates, PredictivePriority.Low, Array.Empty<string>());
        var maintenance = new PredictiveMaintenanceDecision(Guid.NewGuid(), maintenanceEscalates, PredictivePriority.Low, Array.Empty<string>());
        var routing = new PredictiveRoutingDecision("Fire", Guid.NewGuid(), routingEscalates, PredictivePriority.Low, Array.Empty<string>());
        var scheduling = new PredictiveSchedulingDecision(schedulingEscalates, PredictivePriority.Low, Array.Empty<string>());
        var audit = new PredictiveAuditAnomaly(false, PredictivePriority.Low, Array.Empty<string>());
        return new PredictiveSafetyEvaluationResultContract(metadata, hazard, maintenance, routing, scheduling, audit, overallPriority);
    }

    private static SimulationScenario ScenarioWithSteps(params SimulationStepType[] stepTypes)
    {
        var steps = new List<SimulationStep>();
        for (int i = 0; i < stepTypes.Length; i++)
        {
            steps.Add(new SimulationStep(i, stepTypes[i], stepTypes[i].ToString()));
        }

        return new SimulationScenario("scenario-1", "Test", steps);
    }

    [Fact]
    public void Evaluate_NoEscalatingSignals_ReturnsLowPriorityNoEscalation()
    {
        var context = new SimulationScenarioContext(ScenarioWithSteps(SimulationStepType.Hazard), MakePredictiveResult(), null);

        SimulationScenarioResult result = SimulationScenarioEngine.Evaluate(context);

        result.RequiresScenarioWideEscalation.Should().BeFalse();
        result.OverallPriority.Should().Be(PredictivePriority.Low);
    }

    [Fact]
    public void Evaluate_HazardStepWithEscalatingPredictiveResult_Escalates()
    {
        var context = new SimulationScenarioContext(ScenarioWithSteps(SimulationStepType.Hazard), MakePredictiveResult(hazardEscalates: true), null);

        SimulationScenarioResult result = SimulationScenarioEngine.Evaluate(context);

        result.RequiresScenarioWideEscalation.Should().BeTrue();
        result.AppliedRuleCodes.Should().Contain("HAZARD-STEP-ESCALATION");
    }

    [Fact]
    public void Evaluate_PredictiveInferenceStepWithHighOverallPriority_Escalates()
    {
        var context = new SimulationScenarioContext(
            ScenarioWithSteps(SimulationStepType.PredictiveInference), MakePredictiveResult(overallPriority: PredictivePriority.High), null);

        SimulationScenarioResult result = SimulationScenarioEngine.Evaluate(context);

        result.AppliedRuleCodes.Should().Contain("PREDICTIVE-INFERENCE-STEP-ESCALATION");
    }

    [Fact]
    public void Evaluate_WorkflowStepWithGraphPropagation_Escalates()
    {
        var workflowNode = new SafetyGraphNode("Workflow:1", SafetyGraphNodeType.Workflow, "wf", new Dictionary<string, string>());
        var targetNode = new SafetyGraphNode("RoutingDecision:1", SafetyGraphNodeType.RoutingDecision, "rd", new Dictionary<string, string>());
        var edge = new SafetyGraphEdge("Workflow:1->RoutingDecision:1", "Workflow:1", "RoutingDecision:1", SafetyGraphEdgeType.Dependency, "");
        var graph = new SafetyGraphModel(new SafetyGraphMetadata(1, "acme", "corr-1", DateTime.UtcNow), new[] { workflowNode, targetNode }, new[] { edge });

        var context = new SimulationScenarioContext(ScenarioWithSteps(SimulationStepType.Workflow), null, graph);

        SimulationScenarioResult result = SimulationScenarioEngine.Evaluate(context);

        result.AppliedRuleCodes.Should().Contain("WORKFLOW-STEP-ESCALATION");
    }

    [Fact]
    public void Evaluate_WorkflowStepWithNullGraph_DoesNotEscalate()
    {
        var context = new SimulationScenarioContext(ScenarioWithSteps(SimulationStepType.Workflow), null, null);

        SimulationScenarioEngine.Evaluate(context).RequiresScenarioWideEscalation.Should().BeFalse();
    }

    [Fact]
    public void Evaluate_StepsEvaluatedInAscendingStepIndexOrder()
    {
        var steps = new[]
        {
            new SimulationStep(1, SimulationStepType.Routing, "second"),
            new SimulationStep(0, SimulationStepType.Hazard, "first"),
        };
        var scenario = new SimulationScenario("scenario-1", "Test", steps);
        var context = new SimulationScenarioContext(scenario, MakePredictiveResult(), null);

        SimulationScenarioResult result = SimulationScenarioEngine.Evaluate(context);

        result.StepOutcomes.Select(o => o.StepIndex).Should().Equal(0, 1);
    }

    [Fact]
    public void Evaluate_NullContext_Throws()
    {
        Action act = () => SimulationScenarioEngine.Evaluate(null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
