// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.WorkOrderPriority
//  Layer:          Domain
//  File:           WorkOrderPriorityEscalatedEvent.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Raised when a P1/P2 work order passes its priority-based due window unresolved
//                  and is automatically escalated ("Automatic escalation for overdue P1/P2
//                  items").
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.WorkOrderPriority.Events;

/// <summary>Raised when a <see cref="WorkOrderPriority"/> record is escalated for being overdue.</summary>
public sealed record WorkOrderPriorityEscalatedEvent(
    WorkOrderPriorityEventId EventId,
    WorkOrderPriorityId      WorkOrderPriorityId,
    string                   PriorityCode,
    string                   EscalationReason,
    UtcTimestamp             EscalatedAtUtc,
    PerformedByActor         Actor,
    CorrelationId            CorrelationId,
    CausationId              CausationId
) : IWorkOrderPriorityDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "WorkOrderPriorityEscalated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => EscalatedAtUtc;
}
