// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Classification
//  Layer:          Domain
//  File:           HazmatMaterialId.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the HazmatMaterial aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.PHMSA.Classification.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the HazmatMaterial aggregate root.</summary>
public readonly record struct HazmatMaterialId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a HazmatMaterialId wrapping a non-empty GUID.</summary>
    public HazmatMaterialId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("HazmatMaterialId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new HazmatMaterialId using Uuid7.</summary>
    public static HazmatMaterialId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a HazmatMaterialId.</summary>
    public static HazmatMaterialId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a HazmatMaterialId.</summary>
    public static HazmatMaterialId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
