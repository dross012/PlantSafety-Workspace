// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.WorkOrders
//  Layer:          Domain
//  File:           WorkOrderInvalidStatusTransitionException.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Thrown when a WorkOrder lifecycle transition is not permitted. Additive-only
//                  evolution.
// ============================================================================
using PlantSafety.Domain.Operational.WorkOrders.ValueObjects.Ids;

namespace PlantSafety.Domain.Operational.WorkOrders.Exceptions;

/// <summary>Thrown when a WorkOrder lifecycle transition is not permitted.</summary>
public sealed class WorkOrderInvalidStatusTransitionException : WorkOrderDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public WorkOrderInvalidStatusTransitionException(
        WorkOrderId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot transition WorkOrder '{id}' from '{from}' to '{to}'.",
            "WorkOrder.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
