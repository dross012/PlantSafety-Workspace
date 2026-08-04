// ============================================================
// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Client-side DTOs for the LogisticsTransportSafety module, mirroring the
//   PlantSafety.Api read-model shapes for Carriers, TransportTrips, and TransportIncidents.
//   Display only.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;

namespace PlantSafety.UI.Modules.LogisticsTransportSafety.Models;

/// <summary>A single fleet or third-party transport carrier record.</summary>
public sealed class CarrierDto
{
    /// <summary>Gets the carrier record identifier.</summary>
    public Guid CarrierId { get; init; }

    /// <summary>Gets the carrier name.</summary>
    public string CarrierName { get; init; } = string.Empty;

    /// <summary>Gets the carrier classification.</summary>
    public string CarrierType { get; init; } = string.Empty;

    /// <summary>Gets the overall status classification.</summary>
    public string CarrierStatus { get; init; } = string.Empty;

    /// <summary>Gets the safety rating score (0-100), if qualified.</summary>
    public decimal? SafetyRatingScore { get; init; }
}

/// <summary>Response payload for the carriers read-model query.</summary>
public sealed class CarrierQueryResponseDto
{
    /// <summary>Gets the matching carrier records. Never null; empty when no results.</summary>
    public IReadOnlyList<CarrierDto> Items { get; init; } = [];
}

/// <summary>A single transport trip record.</summary>
public sealed class TransportTripDto
{
    /// <summary>Gets the trip record identifier.</summary>
    public Guid TransportTripId { get; init; }

    /// <summary>Gets the identity of the vehicle assigned to this trip.</summary>
    public Guid VehicleId { get; init; }

    /// <summary>Gets the identity of the driver assigned to this trip.</summary>
    public Guid DriverUserId { get; init; }

    /// <summary>Gets the trip classification.</summary>
    public string TripType { get; init; } = string.Empty;

    /// <summary>Gets the overall status classification.</summary>
    public string TripStatus { get; init; } = string.Empty;

    /// <summary>Gets the route description.</summary>
    public string RouteDescription { get; init; } = string.Empty;

    /// <summary>Gets the planned UTC departure time.</summary>
    public DateTime PlannedDepartureAtUtc { get; init; }
}

/// <summary>Response payload for the active-trips read-model query.</summary>
public sealed class TransportTripQueryResponseDto
{
    /// <summary>Gets the matching active trip records. Never null; empty when no results.</summary>
    public IReadOnlyList<TransportTripDto> Items { get; init; } = [];
}

/// <summary>A single transport incident record.</summary>
public sealed class TransportIncidentDto
{
    /// <summary>Gets the incident record identifier.</summary>
    public Guid TransportIncidentId { get; init; }

    /// <summary>Gets the identity of the vehicle involved.</summary>
    public Guid VehicleId { get; init; }

    /// <summary>Gets the incident classification.</summary>
    public string IncidentType { get; init; } = string.Empty;

    /// <summary>Gets the severity classification.</summary>
    public string Severity { get; init; } = string.Empty;

    /// <summary>Gets the overall status classification.</summary>
    public string IncidentStatus { get; init; } = string.Empty;

    /// <summary>Gets the location description where the incident occurred.</summary>
    public string Location { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp the incident occurred.</summary>
    public DateTime OccurredAtUtc { get; init; }
}

/// <summary>Response payload for the transport-incident-history read-model query.</summary>
public sealed class TransportIncidentQueryResponseDto
{
    /// <summary>Gets the matching transport incident records. Never null; empty when no results.</summary>
    public IReadOnlyList<TransportIncidentDto> Items { get; init; } = [];
}
