// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Checklists.ChecklistTemplate
//  Layer:          Domain
//  File:           ChecklistTemplatePublishedEvent.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Raised when a ChecklistTemplate transitions from Created (draft) to
//                  Active (published, available for ChecklistCompletion execution).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Checklists.ChecklistTemplate.Events;

/// <summary>Raised when a <see cref="ChecklistTemplate"/> is published for active use.</summary>
public sealed record ChecklistTemplatePublishedEvent(
    ChecklistTemplateEventId EventId,
    ChecklistTemplateId ChecklistTemplateId,
    int ItemCount,
    UtcTimestamp PublishedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IChecklistTemplateDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ChecklistTemplatePublished";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => PublishedAtUtc;
}
