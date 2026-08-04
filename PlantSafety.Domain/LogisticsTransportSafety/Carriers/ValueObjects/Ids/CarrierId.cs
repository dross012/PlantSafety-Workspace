// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.Carriers
//  Layer:          Domain
//  File:           CarrierId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the Carrier aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.Carriers.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the Carrier aggregate root.</summary>
public readonly record struct CarrierId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a CarrierId wrapping a non-empty GUID.</summary>
    public CarrierId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("CarrierId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new CarrierId using Uuid7.</summary>
    public static CarrierId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a CarrierId.</summary>
    public static CarrierId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a CarrierId.</summary>
    public static CarrierId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
