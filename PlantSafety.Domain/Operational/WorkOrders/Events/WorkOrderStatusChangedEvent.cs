// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.WorkOrders
//  Layer:          Domain
//  File:           WorkOrderStatusChangedEvent.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Raised for every WorkOrder lifecycle transition other than creation/assignment
//                  (Start/PutOnHold/Resume/Cancel). A single generic event carrying the new status
//                  keeps the event surface proportionate to this module's scope -- mirrors the
//                  single-event-per-transition style of the sibling PreventiveMaintenanceTask
//                  aggregate but consolidates the "pure status move" transitions into one shape
//                  since none of them carry materially different payloads. Completion is raised
//                  via the distinct WorkOrderCompletedEvent because it also carries completion
//                  notes. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Operational.WorkOrders.Enums;
using PlantSafety.Domain.Operational.WorkOrders.ValueObjects.Ids;

namespace PlantSafety.Domain.Operational.WorkOrders.Events;

/// <summary>Raised for a WorkOrder lifecycle transition (Start/PutOnHold/Resume/Cancel).</summary>
public sealed record WorkOrderStatusChangedEvent(
    Guid EventId,
    WorkOrderId WorkOrderId,
    WorkOrderStatus PreviousStatus,
    WorkOrderStatus NewStatus,
    string? Reason,
    UtcTimestamp ChangedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId) : IWorkOrderDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "WorkOrderStatusChanged";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ChangedAtUtc;
}
