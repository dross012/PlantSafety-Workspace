// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth.WaterDischargeCompliance.DischargePermit
//  Layer:          Domain
//  File:           DischargePermitEventId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for DischargePermit domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargePermit.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for DischargePermit domain events.</summary>
public readonly record struct DischargePermitEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a DischargePermitEventId wrapping a non-empty GUID.</summary>
    public DischargePermitEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("DischargePermitEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new DischargePermitEventId using Uuid7.</summary>
    public static DischargePermitEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}