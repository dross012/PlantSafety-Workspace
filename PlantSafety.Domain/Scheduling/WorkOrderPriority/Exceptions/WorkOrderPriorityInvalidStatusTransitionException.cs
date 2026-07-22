// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.WorkOrderPriority
//  Layer:          Domain
//  File:           WorkOrderPriorityInvalidStatusTransitionException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a WorkOrderPriority lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.WorkOrderPriority.Exceptions;

/// <summary>Thrown when a <see cref="WorkOrderPriority"/> lifecycle transition is not permitted.</summary>
public sealed class WorkOrderPriorityInvalidStatusTransitionException : WorkOrderPriorityDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public WorkOrderPriorityInvalidStatusTransitionException(
        WorkOrderPriorityId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot transition 'WorkOrderPriority' {id} from '{from}' to '{to}'.",
            "WorkOrderPriority.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}