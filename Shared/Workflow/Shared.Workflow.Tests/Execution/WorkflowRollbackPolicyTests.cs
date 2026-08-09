// ============================================================================
//  Project:        Shared.Workflow.Tests
//  File:           WorkflowRollbackPolicyTests.cs
//  Created:        2026-08-09
// ============================================================================
using AwesomeAssertions;

using Shared.Workflow.Abstractions.Execution;

using Xunit;

namespace Shared.Workflow.Tests.Execution;

public sealed class WorkflowRollbackPolicyTests
{
    [Fact]
    public void DetermineRollbackIndices_FailureAtStepZero_ReturnsNoRollbacks()
    {
        WorkflowRollbackPolicy.DetermineRollbackIndices(5, 0).Should().BeEmpty();
    }

    [Fact]
    public void DetermineRollbackIndices_FailureAtLastStep_RollsBackEveryPriorStepInReverseOrder()
    {
        IReadOnlyList<int> indices = WorkflowRollbackPolicy.DetermineRollbackIndices(4, 3);

        indices.Should().BeEquivalentTo([2, 1, 0], options => options.WithStrictOrdering());
    }

    [Fact]
    public void DetermineRollbackIndices_FailureInMiddle_RollsBackOnlyPriorSteps()
    {
        IReadOnlyList<int> indices = WorkflowRollbackPolicy.DetermineRollbackIndices(5, 2);

        indices.Should().BeEquivalentTo([1, 0], options => options.WithStrictOrdering());
    }

    [Fact]
    public void DetermineRollbackIndices_FailedIndexOutOfRange_Throws()
    {
        Action act = () => WorkflowRollbackPolicy.DetermineRollbackIndices(3, 3);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void DetermineRollbackIndices_NegativeTotalSteps_Throws()
    {
        Action act = () => WorkflowRollbackPolicy.DetermineRollbackIndices(-1, 0);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
