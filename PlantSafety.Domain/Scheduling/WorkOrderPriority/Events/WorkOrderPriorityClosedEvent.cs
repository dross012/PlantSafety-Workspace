// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.WorkOrderPriority
//  Layer:          Domain
//  File:           WorkOrderPriorityClosedEvent.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Raised when a resolved work order is closed with no further action required.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.WorkOrderPriority.Events;

/// <summary>Raised when a <see cref="WorkOrderPriority"/> record is closed.</summary>
public sealed record WorkOrderPriorityClosedEvent(
    WorkOrderPriorityEventId EventId,
    WorkOrderPriorityId      WorkOrderPriorityId,
    UtcTimestamp             ClosedAtUtc,
    PerformedByActor         Actor,
    CorrelationId            CorrelationId,
    CausationId              CausationId
) : IWorkOrderPriorityDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "WorkOrderPriorityClosed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ClosedAtUtc;
}
