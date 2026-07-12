// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single predictive analytics result.
// SchemaVersion: 1

namespace PlantSafety.Application.Analytics.PredictiveAnalytics.Models;

/// <summary>A single predictive analytics model output.</summary>
public sealed record PredictiveAnalyticsItemDto
{
    /// <summary>Prediction identifier.</summary>
    public required Guid PredictionId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Name of the predictive model that produced this result.</summary>
    public required string ModelName { get; init; }

    /// <summary>Predicted risk/likelihood score (0.0-1.0).</summary>
    public required decimal Score { get; init; }

    /// <summary>Current status (e.g., Generated/Reviewed/Actioned).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when the prediction was generated.</summary>
    public required DateTime GeneratedAtUtc { get; init; }

    /// <summary>Identity of the system/user that generated the prediction.</summary>
    public required Guid GeneratedBy { get; init; }
}
