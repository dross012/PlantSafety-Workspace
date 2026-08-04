// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PublicSafetyCommunication.PublicAlertBroadcast
//  Layer:          Domain
//  File:           PublicAlertBroadcastEventId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for PublicAlertBroadcast domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PublicSafetyCommunication.PublicAlertBroadcast.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for PublicAlertBroadcast domain events.</summary>
public readonly record struct PublicAlertBroadcastEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a PublicAlertBroadcastEventId wrapping a non-empty GUID.</summary>
    public PublicAlertBroadcastEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("PublicAlertBroadcastEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new PublicAlertBroadcastEventId using Uuid7.</summary>
    public static PublicAlertBroadcastEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
