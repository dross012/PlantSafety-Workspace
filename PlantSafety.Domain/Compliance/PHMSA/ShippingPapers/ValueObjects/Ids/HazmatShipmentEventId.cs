// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.ShippingPapers
//  Layer:          Domain
//  File:           HazmatShipmentEventId.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for HazmatShipment domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.PHMSA.ShippingPapers.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for HazmatShipment domain events.</summary>
public readonly record struct HazmatShipmentEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a HazmatShipmentEventId wrapping a non-empty GUID.</summary>
    public HazmatShipmentEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("HazmatShipmentEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new HazmatShipmentEventId using Uuid7.</summary>
    public static HazmatShipmentEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
