// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.TransportTrips
//  Layer:          Domain
//  File:           TransportTripLifecycleRules.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Defines permitted lifecycle transitions for the TransportTrip aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.TransportTrips.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="TransportTrip"/> aggregate.</summary>
public static class TransportTripLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when a trip may be completed from <paramref name="current"/>.</summary>
    public static bool CanComplete(TransportTripStatus current) =>
        current is TransportTripStatus.Planned or TransportTripStatus.InProgress;

    /// <summary>Returns <see langword="true"/> when a trip may be cancelled from <paramref name="current"/>.</summary>
    public static bool CanCancel(TransportTripStatus current) =>
        current is TransportTripStatus.Planned or TransportTripStatus.InProgress;
}
