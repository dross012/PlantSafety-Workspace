// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.TsdfFacilities
//  Layer:          Domain
//  File:           TsdfFacilityId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Strong-typed, Uuid7-backed identity for the TsdfFacility aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.HazardousWasteManagement.TsdfFacilities.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the TsdfFacility aggregate root.</summary>
public readonly record struct TsdfFacilityId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a TsdfFacilityId wrapping a non-empty GUID.</summary>
    public TsdfFacilityId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("TsdfFacilityId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new TsdfFacilityId using Uuid7.</summary>
    public static TsdfFacilityId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a TsdfFacilityId.</summary>
    public static TsdfFacilityId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a TsdfFacilityId.</summary>
    public static TsdfFacilityId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
