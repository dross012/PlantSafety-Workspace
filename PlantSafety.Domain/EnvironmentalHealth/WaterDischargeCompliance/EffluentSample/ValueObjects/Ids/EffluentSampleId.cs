// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth.WaterDischargeCompliance.EffluentSample
//  Layer:          Domain
//  File:           EffluentSampleId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the EffluentSample aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.EffluentSample.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the EffluentSample aggregate root.</summary>
public readonly record struct EffluentSampleId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a EffluentSampleId wrapping a non-empty GUID.</summary>
    public EffluentSampleId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("EffluentSampleId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new EffluentSampleId using Uuid7.</summary>
    public static EffluentSampleId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a EffluentSampleId.</summary>
    public static EffluentSampleId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a EffluentSampleId.</summary>
    public static EffluentSampleId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}