// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.TransportTrips
//  Layer:          Domain
//  File:           TransportTripId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the TransportTrip aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.TransportTrips.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the TransportTrip aggregate root.</summary>
public readonly record struct TransportTripId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a TransportTripId wrapping a non-empty GUID.</summary>
    public TransportTripId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("TransportTripId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new TransportTripId using Uuid7.</summary>
    public static TransportTripId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a TransportTripId.</summary>
    public static TransportTripId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a TransportTripId.</summary>
    public static TransportTripId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
