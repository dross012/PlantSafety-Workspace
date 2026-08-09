// ============================================================================
//  Project:        AI.WorkflowAutomation.Tests
//  File:           WorkflowAutomationEngineTests.cs
//  Created:        2026-08-08
//  Description:    Phase E.3 unit tests for WorkflowAutomationEngine.Automate:
//                  required-action/SOP/permit/evacuation generation
//                  (delegated to Shared.WorkflowEngine), shutdown/restart
//                  rule generation, and execution-plan ordering.
// ============================================================================
using FluentAssertions;

using Shared.FacilityDomain.Abstractions.Hazards;
using Shared.FacilityDomain.Abstractions.Severity;

using Shared.WorkflowEngine.Abstractions.Engine;
using Shared.WorkflowEngine.Abstractions.Triggers;

using Enterprise.WorkflowOrchestration.Abstractions.Steps;

using AI.WorkflowAutomation.Abstractions.Automation;
using AI.WorkflowAutomation.Abstractions.Engine;
using AI.WorkflowAutomation.Abstractions.Rules;

using Xunit;

namespace AI.WorkflowAutomation.Tests.Engine;

public sealed class WorkflowAutomationEngineTests
{
    private static WorkflowAutomationInput CreateInput(bool requiresShutdown = false, bool requiresRestart = false) =>
        new(
            new WorkflowContext(HazardType.Fire, SeverityLevel.Severe, EvacuationScope.Building, EmployeeExposureCount: 0, MachineFailureCount: 0, EventChainLength: 1),
            requiresShutdown,
            requiresRestart);

    [Fact]
    public void Automate_SevereFireContext_GeneratesEvacuationAndSopSteps()
    {
        WorkflowAutomationOutput output = WorkflowAutomationEngine.Automate(CreateInput(), []);

        output.EvacuationSteps.Should().ContainSingle();
        output.SopSteps.Should().Contain(t => t.SopCode == "FIRE-RESPONSE-SOP");
    }

    [Fact]
    public void Automate_RequiresShutdown_MatchingRule_GeneratesShutdownStep()
    {
        var rule = new WorkflowAutomationRule(
            "R1", "Shutdown on request",
            i => i.RequiresShutdown,
            new EnterpriseWorkflowStep(EnterpriseWorkflowStepType.ShutdownTrigger, "SHUTDOWN-1", "Requested shutdown."));

        WorkflowAutomationOutput output = WorkflowAutomationEngine.Automate(CreateInput(requiresShutdown: true), [rule]);

        output.ShutdownRestartSequence.Should().ContainSingle(s => s.StepType == EnterpriseWorkflowStepType.ShutdownTrigger);
    }

    [Fact]
    public void Automate_RequiresShutdownFalse_DoesNotGenerateShutdownStep()
    {
        var rule = new WorkflowAutomationRule(
            "R1", "Shutdown on request",
            i => i.RequiresShutdown,
            new EnterpriseWorkflowStep(EnterpriseWorkflowStepType.ShutdownTrigger, "SHUTDOWN-1", "Requested shutdown."));

        WorkflowAutomationOutput output = WorkflowAutomationEngine.Automate(CreateInput(requiresShutdown: false), [rule]);

        output.ShutdownRestartSequence.Should().BeEmpty();
    }

    [Fact]
    public void Automate_ExecutionPlan_OrdersEvacuationBeforeSop()
    {
        WorkflowAutomationOutput output = WorkflowAutomationEngine.Automate(CreateInput(), []);

        int evacuationIndex = IndexOfType(output.ExecutionPlan, EnterpriseWorkflowStepType.EvacuationTrigger);
        int sopIndex = IndexOfType(output.ExecutionPlan, EnterpriseWorkflowStepType.SopTrigger);

        evacuationIndex.Should().BeLessThan(sopIndex);
    }

    private static int IndexOfType(IReadOnlyList<EnterpriseWorkflowStep> plan, EnterpriseWorkflowStepType type) =>
        plan.Select((step, index) => (step, index)).First(pair => pair.step.StepType == type).index;
}
