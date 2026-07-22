// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.WorkOrderPriority
//  Layer:          Domain
//  File:           WorkOrderPriorityNotFoundException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a WorkOrderPriority aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.WorkOrderPriority.Exceptions;

/// <summary>Thrown when a <see cref="WorkOrderPriority"/> aggregate lookup finds no match.</summary>
public sealed class WorkOrderPriorityNotFoundException : WorkOrderPriorityDomainException
{
    /// <summary>Creates a not-found exception for the given WorkOrderPriority identity.</summary>
    public WorkOrderPriorityNotFoundException(WorkOrderPriorityId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"WorkOrderPriority with id '{id}' was not found.",
            "WorkOrderPriority.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}