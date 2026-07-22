// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Checklists.ChecklistCompletion
//  Layer:          Domain
//  File:           ChecklistCompletionCreatedEvent.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Raised when a new ChecklistCompletion aggregate is created.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Checklists.ChecklistCompletion.Events;

/// <summary>Raised when a new <see cref="ChecklistCompletion"/> aggregate is created.</summary>
public sealed record ChecklistCompletionCreatedEvent(
    ChecklistCompletionEventId EventId,
    ChecklistCompletionId      ChecklistCompletionId,
    UtcTimestamp  CreatedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId   CausationId
) : IChecklistCompletionDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ChecklistCompletionCreated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => CreatedAtUtc;
}