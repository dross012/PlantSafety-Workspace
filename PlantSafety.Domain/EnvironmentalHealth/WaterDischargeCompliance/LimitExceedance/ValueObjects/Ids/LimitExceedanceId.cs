// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance
//  Layer:          Domain
//  File:           LimitExceedanceId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the LimitExceedance aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the LimitExceedance aggregate root.</summary>
public readonly record struct LimitExceedanceId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a LimitExceedanceId wrapping a non-empty GUID.</summary>
    public LimitExceedanceId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("LimitExceedanceId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new LimitExceedanceId using Uuid7.</summary>
    public static LimitExceedanceId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a LimitExceedanceId.</summary>
    public static LimitExceedanceId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a LimitExceedanceId.</summary>
    public static LimitExceedanceId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}