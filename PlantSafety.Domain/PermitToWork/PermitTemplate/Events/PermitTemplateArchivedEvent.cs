// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PermitToWork.PermitTemplate
//  Layer:          Domain
//  File:           PermitTemplateArchivedEvent.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Raised when a retired PermitTemplate is archived for long-term compliance
//                  retention.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PermitToWork.PermitTemplate.Events;

/// <summary>Raised when a <see cref="PermitTemplate"/> is archived.</summary>
public sealed record PermitTemplateArchivedEvent(
    PermitTemplateEventId EventId,
    PermitTemplateId      PermitTemplateId,
    UtcTimestamp          ArchivedAtUtc,
    PerformedByActor      Actor,
    CorrelationId         CorrelationId,
    CausationId           CausationId
) : IPermitTemplateDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PermitTemplateArchived";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ArchivedAtUtc;
}
