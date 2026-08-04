// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability.AdvancedMetrics
//  Layer:          Domain
//  File:           SustainabilityTargetId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for a SustainabilityTarget child entity.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Sustainability.AdvancedMetrics.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for a SustainabilityTarget child entity.</summary>
public readonly record struct SustainabilityTargetId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a SustainabilityTargetId wrapping a non-empty GUID.</summary>
    public SustainabilityTargetId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("SustainabilityTargetId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new SustainabilityTargetId using Uuid7.</summary>
    public static SustainabilityTargetId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
