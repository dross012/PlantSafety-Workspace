// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         RiskManagement
//  Layer:          Domain
//  File:           RiskCreatedEvent.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Raised when a new Risk aggregate is created.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.RiskManagement.ValueObjects.Ids;

namespace PlantSafety.Domain.RiskManagement.Events;

/// <summary>Raised when a new Risk aggregate is created.</summary>
public sealed record RiskCreatedEvent(
    RiskEventId EventId,
    RiskId      RiskId,
    UtcTimestamp     CreatedAtUtc,
    PerformedByActor Actor,
    CorrelationId    CorrelationId,
    CausationId      CausationId
) : IRiskDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "RiskCreated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => CreatedAtUtc;
}