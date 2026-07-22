// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PermitToWork.PermitTemplate
//  Layer:          Domain
//  File:           PermitTemplateRetiredEvent.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Raised when a deprecated PermitTemplate is retired and is no longer usable
//                  for any purpose.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PermitToWork.PermitTemplate.Events;

/// <summary>Raised when a <see cref="PermitTemplate"/> is retired.</summary>
public sealed record PermitTemplateRetiredEvent(
    PermitTemplateEventId EventId,
    PermitTemplateId      PermitTemplateId,
    UtcTimestamp          RetiredAtUtc,
    PerformedByActor      Actor,
    CorrelationId         CorrelationId,
    CausationId           CausationId
) : IPermitTemplateDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PermitTemplateRetired";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => RetiredAtUtc;
}
