// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.Carriers
//  Layer:          Domain
//  File:           CarrierEventId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for Carrier domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.Carriers.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for Carrier domain events.</summary>
public readonly record struct CarrierEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a CarrierEventId wrapping a non-empty GUID.</summary>
    public CarrierEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("CarrierEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new CarrierEventId using Uuid7.</summary>
    public static CarrierEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
