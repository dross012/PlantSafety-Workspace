// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CommunitySafety.CommunityContact
//  Layer:          Domain
//  File:           CommunityContactRegisteredEvent.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Raised when a new CommunityContact aggregate is registered. Additive-only
//                  evolution.
// ============================================================================
using PlantSafety.Domain.CommunitySafety.CommunityContact.Enums;
using PlantSafety.Domain.CommunitySafety.CommunityContact.ValueObjects.Ids;

namespace PlantSafety.Domain.CommunitySafety.CommunityContact.Events;

/// <summary>Raised when a new <see cref="Aggregates.CommunityContact"/> aggregate is registered.</summary>
public sealed record CommunityContactRegisteredEvent(
    CommunityContactEventId EventId,
    CommunityContactId      CommunityContactId,
    CommunityContactType    ContactType,
    UtcTimestamp             RegisteredAtUtc,
    PerformedByActor         Actor,
    CorrelationId            CorrelationId,
    CausationId              CausationId
) : ICommunityContactDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "CommunityContactRegistered";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => RegisteredAtUtc;
}
