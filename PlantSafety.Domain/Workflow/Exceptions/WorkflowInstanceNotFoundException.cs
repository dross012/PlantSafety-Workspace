// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Domain
//  File:           WorkflowInstanceNotFoundException.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a referenced WorkflowInstanceId cannot be resolved.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.Domain.Workflow.ValueObjects.Ids;

namespace PlantSafety.Domain.Workflow.Exceptions;

/// <summary>Thrown when a referenced WorkflowInstanceId cannot be resolved.</summary>
public sealed class WorkflowInstanceNotFoundException : WorkflowDomainException
{
    /// <summary>Creates an instance-not-found exception.</summary>
    public WorkflowInstanceNotFoundException(WorkflowInstanceId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"WorkflowInstance {id} was not found.",
            "Workflow.InstanceNotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
