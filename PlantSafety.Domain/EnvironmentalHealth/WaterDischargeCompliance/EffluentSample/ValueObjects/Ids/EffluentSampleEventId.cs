// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth.WaterDischargeCompliance.EffluentSample
//  Layer:          Domain
//  File:           EffluentSampleEventId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for EffluentSample domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.EffluentSample.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for EffluentSample domain events.</summary>
public readonly record struct EffluentSampleEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a EffluentSampleEventId wrapping a non-empty GUID.</summary>
    public EffluentSampleEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("EffluentSampleEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new EffluentSampleEventId using Uuid7.</summary>
    public static EffluentSampleEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}