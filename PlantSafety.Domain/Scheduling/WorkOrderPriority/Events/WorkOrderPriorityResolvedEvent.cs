// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.WorkOrderPriority
//  Layer:          Domain
//  File:           WorkOrderPriorityResolvedEvent.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Raised when the hazard addressed by a work order has been resolved.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.WorkOrderPriority.Events;

/// <summary>Raised when a <see cref="WorkOrderPriority"/> record is resolved.</summary>
public sealed record WorkOrderPriorityResolvedEvent(
    WorkOrderPriorityEventId EventId,
    WorkOrderPriorityId      WorkOrderPriorityId,
    string?                  ResolutionNotes,
    UtcTimestamp             ResolvedAtUtc,
    PerformedByActor         Actor,
    CorrelationId            CorrelationId,
    CausationId              CausationId
) : IWorkOrderPriorityDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "WorkOrderPriorityResolved";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ResolvedAtUtc;
}
