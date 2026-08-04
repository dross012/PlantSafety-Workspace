// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin.EvacuationModeling.EvacuationZones
//  Layer:          Domain
//  File:           OccupancyReadingId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for OccupancyReading entries.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationZones.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for an <see cref="Entities.OccupancyReading"/>.</summary>
public readonly record struct OccupancyReadingId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates an OccupancyReadingId wrapping a non-empty GUID.</summary>
    public OccupancyReadingId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("OccupancyReadingId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new OccupancyReadingId using Uuid7.</summary>
    public static OccupancyReadingId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as an OccupancyReadingId.</summary>
    public static OccupancyReadingId FromGuid(Guid value) => new(value);

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
