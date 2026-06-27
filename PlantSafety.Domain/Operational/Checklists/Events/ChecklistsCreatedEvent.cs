// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.Checklists
//  Layer:          Domain
//  File:           ChecklistsCreatedEvent.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.Checklists.Events;

/// <summary>Raised when a new <see cref="Checklists"/> aggregate is created.</summary>
public sealed record ChecklistsCreatedEvent(
    ChecklistsEventId EventId,
    ChecklistsId      ChecklistsId,
    UtcTimestamp  CreatedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId   CausationId
) : IChecklistsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ChecklistsCreated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => CreatedAtUtc;
}
