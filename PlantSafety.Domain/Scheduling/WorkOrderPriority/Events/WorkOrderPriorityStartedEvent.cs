// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.WorkOrderPriority
//  Layer:          Domain
//  File:           WorkOrderPriorityStartedEvent.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Raised when work begins against a prioritized work order.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.WorkOrderPriority.Events;

/// <summary>Raised when work begins against a <see cref="WorkOrderPriority"/> record.</summary>
public sealed record WorkOrderPriorityStartedEvent(
    WorkOrderPriorityEventId EventId,
    WorkOrderPriorityId      WorkOrderPriorityId,
    UtcTimestamp             StartedAtUtc,
    PerformedByActor         Actor,
    CorrelationId            CorrelationId,
    CausationId              CausationId
) : IWorkOrderPriorityDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "WorkOrderPriorityStarted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => StartedAtUtc;
}
