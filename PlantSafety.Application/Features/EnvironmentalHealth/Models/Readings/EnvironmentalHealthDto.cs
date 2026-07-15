// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-15
// FILE DESCRIPTION: Read-side DTO for an Environmental Health monitoring reading.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.EnvironmentalHealth.Models.Readings;

/// <summary>
/// Read-side projection of an EnvironmentalHealth aggregate. Represents a single environmental
/// monitoring reading (air quality, noise, water quality, emissions, waste handling, etc.)
/// captured at a site and evaluated against a threshold.
/// </summary>
public sealed record EnvironmentalHealthDto
{
    /// <summary>Unique identifier (UUIDv7).</summary>
    public required Guid EnvironmentalHealthId { get; init; }

    /// <summary>Tenant owning this reading.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site this reading was captured at. Null means enterprise-level.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Environmental metric type (e.g., AirQuality, NoiseLevel, WaterQuality, Emissions, WasteHandling).</summary>
    public required string MetricType { get; init; }

    /// <summary>Measured value for the metric.</summary>
    public required decimal MeasurementValue { get; init; }

    /// <summary>Unit of measure for <see cref="MeasurementValue"/>.</summary>
    public required string Unit { get; init; }

    /// <summary>Regulatory/internal threshold this measurement is evaluated against.</summary>
    public required decimal Threshold { get; init; }

    /// <summary>Evaluated status (Normal/Warning/Critical).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp the reading was recorded.</summary>
    public required DateTimeOffset RecordedAtUtc { get; init; }
}
