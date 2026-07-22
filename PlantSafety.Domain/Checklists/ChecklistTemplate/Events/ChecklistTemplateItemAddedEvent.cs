// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Checklists.ChecklistTemplate
//  Layer:          Domain
//  File:           ChecklistTemplateItemAddedEvent.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Raised when a new inspection line item is added to a ChecklistTemplate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Checklists.ChecklistTemplate.Events;

/// <summary>Raised when a new line item is added to a <see cref="ChecklistTemplate"/>.</summary>
public sealed record ChecklistTemplateItemAddedEvent(
    ChecklistTemplateEventId EventId,
    ChecklistTemplateId ChecklistTemplateId,
    Guid ItemId,
    int Sequence,
    bool IsCritical,
    UtcTimestamp AddedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IChecklistTemplateDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ChecklistTemplateItemAdded";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => AddedAtUtc;
}
