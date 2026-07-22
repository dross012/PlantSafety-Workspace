// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Read-model item DTO for a single PPEBehavioralMonitoring record. PPE & behavioral compliance monitoring via computer vision: PPE detection, restricted-area verification, unsafe behavior identification.
// SchemaVersion: 1

namespace PlantSafety.Application.Observability.PPEBehavioralMonitoring.Models;

/// <summary>A single PPEBehavioralMonitoring read-model record.</summary>
public sealed record PPEBehavioralMonitoringItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid PPEBehavioralMonitoringId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Current status (e.g., Created/Active/Completed/Closed/Archived).</summary>
    public required string Status { get; init; }

    /// <summary>SensorId.</summary>
    public required  string SensorId { get; init; }

    /// <summary>ZoneCode.</summary>
    public required  string ZoneCode { get; init; }

    /// <summary>DetectedEventType.</summary>
    public required  string DetectedEventType { get; init; }

    /// <summary>ConfidenceScore.</summary>
    public required  decimal ConfidenceScore { get; init; }

    /// <summary>UTC timestamp when the record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }
}