// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Checklists.ChecklistCompletion
//  Layer:          Domain
//  File:           ChecklistItemCriticalFailureFlaggedEvent.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Raised when an inspector flags a checklist item failure as a critical /
//                  stop-work condition (e.g. missing LOTO device, disabled interlock,
//                  failed emergency stop). Downstream consumers (Auditing, SafetyCulture
//                  StopWorkEvent, Alerts) may react to this event.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Checklists.ChecklistCompletion.Events;

/// <summary>Raised when a checklist item failure is flagged as critical/stop-work.</summary>
public sealed record ChecklistItemCriticalFailureFlaggedEvent(
    ChecklistCompletionEventId EventId,
    ChecklistCompletionId ChecklistCompletionId,
    Guid ChecklistTemplateItemId,
    string Description,
    UtcTimestamp FlaggedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IChecklistCompletionDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ChecklistItemCriticalFailureFlagged";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => FlaggedAtUtc;
}
