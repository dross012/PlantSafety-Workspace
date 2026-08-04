// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CommunitySafety.CommunityMeetingRecord
//  Layer:          Domain
//  File:           CommunityMeetingRecordNotFoundException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a CommunityMeetingRecord aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.CommunitySafety.CommunityMeetingRecord.ValueObjects.Ids;

namespace PlantSafety.Domain.CommunitySafety.CommunityMeetingRecord.Exceptions;

/// <summary>Thrown when a <see cref="Aggregates.CommunityMeetingRecord"/> aggregate lookup finds no match.</summary>
public sealed class CommunityMeetingRecordNotFoundException : CommunityMeetingRecordDomainException
{
    /// <summary>Creates a not-found exception for the given CommunityMeetingRecord identity.</summary>
    public CommunityMeetingRecordNotFoundException(CommunityMeetingRecordId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"CommunityMeetingRecord with id '{id}' was not found.",
            "CommunityMeetingRecord.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
