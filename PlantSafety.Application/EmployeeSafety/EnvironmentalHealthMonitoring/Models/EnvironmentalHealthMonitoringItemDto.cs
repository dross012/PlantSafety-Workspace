// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Read-model item DTO for a single EnvironmentalHealthMonitoring record.
// SchemaVersion: 1

namespace PlantSafety.Application.EmployeeSafety.EnvironmentalHealthMonitoring.Models;

/// <summary>A single EnvironmentalHealthMonitoring record (Environmental and health monitoring protects employees from airborne, noise, and vibration hazards through ventilation, dust collection, noise assessments, and respiratory protection programs.).</summary>
public sealed record EnvironmentalHealthMonitoringItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid EnvironmentalHealthMonitoringId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Monitoring Title.</summary>
    public required string MonitoringTitle { get; init; }

    /// <summary>Classification: AirQuality, Noise, Vibration, RespiratoryExposure.</summary>
    public required string MonitoringCategory { get; init; }

    /// <summary>Monitoring Location.</summary>
    public required string MonitoringLocation { get; init; }

    /// <summary>Overall status classification.</summary>
    public required string MonitoringStatus { get; init; }

    /// <summary>UTC timestamp when this record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public required Guid CreatedBy { get; init; }
}