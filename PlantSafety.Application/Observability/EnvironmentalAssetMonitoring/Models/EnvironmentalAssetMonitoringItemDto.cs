// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Read-model item DTO for a single EnvironmentalAssetMonitoring record. Environmental & asset monitoring: air quality, temperature/humidity, noise, vibration, spill and gas-leak detection.
// SchemaVersion: 1

namespace PlantSafety.Application.Observability.EnvironmentalAssetMonitoring.Models;

/// <summary>A single EnvironmentalAssetMonitoring read-model record.</summary>
public sealed record EnvironmentalAssetMonitoringItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid EnvironmentalAssetMonitoringId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Current status (e.g., Created/Active/Completed/Closed/Archived).</summary>
    public required string Status { get; init; }

    /// <summary>SensorId.</summary>
    public required  string SensorId { get; init; }

    /// <summary>MeasurementType.</summary>
    public required  string MeasurementType { get; init; }

    /// <summary>Value.</summary>
    public required  decimal Value { get; init; }

    /// <summary>ThresholdExceeded.</summary>
    public required  bool ThresholdExceeded { get; init; }

    /// <summary>UTC timestamp when the record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }
}