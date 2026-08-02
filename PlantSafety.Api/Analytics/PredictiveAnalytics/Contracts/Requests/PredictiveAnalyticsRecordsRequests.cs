// ============================================================================
// File: PredictiveAnalyticsRecordsRequests.cs
// Project: PlantSafety.Api
// Module: Analytics.PredictiveAnalytics
// Created: 2026-08-02
// Description: Request DTOs bound from the HTTP request body/query string for the
//              Analytics.PredictiveAnalytics Records V1 controller. Covers the full lifecycle:
//              Create -> Review -> Action -> Close -> Archive. Mirrors
//              PlantSafety.Api.Compliance.Permits.Contracts.Requests.PermitsRequests.cs -- every
//              mutating lifecycle request carries an optional ETag field so clients can supply the
//              optimistic-concurrency token captured from a prior read (e.g.
//              PredictiveAnalyticsItemDto.ETag); a stale value results in a concurrency-conflict
//              error.
// ============================================================================

namespace PlantSafety.Api.Analytics.PredictiveAnalytics.Contracts.Requests;

/// <summary>Request body for submitting a new PredictiveAnalytics record.</summary>
public sealed record CreatePredictiveAnalyticsRequest
{
    /// <summary>Optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Name of the predictive model that generated this output.</summary>
    public required string ModelName { get; init; }

    /// <summary>Prediction classification (e.g., IncidentRisk, EquipmentFailure, ComplianceDrift).</summary>
    public required string PredictionType { get; init; }

    /// <summary>Entity this prediction targets (worker, equipment, site), when applicable.</summary>
    public Guid? TargetEntityId { get; init; }

    /// <summary>Predicted risk score (0-100).</summary>
    public required decimal PredictedRiskScore { get; init; }

    /// <summary>Model confidence level for this prediction (0-100).</summary>
    public decimal ConfidenceLevel { get; init; }

    /// <summary>Prediction horizon, in days, this output forecasts over.</summary>
    public int PredictionHorizonDays { get; init; } = 30;
}

/// <summary>Request body for reviewing a generated prediction (Created -&gt; Active, i.e. "Reviewed").</summary>
public sealed record ReviewPredictiveAnalyticsRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for recording that a reviewed prediction has been acted upon (Active -&gt; Completed, i.e. "Actioned").</summary>
public sealed record ActionPredictiveAnalyticsRequest
{
    /// <summary>Optional free-text notes describing the action taken.</summary>
    public string? ActionNotes { get; init; }

    /// <summary>Optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for formally closing an actioned prediction (Completed -&gt; Closed).</summary>
public sealed record ClosePredictiveAnalyticsRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for archiving a closed prediction (Closed -&gt; Archived).</summary>
public sealed record ArchivePredictiveAnalyticsRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; init; }
}
