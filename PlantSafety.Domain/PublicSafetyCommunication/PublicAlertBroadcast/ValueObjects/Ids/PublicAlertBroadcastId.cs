// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PublicSafetyCommunication.PublicAlertBroadcast
//  Layer:          Domain
//  File:           PublicAlertBroadcastId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the PublicAlertBroadcast aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PublicSafetyCommunication.PublicAlertBroadcast.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the PublicAlertBroadcast aggregate root.</summary>
public readonly record struct PublicAlertBroadcastId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a PublicAlertBroadcastId wrapping a non-empty GUID.</summary>
    public PublicAlertBroadcastId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("PublicAlertBroadcastId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new PublicAlertBroadcastId using Uuid7.</summary>
    public static PublicAlertBroadcastId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a PublicAlertBroadcastId.</summary>
    public static PublicAlertBroadcastId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a PublicAlertBroadcastId.</summary>
    public static PublicAlertBroadcastId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
