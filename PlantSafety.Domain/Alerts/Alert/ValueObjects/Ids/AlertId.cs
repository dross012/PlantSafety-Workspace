// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Alerts.Alert
//  Layer:          Domain
//  File:           AlertId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the Alert aggregate root.
//                  Grounded in PlantSafety_Program_Structure_extract.txt "Safety Culture &
//                  Alerts" / "Comprehensive Manufacturing Safety Alerts" catalog.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Alerts.Alert.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the <see cref="Aggregates.Alert"/> aggregate root.</summary>
public readonly record struct AlertId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates an AlertId wrapping a non-empty GUID.</summary>
    public AlertId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("AlertId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new AlertId using Uuid7.</summary>
    public static AlertId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as an AlertId.</summary>
    public static AlertId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into an AlertId.</summary>
    public static AlertId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
