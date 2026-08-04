// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.TransportTrips
//  Layer:          Domain
//  File:           TransportTripEventId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for TransportTrip domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.TransportTrips.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for TransportTrip domain events.</summary>
public readonly record struct TransportTripEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a TransportTripEventId wrapping a non-empty GUID.</summary>
    public TransportTripEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("TransportTripEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new TransportTripEventId using Uuid7.</summary>
    public static TransportTripEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
