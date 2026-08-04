// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.LoadSecurementChecks
//  Layer:          Domain
//  File:           LoadSecurementCheckId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the LoadSecurementCheck aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.LoadSecurementChecks.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the LoadSecurementCheck aggregate root.</summary>
public readonly record struct LoadSecurementCheckId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a LoadSecurementCheckId wrapping a non-empty GUID.</summary>
    public LoadSecurementCheckId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("LoadSecurementCheckId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new LoadSecurementCheckId using Uuid7.</summary>
    public static LoadSecurementCheckId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a LoadSecurementCheckId.</summary>
    public static LoadSecurementCheckId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a LoadSecurementCheckId.</summary>
    public static LoadSecurementCheckId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
