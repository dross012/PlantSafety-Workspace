// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.TransportVehicles
//  Layer:          Domain
//  File:           TransportVehicleId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the TransportVehicle aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.TransportVehicles.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the TransportVehicle aggregate root.</summary>
public readonly record struct TransportVehicleId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a TransportVehicleId wrapping a non-empty GUID.</summary>
    public TransportVehicleId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("TransportVehicleId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new TransportVehicleId using Uuid7.</summary>
    public static TransportVehicleId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a TransportVehicleId.</summary>
    public static TransportVehicleId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a TransportVehicleId.</summary>
    public static TransportVehicleId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
