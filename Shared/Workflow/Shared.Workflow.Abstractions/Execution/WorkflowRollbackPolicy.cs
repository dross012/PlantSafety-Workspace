// ============================================================================
//  Project:        Shared.Workflow
//  Module:         Abstractions.Execution
//  File:           WorkflowRollbackPolicy.cs
//  Created:        2026-08-09
//  Description:    Phase 2.8 deliverable: deterministic rollback rules.
//                  Pure function: given the total step count and which
//                  step failed, returns exactly which already-completed
//                  steps must be rolled back, and in what order
//                  (compensate the most recently completed step first --
//                  the standard "reverse of forward order" rollback rule).
// ============================================================================
namespace Shared.Workflow.Abstractions.Execution;

/// <summary>Deterministic rollback rules for a failed workflow step.</summary>
public static class WorkflowRollbackPolicy
{
    /// <summary>
    /// Returns the indices (into a 0-based step list) that must be rolled back when the
    /// step at <paramref name="failedStepIndex"/> fails, in the order they should be
    /// compensated (most recently completed first).
    /// </summary>
    public static IReadOnlyList<int> DetermineRollbackIndices(int totalSteps, int failedStepIndex)
    {
        if (totalSteps < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(totalSteps), totalSteps, "Total step count cannot be negative.");
        }

        if (failedStepIndex < 0 || failedStepIndex >= totalSteps)
        {
            throw new ArgumentOutOfRangeException(nameof(failedStepIndex), failedStepIndex, $"Failed step index must be within [0, {totalSteps}).");
        }

        var indices = new List<int>();
        for (int i = failedStepIndex - 1; i >= 0; i--)
        {
            indices.Add(i);
        }

        return indices;
    }
}
