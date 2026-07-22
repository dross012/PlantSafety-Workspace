// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Checklists.ChecklistCompletion
//  Layer:          Domain
//  File:           ChecklistCompletionCompletedEvent.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Raised when a ChecklistCompletion execution is finished and its overall
//                  outcome (Pass/Fail/PartialWithFindings) is determined.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Checklists.ChecklistCompletion.Events;

/// <summary>Raised when a <see cref="ChecklistCompletion"/> execution is completed.</summary>
public sealed record ChecklistCompletionCompletedEvent(
    ChecklistCompletionEventId EventId,
    ChecklistCompletionId ChecklistCompletionId,
    string Outcome,
    int ItemsTotal,
    int ItemsPassed,
    int ItemsFailed,
    UtcTimestamp CompletedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IChecklistCompletionDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ChecklistCompletionCompleted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => CompletedAtUtc;
}
