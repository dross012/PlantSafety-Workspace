// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CommunitySafety.CommunityContact
//  Layer:          Domain
//  File:           CommunityContactId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the CommunityContact aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.CommunitySafety.CommunityContact.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the CommunityContact aggregate root.</summary>
public readonly record struct CommunityContactId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a CommunityContactId wrapping a non-empty GUID.</summary>
    public CommunityContactId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("CommunityContactId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new CommunityContactId using Uuid7.</summary>
    public static CommunityContactId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a CommunityContactId.</summary>
    public static CommunityContactId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a CommunityContactId.</summary>
    public static CommunityContactId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
