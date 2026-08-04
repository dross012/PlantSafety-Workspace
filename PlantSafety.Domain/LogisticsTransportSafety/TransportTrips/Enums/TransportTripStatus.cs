// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.TransportTrips
//  Layer:          Domain
//  File:           TransportTripStatus.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Lifecycle states for the TransportTrip aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.TransportTrips.Enums;

/// <summary>Lifecycle states for the <see cref="TransportTrip"/> aggregate root.</summary>
public enum TransportTripStatus
{
    /// <summary>Trip planned and awaiting departure.</summary>
    Planned = 0,

    /// <summary>Trip currently underway.</summary>
    InProgress = 1,

    /// <summary>Trip completed successfully.</summary>
    Completed = 2,

    /// <summary>Trip cancelled before completion.</summary>
    Cancelled = 3,

    /// <summary>An incident was reported during this trip.</summary>
    IncidentReported = 4,
}
