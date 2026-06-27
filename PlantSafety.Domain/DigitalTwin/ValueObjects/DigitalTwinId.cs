// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin
//  Layer:          Domain
//  File:           DigitalTwinId.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Strong-typed Uuid7-backed identity for the DigitalTwin aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.DigitalTwin.ValueObjects;

/// <summary>Strong-typed, Uuid7-backed identity for the DigitalTwin aggregate root.</summary>
public readonly record struct DigitalTwinId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a DigitalTwinId wrapping a non-empty GUID.</summary>
    public DigitalTwinId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("DigitalTwinId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new DigitalTwinId using Uuid7.</summary>
    public static DigitalTwinId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a DigitalTwinId.</summary>
    public static DigitalTwinId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a DigitalTwinId.</summary>
    public static DigitalTwinId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
