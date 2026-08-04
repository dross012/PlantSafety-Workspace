// ============================================================================
// File: CognitiveLoadAnalyticsRequests.cs
// Project: PlantSafety.Api
// Module: HumanFactors.CognitiveLoadAnalytics
// Created: 2026-08-04
// Description: Request DTOs bound from the HTTP request body for the HumanFactors.
//              CognitiveLoadAnalytics V1 controller: cognitive-load reading capture, task
//              complexity profile definition, workload trend recalculation, and predictive
//              cognitive-risk generation/acknowledgement.
// ============================================================================

namespace PlantSafety.Api.HumanFactors.CognitiveLoadAnalytics.Contracts.Requests;

/// <summary>Request body for recording a single worker cognitive-workload measurement.</summary>
public sealed record RecordCognitiveLoadReadingRequest
{
    /// <summary>Optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Shift classification: Day, Evening, Night, or Rotating.</summary>
    public required string ShiftType { get; init; }

    /// <summary>UTC start-of-shift instant (identifies/creates the target ledger).</summary>
    public required DateTime ShiftDateUtc { get; init; }

    /// <summary>Worker this reading was measured for.</summary>
    public required Guid WorkerUserId { get; init; }

    /// <summary>Optional reference to the task complexity profile in force when this reading was taken.</summary>
    public Guid? TaskComplexityProfileId { get; init; }

    /// <summary>Measured cognitive workload score, 0-100.</summary>
    public required decimal CognitiveLoadScore { get; init; }

    /// <summary>Measurement instrument/method (e.g. "NASA-TLX", "Physiological", "SelfReport").</summary>
    public required string MeasurementMethod { get; init; }

    /// <summary>UTC instant the underlying measurement was taken.</summary>
    public required DateTime MeasuredAtUtc { get; init; }
}

/// <summary>Request body for defining a task-complexity profile.</summary>
public sealed record DefineTaskComplexityProfileRequest
{
    /// <summary>Optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Shift classification: Day, Evening, Night, or Rotating.</summary>
    public required string ShiftType { get; init; }

    /// <summary>UTC start-of-shift instant (identifies/creates the target ledger).</summary>
    public required DateTime ShiftDateUtc { get; init; }

    /// <summary>Short business code identifying the task.</summary>
    public required string TaskCode { get; init; }

    /// <summary>Human-readable task name.</summary>
    public required string TaskName { get; init; }

    /// <summary>Complexity rating, 1-10.</summary>
    public required int ComplexityScore { get; init; }

    /// <summary>Narrative description of what drives the task's complexity.</summary>
    public string? Description { get; init; }
}

/// <summary>Request body for recalculating a ledger's workload trend for a window.</summary>
public sealed record RecalculateWorkloadTrendRequest
{
    /// <summary>Window start.</summary>
    public required DateTime PeriodStartUtc { get; init; }

    /// <summary>Window end.</summary>
    public required DateTime PeriodEndUtc { get; init; }
}

/// <summary>Request body for generating a predictive cognitive-risk assessment for a worker.</summary>
public sealed record GenerateCognitiveRiskPredictionRequest
{
    /// <summary>Optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Worker this prediction is generated for.</summary>
    public required Guid WorkerUserId { get; init; }

    /// <summary>Source CognitiveLoadLedger this prediction is derived from.</summary>
    public required Guid SourceLedgerId { get; init; }

    /// <summary>Predictive model version identifier.</summary>
    public required string ModelVersion { get; init; }
}

/// <summary>Request body for acknowledging a generated cognitive-risk alert.</summary>
public sealed record AcknowledgeCognitiveRiskAlertRequest
{
    /// <summary>Optional free-text acknowledgement notes.</summary>
    public string? AcknowledgementNotes { get; init; }
}
