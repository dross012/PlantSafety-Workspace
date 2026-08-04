// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CommunitySafety.CommunityConcern
//  Layer:          Domain
//  File:           CommunityConcernEscalatedEvent.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Raised when a CommunityConcern aggregate is escalated (e.g. detected as part
//                  of a recurring-concern pattern for a facility area). This is the CommunitySafety
//                  Risk Hook: a downstream RiskManagement consumer subscribes to this event type
//                  to elevate local risk visibility for the affected facility area. Additive-only
//                  evolution.
// ============================================================================
using PlantSafety.Domain.CommunitySafety.CommunityConcern.ValueObjects.Ids;

namespace PlantSafety.Domain.CommunitySafety.CommunityConcern.Events;

/// <summary>Raised when a <see cref="Aggregates.CommunityConcern"/> aggregate is escalated.</summary>
public sealed record CommunityConcernEscalatedEvent(
    CommunityConcernEventId EventId,
    CommunityConcernId      CommunityConcernId,
    string                  FacilityArea,
    string                  Reason,
    int                     RecentConcernCountForArea,
    UtcTimestamp             EscalatedAtUtc,
    PerformedByActor         Actor,
    CorrelationId            CorrelationId,
    CausationId              CausationId
) : ICommunityConcernDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "CommunityConcernEscalated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => EscalatedAtUtc;
}
