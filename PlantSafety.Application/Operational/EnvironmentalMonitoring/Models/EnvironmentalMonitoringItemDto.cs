// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-20
// FILE DESCRIPTION: Read-model item DTO for a single EnvironmentalMonitoring record.
// SchemaVersion: 1

namespace PlantSafety.Application.Operational.EnvironmentalMonitoring.Models;

/// <summary>A single EnvironmentalMonitoring read-model record.</summary>
public sealed record EnvironmentalMonitoringItemDto
{
    /// <summary>EnvironmentalMonitoring record identifier.</summary>
    public required Guid EnvironmentalMonitoringRecordId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Identifier of the environmental sensor that produced this reading.</summary>
    public required Guid SensorId { get; init; }

    /// <summary>Display name of the environmental sensor.</summary>
    public required string SensorName { get; init; }

    /// <summary>Type of environmental reading (e.g. AirQuality, Noise, Temperature, Humidity).</summary>
    public required string ReadingType { get; init; }

    /// <summary>Measured reading value.</summary>
    public required decimal ReadingValue { get; init; }

    /// <summary>Current status of this record.</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when this record was last actioned.</summary>
    public required DateTime RecordedAtUtc { get; init; }

    /// <summary>Identity of the system or user that recorded this reading. Null for automated sensor feeds.</summary>
    public Guid? RecordedBy { get; init; }
}