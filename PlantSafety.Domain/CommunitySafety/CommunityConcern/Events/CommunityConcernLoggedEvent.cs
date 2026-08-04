// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CommunitySafety.CommunityConcern
//  Layer:          Domain
//  File:           CommunityConcernLoggedEvent.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Raised when a new CommunityConcern aggregate is logged. Additive-only
//                  evolution.
// ============================================================================
using PlantSafety.Domain.CommunitySafety.CommunityConcern.Enums;
using PlantSafety.Domain.CommunitySafety.CommunityConcern.ValueObjects.Ids;

namespace PlantSafety.Domain.CommunitySafety.CommunityConcern.Events;

/// <summary>Raised when a new <see cref="Aggregates.CommunityConcern"/> aggregate is logged.</summary>
public sealed record CommunityConcernLoggedEvent(
    CommunityConcernEventId EventId,
    CommunityConcernId      CommunityConcernId,
    CommunityConcernCategory Category,
    string                   Severity,
    UtcTimestamp             LoggedAtUtc,
    PerformedByActor         Actor,
    CorrelationId            CorrelationId,
    CausationId              CausationId
) : ICommunityConcernDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "CommunityConcernLogged";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => LoggedAtUtc;
}
