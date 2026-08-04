// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin.EvacuationModeling.EvacuationZones
//  Layer:          Domain
//  File:           EvacuationZoneId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the EvacuationZone aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationZones.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the EvacuationZone aggregate root.</summary>
public readonly record struct EvacuationZoneId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates an EvacuationZoneId wrapping a non-empty GUID.</summary>
    public EvacuationZoneId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("EvacuationZoneId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new EvacuationZoneId using Uuid7.</summary>
    public static EvacuationZoneId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as an EvacuationZoneId.</summary>
    public static EvacuationZoneId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into an EvacuationZoneId.</summary>
    public static EvacuationZoneId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
