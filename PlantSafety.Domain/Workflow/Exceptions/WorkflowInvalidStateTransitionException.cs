// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Domain
//  File:           WorkflowInvalidStateTransitionException.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a WorkflowInstance lifecycle transition is not permitted from its
//                  current state.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.Domain.Workflow.ValueObjects.Ids;

namespace PlantSafety.Domain.Workflow.Exceptions;

/// <summary>Thrown when a WorkflowInstance lifecycle transition is not permitted from its current state.</summary>
public sealed class WorkflowInvalidStateTransitionException : WorkflowDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public WorkflowInvalidStateTransitionException(
        WorkflowInstanceId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot transition WorkflowInstance {id} from '{from}' to '{to}'.",
            "Workflow.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
