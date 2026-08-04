// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.TransportVehicles
//  Layer:          Domain
//  File:           TransportVehicleEventId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for TransportVehicle domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.TransportVehicles.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for TransportVehicle domain events.</summary>
public readonly record struct TransportVehicleEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a TransportVehicleEventId wrapping a non-empty GUID.</summary>
    public TransportVehicleEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("TransportVehicleEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new TransportVehicleEventId using Uuid7.</summary>
    public static TransportVehicleEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
