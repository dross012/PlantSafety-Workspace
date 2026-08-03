// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Domain
//  File:           WorkflowTimeoutPolicyEvaluator.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Domain policy determining whether an active WorkflowStepInstance has exceeded
//                  its configured TimeoutPolicy window (Phase 5.2 timeout rules). Pure function of
//                  its inputs -- never reads the wall clock.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Workflow.Entities;
using PlantSafety.Domain.Workflow.ValueObjects;

namespace PlantSafety.Domain.Workflow.Policies;

/// <summary>Determines whether an active workflow step has exceeded its configured timeout window.</summary>
public static class WorkflowTimeoutPolicyEvaluator
{
    /// <summary>
    /// Returns <see langword="true"/> when <paramref name="step"/> is Active or Retrying, carries a
    /// non-null <paramref name="timeout"/> policy, and has exceeded it as of <paramref name="nowUtc"/>.
    /// </summary>
    public static bool HasTimedOut(WorkflowStepInstance step, TimeoutPolicy? timeout, UtcTimestamp nowUtc)
    {
        if (timeout is null)
            return false;

        if (step.Status is not (Enums.WorkflowStepStatus.Active or Enums.WorkflowStepStatus.Retrying))
            return false;

        if (step.ActivatedAtUtc is not { } activatedAt)
            return false;

        return timeout.Value.HasTimedOut(activatedAt, nowUtc);
    }
}
