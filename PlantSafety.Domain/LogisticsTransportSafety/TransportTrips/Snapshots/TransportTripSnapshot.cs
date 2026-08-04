// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.TransportTrips
//  Layer:          Domain
//  File:           TransportTripSnapshot.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for TransportTrip aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.TransportTrips.Snapshots;

/// <summary>Immutable snapshot of a <see cref="TransportTrip"/> aggregate for fast rehydration.</summary>
public sealed record TransportTripSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public TransportTripId TransportTripId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public TransportTripStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}
