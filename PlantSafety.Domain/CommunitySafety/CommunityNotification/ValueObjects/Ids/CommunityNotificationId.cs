// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CommunitySafety.CommunityNotification
//  Layer:          Domain
//  File:           CommunityNotificationId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the CommunityNotification aggregate
//                  root. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.CommunitySafety.CommunityNotification.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the CommunityNotification aggregate root.</summary>
public readonly record struct CommunityNotificationId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a CommunityNotificationId wrapping a non-empty GUID.</summary>
    public CommunityNotificationId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("CommunityNotificationId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new CommunityNotificationId using Uuid7.</summary>
    public static CommunityNotificationId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a CommunityNotificationId.</summary>
    public static CommunityNotificationId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a CommunityNotificationId.</summary>
    public static CommunityNotificationId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
