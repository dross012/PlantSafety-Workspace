// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.TransportTrips
//  Layer:          Domain
//  File:           TransportTripArchivalPolicy.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain policy governing archival eligibility for TransportTrip aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.TransportTrips.Policies;

/// <summary>Determines whether a <see cref="TransportTrip"/> aggregate is eligible for archival.</summary>
public static class TransportTripArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the trip record may be archived (completed or cancelled only).</summary>
    public static bool IsEligibleForArchival(PlantSafety.Domain.LogisticsTransportSafety.TransportTrips.Aggregates.TransportTrip aggregate)
        => aggregate.Status is TransportTripStatus.Completed or TransportTripStatus.Cancelled;
}
