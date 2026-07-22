// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PermitToWork.PermitTemplate
//  Layer:          Domain
//  File:           PermitTemplatePublishedEvent.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Raised when a PermitTemplate is published and becomes available for
//                  requesting/issuing permits of its category.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PermitToWork.PermitTemplate.Events;

/// <summary>Raised when a <see cref="PermitTemplate"/> is published and made active.</summary>
public sealed record PermitTemplatePublishedEvent(
    PermitTemplateEventId EventId,
    PermitTemplateId      PermitTemplateId,
    UtcTimestamp          PublishedAtUtc,
    PerformedByActor      Actor,
    CorrelationId         CorrelationId,
    CausationId           CausationId
) : IPermitTemplateDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PermitTemplatePublished";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => PublishedAtUtc;
}
