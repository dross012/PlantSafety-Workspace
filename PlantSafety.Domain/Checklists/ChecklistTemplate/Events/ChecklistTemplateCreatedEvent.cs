// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Checklists.ChecklistTemplate
//  Layer:          Domain
//  File:           ChecklistTemplateCreatedEvent.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Raised when a new ChecklistTemplate aggregate is created.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Checklists.ChecklistTemplate.Events;

/// <summary>Raised when a new <see cref="ChecklistTemplate"/> aggregate is created.</summary>
public sealed record ChecklistTemplateCreatedEvent(
    ChecklistTemplateEventId EventId,
    ChecklistTemplateId      ChecklistTemplateId,
    UtcTimestamp  CreatedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId   CausationId
) : IChecklistTemplateDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ChecklistTemplateCreated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => CreatedAtUtc;
}