// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.TsdfFacilities
//  Layer:          Domain
//  File:           TsdfFacilityEventId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Strong-typed, Uuid7-backed identity for TsdfFacility domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.HazardousWasteManagement.TsdfFacilities.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for TsdfFacility domain events.</summary>
public readonly record struct TsdfFacilityEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a TsdfFacilityEventId wrapping a non-empty GUID.</summary>
    public TsdfFacilityEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("TsdfFacilityEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new TsdfFacilityEventId using Uuid7.</summary>
    public static TsdfFacilityEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
