// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Read-model item DTO for a single OperatingGeofence record.
// SchemaVersion: 1

namespace PlantSafety.Application.AutonomousVehicleSafety.OperatingGeofence.Models;

/// <summary>A single OperatingGeofence record (exclusion/restricted/operating/speed-limit zone).</summary>
public sealed record OperatingGeofenceItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid OperatingGeofenceId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Geofence name.</summary>
    public required string GeofenceName { get; init; }

    /// <summary>Zone classification.</summary>
    public required string ZoneType { get; init; }

    /// <summary>Cumulative count of violations detected against this geofence.</summary>
    public required int ViolationCount { get; init; }

    /// <summary>Overall lifecycle status classification.</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp of the most recent audit, if any.</summary>
    public DateTime? LastAuditedAtUtc { get; init; }

    /// <summary>UTC timestamp when this record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public required Guid CreatedBy { get; init; }
}
