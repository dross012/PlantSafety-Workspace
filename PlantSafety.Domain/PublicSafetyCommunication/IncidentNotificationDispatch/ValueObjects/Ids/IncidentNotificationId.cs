// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PublicSafetyCommunication.IncidentNotificationDispatch
//  Layer:          Domain
//  File:           IncidentNotificationId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the IncidentNotification aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PublicSafetyCommunication.IncidentNotificationDispatch.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the IncidentNotification aggregate root.</summary>
public readonly record struct IncidentNotificationId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates an IncidentNotificationId wrapping a non-empty GUID.</summary>
    public IncidentNotificationId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("IncidentNotificationId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new IncidentNotificationId using Uuid7.</summary>
    public static IncidentNotificationId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as an IncidentNotificationId.</summary>
    public static IncidentNotificationId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into an IncidentNotificationId.</summary>
    public static IncidentNotificationId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
