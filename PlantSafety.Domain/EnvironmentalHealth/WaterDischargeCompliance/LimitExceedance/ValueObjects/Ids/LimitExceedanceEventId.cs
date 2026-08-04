// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance
//  Layer:          Domain
//  File:           LimitExceedanceEventId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for LimitExceedance domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for LimitExceedance domain events.</summary>
public readonly record struct LimitExceedanceEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a LimitExceedanceEventId wrapping a non-empty GUID.</summary>
    public LimitExceedanceEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("LimitExceedanceEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new LimitExceedanceEventId using Uuid7.</summary>
    public static LimitExceedanceEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}