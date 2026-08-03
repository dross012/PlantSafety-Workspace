// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Domain
//  File:           WorkflowEscalationPolicy.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Domain policy resolving the escalation target for a timed-out workflow step
//                  (Phase 5.2 timeout rules: "a workflow instance can time out a step and
//                  escalate").
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Workflow.Entities;
using PlantSafety.Domain.Workflow.ValueObjects;

namespace PlantSafety.Domain.Workflow.Policies;

/// <summary>Resolves the escalation target for a timed-out workflow step.</summary>
public static class WorkflowEscalationPolicy
{
    /// <summary>Platform-wide fallback escalation role used when a step defines no explicit escalation actor.</summary>
    public const string FallbackEscalationRole = "SafetyManager";

    /// <summary>
    /// Returns the step definition's configured <see cref="WorkflowStepDefinition.EscalationActor"/>
    /// when one is assigned, otherwise falls back to <see cref="FallbackEscalationRole"/>.
    /// </summary>
    public static AssignedActor ResolveEscalationTarget(WorkflowStepDefinition stepDefinition)
        => stepDefinition.EscalationActor.RoleCode is not null || stepDefinition.EscalationActor.UserId is not null
            ? stepDefinition.EscalationActor
            : AssignedActor.ForRole(FallbackEscalationRole);
}
