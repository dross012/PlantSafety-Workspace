// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability.AdvancedMetrics
//  Layer:          Domain
//  File:           EnergyIntensityMetricId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for an EnergyIntensityMetric child entity.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Sustainability.AdvancedMetrics.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for an EnergyIntensityMetric child entity.</summary>
public readonly record struct EnergyIntensityMetricId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates an EnergyIntensityMetricId wrapping a non-empty GUID.</summary>
    public EnergyIntensityMetricId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("EnergyIntensityMetricId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new EnergyIntensityMetricId using Uuid7.</summary>
    public static EnergyIntensityMetricId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
