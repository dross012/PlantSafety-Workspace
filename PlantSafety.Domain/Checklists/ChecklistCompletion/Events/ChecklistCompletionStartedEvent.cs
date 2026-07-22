// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Checklists.ChecklistCompletion
//  Layer:          Domain
//  File:           ChecklistCompletionStartedEvent.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Raised when a ChecklistCompletion execution is started by an inspector.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Checklists.ChecklistCompletion.Events;

/// <summary>Raised when a <see cref="ChecklistCompletion"/> execution is started.</summary>
public sealed record ChecklistCompletionStartedEvent(
    ChecklistCompletionEventId EventId,
    ChecklistCompletionId ChecklistCompletionId,
    UtcTimestamp StartedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IChecklistCompletionDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ChecklistCompletionStarted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => StartedAtUtc;
}
