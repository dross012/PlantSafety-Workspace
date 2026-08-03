// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.WorkOrders
//  Layer:          Domain
//  File:           WorkOrderCompletedEvent.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Raised when a WorkOrder is completed (InProgress -> Completed). Additive-only
//                  evolution.
// ============================================================================
using PlantSafety.Domain.Operational.WorkOrders.ValueObjects.Ids;

namespace PlantSafety.Domain.Operational.WorkOrders.Events;

/// <summary>Raised when a WorkOrder is completed.</summary>
public sealed record WorkOrderCompletedEvent(
    Guid EventId,
    WorkOrderId WorkOrderId,
    string? CompletionNotes,
    UtcTimestamp CompletedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId) : IWorkOrderDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "WorkOrderCompleted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => CompletedAtUtc;
}
