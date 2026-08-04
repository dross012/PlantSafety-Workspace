// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.AccumulationAreas
//  Layer:          Domain
//  File:           AccumulationAreaEventId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Strong-typed, Uuid7-backed identity for AccumulationArea domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.HazardousWasteManagement.AccumulationAreas.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for AccumulationArea domain events.</summary>
public readonly record struct AccumulationAreaEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates an AccumulationAreaEventId wrapping a non-empty GUID.</summary>
    public AccumulationAreaEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("AccumulationAreaEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new AccumulationAreaEventId using Uuid7.</summary>
    public static AccumulationAreaEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
