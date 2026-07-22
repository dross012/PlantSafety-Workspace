// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scorecards
//  Layer:          Domain
//  File:           ScorecardCreatedEvent.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Raised when a new Scorecard aggregate is created.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Scorecards.ValueObjects.Ids;

namespace PlantSafety.Domain.Scorecards.Events;

/// <summary>Raised when a new Scorecard aggregate is created.</summary>
public sealed record ScorecardCreatedEvent(
    ScorecardEventId EventId,
    ScorecardId      ScorecardId,
    UtcTimestamp     CreatedAtUtc,
    PerformedByActor Actor,
    CorrelationId    CorrelationId,
    CausationId      CausationId
) : IScorecardDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ScorecardCreated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => CreatedAtUtc;
}