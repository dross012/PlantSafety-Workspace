// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-15
// FILE DESCRIPTION: Read-side DTO for a sustainability metric record.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.Sustainability.Models;

/// <summary>Read-side projection of a sustainability metric record (emissions/energy/water/waste).</summary>
public sealed record SustainabilityMetricDto
{
    /// <summary>Unique identifier of the metric record (UUIDv7).</summary>
    public required Guid MetricId { get; init; }

    /// <summary>Tenant owning this metric.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site this metric is scoped to. Null means tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Display name of the metric.</summary>
    public required string MetricName { get; init; }

    /// <summary>Metric category (e.g., Emissions, Energy, Water, Waste).</summary>
    public required string Category { get; init; }

    /// <summary>Measured value for the reporting period.</summary>
    public required double Value { get; init; }

    /// <summary>Unit of measure (e.g., tCO2e, kWh, m3, kg).</summary>
    public required string Unit { get; init; }

    /// <summary>Target value for the reporting period, or null when no target is set.</summary>
    public double? Target { get; init; }

    /// <summary>UTC timestamp marking the end of the reporting period.</summary>
    public required DateTime PeriodEndUtc { get; init; }

    /// <summary>Current status against target (e.g., OnTrack, AtRisk, Exceeded).</summary>
    public required string Status { get; init; }
}
