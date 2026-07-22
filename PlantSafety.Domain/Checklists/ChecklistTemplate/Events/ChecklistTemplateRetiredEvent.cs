// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Checklists.ChecklistTemplate
//  Layer:          Domain
//  File:           ChecklistTemplateRetiredEvent.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Raised when a ChecklistTemplate is retired (archived) and is no longer
//                  available for new ChecklistCompletion executions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Checklists.ChecklistTemplate.Events;

/// <summary>Raised when a <see cref="ChecklistTemplate"/> is retired from active use.</summary>
public sealed record ChecklistTemplateRetiredEvent(
    ChecklistTemplateEventId EventId,
    ChecklistTemplateId ChecklistTemplateId,
    string Reason,
    UtcTimestamp RetiredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IChecklistTemplateDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ChecklistTemplateRetired";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => RetiredAtUtc;
}
