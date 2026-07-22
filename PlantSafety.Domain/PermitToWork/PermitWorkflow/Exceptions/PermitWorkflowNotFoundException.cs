// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PermitToWork.PermitWorkflow
//  Layer:          Domain
//  File:           PermitWorkflowNotFoundException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a PermitWorkflow aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PermitToWork.PermitWorkflow.Exceptions;

/// <summary>Thrown when a <see cref="PermitWorkflow"/> aggregate lookup finds no match.</summary>
public sealed class PermitWorkflowNotFoundException : PermitWorkflowDomainException
{
    /// <summary>Creates a not-found exception for the given PermitWorkflow identity.</summary>
    public PermitWorkflowNotFoundException(PermitWorkflowId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"PermitWorkflow with id '{id}' was not found.",
            "PermitWorkflow.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}