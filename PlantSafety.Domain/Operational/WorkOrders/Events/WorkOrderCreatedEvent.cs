// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.WorkOrders
//  Layer:          Domain
//  File:           WorkOrderCreatedEvent.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Raised when a new WorkOrder is created. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Operational.WorkOrders.ValueObjects.Ids;

namespace PlantSafety.Domain.Operational.WorkOrders.Events;

/// <summary>Raised when a new WorkOrder is created.</summary>
public sealed record WorkOrderCreatedEvent(
    Guid EventId,
    WorkOrderId WorkOrderId,
    Guid MachineId,
    string WorkOrderCode,
    UtcTimestamp CreatedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId) : IWorkOrderDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "WorkOrderCreated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => CreatedAtUtc;
}
