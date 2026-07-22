// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Read-model item DTO for a single FacilityTrafficMonitoring record. Facility & traffic control monitoring: forklift proximity, pedestrian tracking, congestion, blocked emergency equipment.
// SchemaVersion: 1

namespace PlantSafety.Application.Observability.FacilityTrafficMonitoring.Models;

/// <summary>A single FacilityTrafficMonitoring read-model record.</summary>
public sealed record FacilityTrafficMonitoringItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid FacilityTrafficMonitoringId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Current status (e.g., Created/Active/Completed/Closed/Archived).</summary>
    public required string Status { get; init; }

    /// <summary>ZoneCode.</summary>
    public required  string ZoneCode { get; init; }

    /// <summary>EventType.</summary>
    public required  string EventType { get; init; }

    /// <summary>SeverityScore.</summary>
    public required  decimal SeverityScore { get; init; }

    /// <summary>UTC timestamp when the record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }
}