// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CommunitySafety.CommunityNotification
//  Layer:          Domain
//  File:           CommunityNotificationEventId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for CommunityNotification domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.CommunitySafety.CommunityNotification.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for a CommunityNotification domain event.</summary>
public readonly record struct CommunityNotificationEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a CommunityNotificationEventId wrapping a non-empty GUID.</summary>
    public CommunityNotificationEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("CommunityNotificationEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new CommunityNotificationEventId using Uuid7.</summary>
    public static CommunityNotificationEventId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a CommunityNotificationEventId.</summary>
    public static CommunityNotificationEventId FromGuid(Guid value) => new(value);

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
