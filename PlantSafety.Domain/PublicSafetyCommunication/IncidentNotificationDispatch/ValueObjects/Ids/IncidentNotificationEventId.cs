// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PublicSafetyCommunication.IncidentNotificationDispatch
//  Layer:          Domain
//  File:           IncidentNotificationEventId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for IncidentNotification domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PublicSafetyCommunication.IncidentNotificationDispatch.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for IncidentNotification domain events.</summary>
public readonly record struct IncidentNotificationEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates an IncidentNotificationEventId wrapping a non-empty GUID.</summary>
    public IncidentNotificationEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("IncidentNotificationEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new IncidentNotificationEventId using Uuid7.</summary>
    public static IncidentNotificationEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
