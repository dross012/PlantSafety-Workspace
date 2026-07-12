// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single safety culture trend model output.
// SchemaVersion: 1

namespace PlantSafety.Application.Analytics.Culture.TrendModeling.Models;

/// <summary>A single predicted culture trend data point produced by the trend model.</summary>
public sealed record TrendModelingItemDto
{
    /// <summary>Trend model output identifier.</summary>
    public required Guid TrendModelId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Name of the culture metric being modeled.</summary>
    public required string MetricName { get; init; }

    /// <summary>Predicted value of the metric for the forecast period.</summary>
    public required decimal PredictedValue { get; init; }

    /// <summary>Model confidence for the prediction, expressed as a 0-1 value.</summary>
    public required decimal ModelConfidence { get; init; }

    /// <summary>Current status of the model output (e.g., Draft, Validated).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when the prediction was generated.</summary>
    public required DateTime GeneratedAtUtc { get; init; }
}
