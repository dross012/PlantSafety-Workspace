// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CommunitySafety.CommunityContact
//  Layer:          Domain
//  File:           CommunityContactEventId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for CommunityContact domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.CommunitySafety.CommunityContact.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for a CommunityContact domain event.</summary>
public readonly record struct CommunityContactEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a CommunityContactEventId wrapping a non-empty GUID.</summary>
    public CommunityContactEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("CommunityContactEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new CommunityContactEventId using Uuid7.</summary>
    public static CommunityContactEventId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a CommunityContactEventId.</summary>
    public static CommunityContactEventId FromGuid(Guid value) => new(value);

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
