// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single predicted incident risk.
// SchemaVersion: 1

namespace PlantSafety.Application.Analytics.Predictive.IncidentPrediction.Models;

/// <summary>A single predicted incident risk produced by the predictive model.</summary>
public sealed record IncidentPredictionItemDto
{
    /// <summary>Prediction identifier.</summary>
    public required Guid PredictionId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Incident category being predicted (e.g., Slip/Trip/Fall, Equipment, Chemical Exposure).</summary>
    public required string IncidentCategory { get; init; }

    /// <summary>Predicted probability of an incident occurring, expressed as a 0-1 value.</summary>
    public required decimal PredictedProbability { get; init; }

    /// <summary>Risk classification (e.g., Low, Medium, High, Critical).</summary>
    public required string RiskLevel { get; init; }

    /// <summary>Current status of the prediction (e.g., Active, Mitigated, Dismissed).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when the prediction was generated.</summary>
    public required DateTime PredictedAtUtc { get; init; }
}
