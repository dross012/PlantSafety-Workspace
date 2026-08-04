// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth.WaterDischargeCompliance.DischargeLimit
//  Layer:          Domain
//  File:           DischargeLimitEventId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for DischargeLimit domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargeLimit.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for DischargeLimit domain events.</summary>
public readonly record struct DischargeLimitEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a DischargeLimitEventId wrapping a non-empty GUID.</summary>
    public DischargeLimitEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("DischargeLimitEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new DischargeLimitEventId using Uuid7.</summary>
    public static DischargeLimitEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}