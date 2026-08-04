// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CommunitySafety.CommunityConcern
//  Layer:          Domain
//  File:           CommunityConcernResolvedEvent.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Raised when a CommunityConcern aggregate transitions to Resolved. Feeds the
//                  concern-resolution-time KPI. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.CommunitySafety.CommunityConcern.ValueObjects.Ids;

namespace PlantSafety.Domain.CommunitySafety.CommunityConcern.Events;

/// <summary>Raised when a <see cref="Aggregates.CommunityConcern"/> aggregate is resolved.</summary>
public sealed record CommunityConcernResolvedEvent(
    CommunityConcernEventId EventId,
    CommunityConcernId      CommunityConcernId,
    string                  ResolutionNotes,
    UtcTimestamp             LoggedAtUtc,
    UtcTimestamp             ResolvedAtUtc,
    PerformedByActor         Actor,
    CorrelationId            CorrelationId,
    CausationId              CausationId
) : ICommunityConcernDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "CommunityConcernResolved";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ResolvedAtUtc;

    /// <summary>Wall-clock resolution duration, for the concern-resolution-time KPI.</summary>
    public TimeSpan ResolutionDuration => ResolvedAtUtc.Value - LoggedAtUtc.Value;
}
