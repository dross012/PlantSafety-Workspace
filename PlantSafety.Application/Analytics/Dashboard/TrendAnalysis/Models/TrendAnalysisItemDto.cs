// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single dashboard trend data point.
// SchemaVersion: 1

namespace PlantSafety.Application.Analytics.Dashboard.TrendAnalysis.Models;

/// <summary>A single data point within a Dashboard trend series.</summary>
public sealed record TrendAnalysisItemDto
{
    /// <summary>Trend point identifier.</summary>
    public required Guid TrendPointId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Name of the metric being trended.</summary>
    public required string MetricName { get; init; }

    /// <summary>Value of the metric at this point.</summary>
    public required decimal MetricValue { get; init; }

    /// <summary>Trend direction versus the prior period (e.g., Improving, Stable, Declining).</summary>
    public required string TrendDirection { get; init; }

    /// <summary>Current status of the data point (e.g., Provisional, Final).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when this point was calculated.</summary>
    public required DateTime CalculatedAtUtc { get; init; }
}
