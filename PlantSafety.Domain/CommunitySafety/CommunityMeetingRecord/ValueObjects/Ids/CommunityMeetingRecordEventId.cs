// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CommunitySafety.CommunityMeetingRecord
//  Layer:          Domain
//  File:           CommunityMeetingRecordEventId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for CommunityMeetingRecord domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.CommunitySafety.CommunityMeetingRecord.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for a CommunityMeetingRecord domain event.</summary>
public readonly record struct CommunityMeetingRecordEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a CommunityMeetingRecordEventId wrapping a non-empty GUID.</summary>
    public CommunityMeetingRecordEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("CommunityMeetingRecordEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new CommunityMeetingRecordEventId using Uuid7.</summary>
    public static CommunityMeetingRecordEventId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a CommunityMeetingRecordEventId.</summary>
    public static CommunityMeetingRecordEventId FromGuid(Guid value) => new(value);

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
