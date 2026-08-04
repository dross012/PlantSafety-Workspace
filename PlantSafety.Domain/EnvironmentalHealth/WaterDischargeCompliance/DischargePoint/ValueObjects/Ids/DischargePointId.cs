// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth.WaterDischargeCompliance.DischargePoint
//  Layer:          Domain
//  File:           DischargePointId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the DischargePoint aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargePoint.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the DischargePoint aggregate root.</summary>
public readonly record struct DischargePointId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a DischargePointId wrapping a non-empty GUID.</summary>
    public DischargePointId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("DischargePointId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new DischargePointId using Uuid7.</summary>
    public static DischargePointId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a DischargePointId.</summary>
    public static DischargePointId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a DischargePointId.</summary>
    public static DischargePointId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}