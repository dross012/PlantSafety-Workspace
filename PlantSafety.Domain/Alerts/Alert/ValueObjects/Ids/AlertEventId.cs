// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Alerts.Alert
//  Layer:          Domain
//  File:           AlertEventId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for Alert domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Alerts.Alert.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for Alert domain events.</summary>
public readonly record struct AlertEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates an AlertEventId wrapping a non-empty GUID.</summary>
    public AlertEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("AlertEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new AlertEventId using Uuid7.</summary>
    public static AlertEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
