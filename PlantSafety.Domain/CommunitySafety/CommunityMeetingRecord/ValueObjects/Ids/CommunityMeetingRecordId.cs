// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CommunitySafety.CommunityMeetingRecord
//  Layer:          Domain
//  File:           CommunityMeetingRecordId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the CommunityMeetingRecord aggregate
//                  root. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.CommunitySafety.CommunityMeetingRecord.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the CommunityMeetingRecord aggregate root.</summary>
public readonly record struct CommunityMeetingRecordId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a CommunityMeetingRecordId wrapping a non-empty GUID.</summary>
    public CommunityMeetingRecordId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("CommunityMeetingRecordId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new CommunityMeetingRecordId using Uuid7.</summary>
    public static CommunityMeetingRecordId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a CommunityMeetingRecordId.</summary>
    public static CommunityMeetingRecordId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a CommunityMeetingRecordId.</summary>
    public static CommunityMeetingRecordId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
