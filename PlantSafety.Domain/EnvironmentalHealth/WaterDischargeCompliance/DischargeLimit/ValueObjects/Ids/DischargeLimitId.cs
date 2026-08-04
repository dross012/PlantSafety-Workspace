// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth.WaterDischargeCompliance.DischargeLimit
//  Layer:          Domain
//  File:           DischargeLimitId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the DischargeLimit aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargeLimit.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the DischargeLimit aggregate root.</summary>
public readonly record struct DischargeLimitId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a DischargeLimitId wrapping a non-empty GUID.</summary>
    public DischargeLimitId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("DischargeLimitId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new DischargeLimitId using Uuid7.</summary>
    public static DischargeLimitId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a DischargeLimitId.</summary>
    public static DischargeLimitId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a DischargeLimitId.</summary>
    public static DischargeLimitId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}