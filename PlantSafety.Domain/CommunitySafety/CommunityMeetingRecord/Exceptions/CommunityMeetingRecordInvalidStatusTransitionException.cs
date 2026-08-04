// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CommunitySafety.CommunityMeetingRecord
//  Layer:          Domain
//  File:           CommunityMeetingRecordInvalidStatusTransitionException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a CommunityMeetingRecord lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.CommunitySafety.CommunityMeetingRecord.ValueObjects.Ids;

namespace PlantSafety.Domain.CommunitySafety.CommunityMeetingRecord.Exceptions;

/// <summary>Thrown when a <see cref="Aggregates.CommunityMeetingRecord"/> lifecycle transition is not permitted.</summary>
public sealed class CommunityMeetingRecordInvalidStatusTransitionException : CommunityMeetingRecordDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public CommunityMeetingRecordInvalidStatusTransitionException(
        CommunityMeetingRecordId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot transition CommunityMeetingRecord '{id}' from '{from}' to '{to}'.",
            "CommunityMeetingRecord.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
