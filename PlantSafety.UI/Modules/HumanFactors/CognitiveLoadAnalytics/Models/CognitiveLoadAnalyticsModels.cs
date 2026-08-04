// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-08-04
// FILE DESCRIPTION: Client-side DTOs for the HumanFactors.CognitiveLoadAnalytics sub-module.
//   Mirrors the wire shape of PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Models.*,
//   returned by PlantSafety.Api.HumanFactors.CognitiveLoadAnalytics.Controllers.V1.
//   CognitiveLoadAnalyticsV1Controller.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.4
// ============================================================

using System;

namespace PlantSafety.UI.Modules.HumanFactors.CognitiveLoadAnalytics.Models;

/// <summary>A single worker's cognitive-load rollup for a period.</summary>
public sealed class CognitiveLoadByWorkerDto
{
    /// <summary>Gets the worker identifier.</summary>
    public Guid WorkerUserId { get; init; }

    /// <summary>Gets the period start.</summary>
    public DateTime PeriodStartUtc { get; init; }

    /// <summary>Gets the period end.</summary>
    public DateTime PeriodEndUtc { get; init; }

    /// <summary>Gets the mean cognitive-load score across the period.</summary>
    public decimal MeanCognitiveLoadScore { get; init; }

    /// <summary>Gets the highest single cognitive-load score recorded in the period.</summary>
    public decimal PeakCognitiveLoadScore { get; init; }

    /// <summary>Gets the count of readings classified as overload events.</summary>
    public int OverloadEventCount { get; init; }

    /// <summary>Gets the total count of readings contributing to this rollup.</summary>
    public int ReadingCount { get; init; }
}

/// <summary>A single task-complexity profile read-model record.</summary>
public sealed class TaskComplexityProfileDto
{
    /// <summary>Gets the profile identifier.</summary>
    public Guid TaskComplexityProfileId { get; init; }

    /// <summary>Gets the owning ledger identifier.</summary>
    public Guid CognitiveLoadLedgerId { get; init; }

    /// <summary>Gets the task business code.</summary>
    public string TaskCode { get; init; } = string.Empty;

    /// <summary>Gets the task name.</summary>
    public string TaskName { get; init; } = string.Empty;

    /// <summary>Gets the complexity rating, 1-10.</summary>
    public int ComplexityScore { get; init; }

    /// <summary>Gets the narrative description.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>Gets the UTC instant this profile was defined.</summary>
    public DateTime DefinedAtUtc { get; init; }
}

/// <summary>A single shift's workload-trend recalculation snapshot.</summary>
public sealed class WorkloadTrendByShiftDto
{
    /// <summary>Gets the trend record identifier.</summary>
    public Guid WorkloadTrendRecordId { get; init; }

    /// <summary>Gets the owning ledger identifier.</summary>
    public Guid CognitiveLoadLedgerId { get; init; }

    /// <summary>Gets the shift classification.</summary>
    public string ShiftType { get; init; } = string.Empty;

    /// <summary>Gets the window start.</summary>
    public DateTime PeriodStartUtc { get; init; }

    /// <summary>Gets the window end.</summary>
    public DateTime PeriodEndUtc { get; init; }

    /// <summary>Gets the mean cognitive-load score across the window.</summary>
    public decimal MeanCognitiveLoadScore { get; init; }

    /// <summary>Gets the count of readings classified as overload events in the window.</summary>
    public int OverloadEventCount { get; init; }

    /// <summary>Gets the total reading count contributing to this snapshot.</summary>
    public int ReadingCount { get; init; }

    /// <summary>Gets the UTC instant this recalculation was performed.</summary>
    public DateTime RecalculatedAtUtc { get; init; }
}

/// <summary>A single cognitive-risk prediction read-model record.</summary>
public sealed class CognitiveRiskPredictionDto
{
    /// <summary>Gets the prediction identifier.</summary>
    public Guid CognitiveRiskPredictionId { get; init; }

    /// <summary>Gets the worker identifier this prediction was generated for.</summary>
    public Guid WorkerUserId { get; init; }

    /// <summary>Gets the source ledger identifier.</summary>
    public Guid SourceLedgerId { get; init; }

    /// <summary>Gets the predicted cognitive-risk classification.</summary>
    public string PredictedRiskLevel { get; init; } = string.Empty;

    /// <summary>Gets the normalized (0-1) prediction confidence score.</summary>
    public decimal PredictionScore { get; init; }

    /// <summary>Gets the predictive model version used.</summary>
    public string ModelVersion { get; init; } = string.Empty;

    /// <summary>Gets the current lifecycle status (Generated/Acknowledged/Dismissed).</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the UTC instant the prediction was generated.</summary>
    public DateTime GeneratedAtUtc { get; init; }

    /// <summary>Gets the UTC instant the alert was acknowledged, if it has been.</summary>
    public DateTime? AcknowledgedAtUtc { get; init; }
}
