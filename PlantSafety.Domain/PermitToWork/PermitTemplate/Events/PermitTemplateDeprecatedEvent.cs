// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PermitToWork.PermitTemplate
//  Layer:          Domain
//  File:           PermitTemplateDeprecatedEvent.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Raised when a PermitTemplate is deprecated: existing permits may run to
//                  completion, but no new permits may be requested against the template.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PermitToWork.PermitTemplate.Events;

/// <summary>Raised when a <see cref="PermitTemplate"/> is deprecated.</summary>
public sealed record PermitTemplateDeprecatedEvent(
    PermitTemplateEventId EventId,
    PermitTemplateId      PermitTemplateId,
    string                Reason,
    UtcTimestamp          DeprecatedAtUtc,
    PerformedByActor      Actor,
    CorrelationId         CorrelationId,
    CausationId           CausationId
) : IPermitTemplateDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PermitTemplateDeprecated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => DeprecatedAtUtc;
}
