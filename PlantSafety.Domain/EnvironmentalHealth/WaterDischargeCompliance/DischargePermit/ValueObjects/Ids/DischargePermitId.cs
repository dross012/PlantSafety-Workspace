// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth.WaterDischargeCompliance.DischargePermit
//  Layer:          Domain
//  File:           DischargePermitId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the DischargePermit aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargePermit.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the DischargePermit aggregate root.</summary>
public readonly record struct DischargePermitId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a DischargePermitId wrapping a non-empty GUID.</summary>
    public DischargePermitId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("DischargePermitId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new DischargePermitId using Uuid7.</summary>
    public static DischargePermitId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a DischargePermitId.</summary>
    public static DischargePermitId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a DischargePermitId.</summary>
    public static DischargePermitId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}