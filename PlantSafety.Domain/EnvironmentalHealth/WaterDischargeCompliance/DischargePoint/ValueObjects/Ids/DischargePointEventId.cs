// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth.WaterDischargeCompliance.DischargePoint
//  Layer:          Domain
//  File:           DischargePointEventId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for DischargePoint domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargePoint.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for DischargePoint domain events.</summary>
public readonly record struct DischargePointEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a DischargePointEventId wrapping a non-empty GUID.</summary>
    public DischargePointEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("DischargePointEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new DischargePointEventId using Uuid7.</summary>
    public static DischargePointEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}