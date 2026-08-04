// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CommunitySafety.CommunityContact
//  Layer:          Domain
//  File:           CommunityContactArchivedEvent.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Raised when a CommunityContact aggregate is archived (soft-deleted), e.g. for
//                  a public-records redaction/right-to-be-forgotten style request. Additive-only
//                  evolution.
// ============================================================================
using PlantSafety.Domain.CommunitySafety.CommunityContact.ValueObjects.Ids;

namespace PlantSafety.Domain.CommunitySafety.CommunityContact.Events;

/// <summary>Raised when a <see cref="Aggregates.CommunityContact"/> aggregate is archived.</summary>
public sealed record CommunityContactArchivedEvent(
    CommunityContactEventId EventId,
    CommunityContactId      CommunityContactId,
    string                  Reason,
    UtcTimestamp             ArchivedAtUtc,
    PerformedByActor         Actor,
    CorrelationId            CorrelationId,
    CausationId              CausationId
) : ICommunityContactDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "CommunityContactArchived";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ArchivedAtUtc;
}
