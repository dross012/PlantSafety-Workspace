// ============================================================================
//  Project:        Enterprise.WorkflowOrchestration.Tests
//  File:           WorkflowOrchestrationTests.cs
//  Created:        2026-08-08
//  Description:    Phase D.6 unit tests for EnterpriseWorkflowOrchestrator:
//                  execution-plan ordering (Evacuation first, Restart
//                  last, stable within type) and guarded state transitions.
// ============================================================================
using Enterprise.WorkflowOrchestration.Abstractions.Ids;
using Enterprise.WorkflowOrchestration.Abstractions.Orchestrator;
using Enterprise.WorkflowOrchestration.Abstractions.States;
using Enterprise.WorkflowOrchestration.Abstractions.Steps;
using Enterprise.WorkflowOrchestration.Abstractions.Transitions;

using FluentAssertions;

using Xunit;

namespace Enterprise.WorkflowOrchestration.Tests.Orchestrator;

public sealed class WorkflowOrchestrationTests
{
    [Fact]
    public void BuildExecutionPlan_OrdersStepsByPriority_EvacuationFirstRestartLast()
    {
        var steps = new[]
        {
            new EnterpriseWorkflowStep(EnterpriseWorkflowStepType.RestartTrigger, "R1", "Restart"),
            new EnterpriseWorkflowStep(EnterpriseWorkflowStepType.RequiredAction, "A1", "Action"),
            new EnterpriseWorkflowStep(EnterpriseWorkflowStepType.EvacuationTrigger, "E1", "Evacuate"),
            new EnterpriseWorkflowStep(EnterpriseWorkflowStepType.SopTrigger, "S1", "SOP"),
            new EnterpriseWorkflowStep(EnterpriseWorkflowStepType.ShutdownTrigger, "SD1", "Shutdown"),
            new EnterpriseWorkflowStep(EnterpriseWorkflowStepType.PermitTrigger, "P1", "Permit"),
        };

        IReadOnlyList<EnterpriseWorkflowStep> plan = EnterpriseWorkflowOrchestrator.BuildExecutionPlan(steps);

        plan.Select(s => s.StepType).Should().Equal(
            EnterpriseWorkflowStepType.EvacuationTrigger,
            EnterpriseWorkflowStepType.ShutdownTrigger,
            EnterpriseWorkflowStepType.PermitTrigger,
            EnterpriseWorkflowStepType.SopTrigger,
            EnterpriseWorkflowStepType.RequiredAction,
            EnterpriseWorkflowStepType.RestartTrigger);
    }

    [Fact]
    public void BuildExecutionPlan_IsStableWithinSameStepType()
    {
        var steps = new[]
        {
            new EnterpriseWorkflowStep(EnterpriseWorkflowStepType.RequiredAction, "A1", "First"),
            new EnterpriseWorkflowStep(EnterpriseWorkflowStepType.RequiredAction, "A2", "Second"),
        };

        IReadOnlyList<EnterpriseWorkflowStep> plan = EnterpriseWorkflowOrchestrator.BuildExecutionPlan(steps);

        plan.Select(s => s.Code).Should().Equal("A1", "A2");
    }

    [Fact]
    public void Transition_PendingToInProgress_Succeeds()
    {
        var workflowId = EnterpriseWorkflowId.New();
        var now = DateTime.UtcNow;

        EnterpriseWorkflowTransition transition = EnterpriseWorkflowOrchestrator.Transition(workflowId, EnterpriseWorkflowState.Pending, EnterpriseWorkflowState.InProgress, now);

        transition.WorkflowId.Should().Be(workflowId);
        transition.FromState.Should().Be(EnterpriseWorkflowState.Pending);
        transition.ToState.Should().Be(EnterpriseWorkflowState.InProgress);
    }

    [Theory]
    [InlineData(EnterpriseWorkflowState.Completed, EnterpriseWorkflowState.InProgress)]
    [InlineData(EnterpriseWorkflowState.Pending, EnterpriseWorkflowState.Completed)]
    [InlineData(EnterpriseWorkflowState.Cancelled, EnterpriseWorkflowState.InProgress)]
    public void Transition_InvalidTransition_Throws(EnterpriseWorkflowState from, EnterpriseWorkflowState to)
    {
        Action act = () => EnterpriseWorkflowOrchestrator.Transition(EnterpriseWorkflowId.New(), from, to, DateTime.UtcNow);

        act.Should().Throw<InvalidOperationException>();
    }

    [Theory]
    [InlineData(EnterpriseWorkflowState.InProgress, EnterpriseWorkflowState.Completed)]
    [InlineData(EnterpriseWorkflowState.InProgress, EnterpriseWorkflowState.Failed)]
    [InlineData(EnterpriseWorkflowState.InProgress, EnterpriseWorkflowState.Cancelled)]
    [InlineData(EnterpriseWorkflowState.Pending, EnterpriseWorkflowState.Cancelled)]
    public void Transition_ValidTransitions_Succeed(EnterpriseWorkflowState from, EnterpriseWorkflowState to)
    {
        EnterpriseWorkflowTransition transition = EnterpriseWorkflowOrchestrator.Transition(EnterpriseWorkflowId.New(), from, to, DateTime.UtcNow);

        transition.ToState.Should().Be(to);
    }
}
