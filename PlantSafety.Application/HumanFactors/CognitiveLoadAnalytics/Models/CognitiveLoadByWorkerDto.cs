// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors.CognitiveLoadAnalytics
//  Layer:          Application
//  File:           CognitiveLoadByWorkerDto.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Read-model DTO backing GetCognitiveLoadByWorkerQuery: a single worker's
//                  cognitive-load rollup for a period. Data governance: single-worker rows are
//                  access-restricted; any cross-worker dashboard uses aggregated projections only.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Models;

/// <summary>A single worker's cognitive-load rollup for a period.</summary>
public sealed record CognitiveLoadByWorkerDto
{
    /// <summary>Worker identifier.</summary>
    public required Guid WorkerUserId { get; init; }

    /// <summary>Period start.</summary>
    public required DateTime PeriodStartUtc { get; init; }

    /// <summary>Period end.</summary>
    public required DateTime PeriodEndUtc { get; init; }

    /// <summary>Mean cognitive-load score across all readings in the period.</summary>
    public decimal MeanCognitiveLoadScore { get; init; }

    /// <summary>Highest single cognitive-load score recorded in the period.</summary>
    public decimal PeakCognitiveLoadScore { get; init; }

    /// <summary>Count of readings classified as overload events.</summary>
    public int OverloadEventCount { get; init; }

    /// <summary>Total count of readings contributing to this rollup.</summary>
    public int ReadingCount { get; init; }
}
