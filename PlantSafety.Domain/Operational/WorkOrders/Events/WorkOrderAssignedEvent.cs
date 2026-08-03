// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.WorkOrders
//  Layer:          Domain
//  File:           WorkOrderAssignedEvent.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Raised when a technician is (re)assigned to a WorkOrder. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Operational.WorkOrders.ValueObjects.Ids;

namespace PlantSafety.Domain.Operational.WorkOrders.Events;

/// <summary>Raised when a technician is (re)assigned to a WorkOrder.</summary>
public sealed record WorkOrderAssignedEvent(
    Guid EventId,
    WorkOrderId WorkOrderId,
    Guid TechnicianId,
    string TechnicianName,
    UtcTimestamp AssignedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId) : IWorkOrderDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "WorkOrderAssigned";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => AssignedAtUtc;
}
