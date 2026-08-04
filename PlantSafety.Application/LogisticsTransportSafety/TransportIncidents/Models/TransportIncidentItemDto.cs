// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Read-model item DTO for a single TransportIncident record.
// SchemaVersion: 1

namespace PlantSafety.Application.LogisticsTransportSafety.TransportIncidents.Models;

/// <summary>A single TransportIncident record.</summary>
public sealed record TransportIncidentItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid TransportIncidentId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Identity of the vehicle involved in this incident.</summary>
    public required Guid VehicleId { get; init; }

    /// <summary>Identity of the driver involved in this incident.</summary>
    public required Guid DriverUserId { get; init; }

    /// <summary>Classification: Collision, CargoSpill, NearMiss, RolloverOrTipOver, PedestrianInteraction.</summary>
    public required string IncidentType { get; init; }

    /// <summary>Severity classification: Minor, Moderate, Major, Critical.</summary>
    public required string Severity { get; init; }

    /// <summary>Overall status classification.</summary>
    public required string IncidentStatus { get; init; }

    /// <summary>Location description where the incident occurred.</summary>
    public required string Location { get; init; }

    /// <summary>UTC timestamp the incident occurred.</summary>
    public required DateTime OccurredAtUtc { get; init; }

    /// <summary>UTC timestamp when this record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public required Guid CreatedBy { get; init; }
}
