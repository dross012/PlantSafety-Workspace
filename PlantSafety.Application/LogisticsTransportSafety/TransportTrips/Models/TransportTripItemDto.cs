// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Read-model item DTO for a single TransportTrip record.
// SchemaVersion: 1

namespace PlantSafety.Application.LogisticsTransportSafety.TransportTrips.Models;

/// <summary>A single TransportTrip record.</summary>
public sealed record TransportTripItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid TransportTripId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Identity of the vehicle assigned to this trip.</summary>
    public required Guid VehicleId { get; init; }

    /// <summary>Identity of the driver assigned to this trip.</summary>
    public required Guid DriverUserId { get; init; }

    /// <summary>Classification: OnSiteMaterialMove, OffSiteDelivery, PersonnelTransport, HazmatTransport.</summary>
    public required string TripType { get; init; }

    /// <summary>Overall status classification.</summary>
    public required string TripStatus { get; init; }

    /// <summary>Route description.</summary>
    public required string RouteDescription { get; init; }

    /// <summary>Planned UTC departure time.</summary>
    public required DateTime PlannedDepartureAtUtc { get; init; }

    /// <summary>UTC timestamp when this record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public required Guid CreatedBy { get; init; }
}
