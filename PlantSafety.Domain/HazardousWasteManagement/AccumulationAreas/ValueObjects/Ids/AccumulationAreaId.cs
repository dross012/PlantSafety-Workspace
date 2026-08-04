// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.AccumulationAreas
//  Layer:          Domain
//  File:           AccumulationAreaId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Strong-typed, Uuid7-backed identity for the AccumulationArea aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.HazardousWasteManagement.AccumulationAreas.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the AccumulationArea aggregate root.</summary>
public readonly record struct AccumulationAreaId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates an AccumulationAreaId wrapping a non-empty GUID.</summary>
    public AccumulationAreaId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("AccumulationAreaId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new AccumulationAreaId using Uuid7.</summary>
    public static AccumulationAreaId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as an AccumulationAreaId.</summary>
    public static AccumulationAreaId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into an AccumulationAreaId.</summary>
    public static AccumulationAreaId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
