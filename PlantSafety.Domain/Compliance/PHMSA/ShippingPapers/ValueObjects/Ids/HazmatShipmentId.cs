// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.ShippingPapers
//  Layer:          Domain
//  File:           HazmatShipmentId.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the HazmatShipment aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.PHMSA.ShippingPapers.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the HazmatShipment aggregate root.</summary>
public readonly record struct HazmatShipmentId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a HazmatShipmentId wrapping a non-empty GUID.</summary>
    public HazmatShipmentId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("HazmatShipmentId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new HazmatShipmentId using Uuid7.</summary>
    public static HazmatShipmentId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a HazmatShipmentId.</summary>
    public static HazmatShipmentId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a HazmatShipmentId.</summary>
    public static HazmatShipmentId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
