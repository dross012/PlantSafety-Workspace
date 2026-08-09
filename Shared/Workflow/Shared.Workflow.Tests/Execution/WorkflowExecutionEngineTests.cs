// ============================================================================
//  Project:        Shared.Workflow.Tests
//  File:           WorkflowExecutionEngineTests.cs
//  Created:        2026-08-09
// ============================================================================
using AwesomeAssertions;

using Shared.Workflow.Abstractions.Envelope;
using Shared.Workflow.Abstractions.Execution;

using Xunit;

namespace Shared.Workflow.Tests.Execution;

public sealed class WorkflowExecutionEngineTests
{
    [Fact]
    public void Execute_AllStepsSucceed_EveryStepIsCompleted()
    {
        string[] plan = ["Evacuate", "NotifySupervisor", "Shutdown"];
        var workflowId = Guid.NewGuid();

        IReadOnlyList<WorkflowStepEnvelope<string>> result = WorkflowExecutionEngine.Execute(
            plan, [true, true, true], workflowId, "corr-1", "WorkflowEngine", Guid.NewGuid);

        result.Should().OnlyContain(e => e.Status == WorkflowStepStatus.Completed);
        result.Should().OnlyContain(e => e.WorkflowId == workflowId && e.CorrelationId == "corr-1" && e.Module == "WorkflowEngine");
    }

    [Fact]
    public void Execute_MiddleStepFails_PriorStepsRolledBackAndLaterStepsSkipped()
    {
        string[] plan = ["Evacuate", "NotifySupervisor", "Shutdown"];

        IReadOnlyList<WorkflowStepEnvelope<string>> result = WorkflowExecutionEngine.Execute(
            plan, [true, false, true], Guid.NewGuid(), "corr-2", "WorkflowEngine", Guid.NewGuid);

        result[0].Status.Should().Be(WorkflowStepStatus.RolledBack);
        result[1].Status.Should().Be(WorkflowStepStatus.Failed);
        result[2].Status.Should().Be(WorkflowStepStatus.Skipped);
    }

    [Fact]
    public void Execute_FirstStepFails_NothingToRollBackAndRestSkipped()
    {
        string[] plan = ["Evacuate", "NotifySupervisor"];

        IReadOnlyList<WorkflowStepEnvelope<string>> result = WorkflowExecutionEngine.Execute(
            plan, [false, true], Guid.NewGuid(), "corr-3", "WorkflowEngine", Guid.NewGuid);

        result[0].Status.Should().Be(WorkflowStepStatus.Failed);
        result[1].Status.Should().Be(WorkflowStepStatus.Skipped);
    }

    [Fact]
    public void Execute_EveryStepGetsAUniqueStepId()
    {
        string[] plan = ["A", "B", "C"];

        IReadOnlyList<WorkflowStepEnvelope<string>> result = WorkflowExecutionEngine.Execute(
            plan, [true, true, true], Guid.NewGuid(), "corr-4", "WorkflowEngine", Guid.NewGuid);

        result.Select(e => e.StepId).Distinct().Should().HaveCount(3);
    }

    [Fact]
    public void Execute_MismatchedPlanAndOutcomeCounts_Throws()
    {
        string[] plan = ["A", "B"];

        Action act = () => WorkflowExecutionEngine.Execute(plan, [true], Guid.NewGuid(), "corr-5", "WorkflowEngine", Guid.NewGuid);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Execute_EmptyPlan_ReturnsEmptyResult()
    {
        IReadOnlyList<WorkflowStepEnvelope<string>> result = WorkflowExecutionEngine.Execute(
            Array.Empty<string>(), Array.Empty<bool>(), Guid.NewGuid(), "corr-6", "WorkflowEngine", Guid.NewGuid);

        result.Should().BeEmpty();
    }
}
