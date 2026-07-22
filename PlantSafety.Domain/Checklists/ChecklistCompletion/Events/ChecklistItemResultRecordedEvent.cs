// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Checklists.ChecklistCompletion
//  Layer:          Domain
//  File:           ChecklistItemResultRecordedEvent.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Raised each time an inspector records a pass/fail result for a single
//                  checklist line item during a ChecklistCompletion execution.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Checklists.ChecklistCompletion.Events;

/// <summary>Raised when a single item result is recorded during a <see cref="ChecklistCompletion"/>.</summary>
public sealed record ChecklistItemResultRecordedEvent(
    ChecklistCompletionEventId EventId,
    ChecklistCompletionId ChecklistCompletionId,
    Guid ChecklistTemplateItemId,
    bool Passed,
    UtcTimestamp RecordedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IChecklistCompletionDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ChecklistItemResultRecorded";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => RecordedAtUtc;
}
