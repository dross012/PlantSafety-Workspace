// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorSafety.IncidentTrackingPerformanceEvaluation
//  Layer:          Domain
//  File:           IncidentTrackingPerformanceEvaluationSnapshot.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for IncidentTrackingPerformanceEvaluation aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContractorSafety.IncidentTrackingPerformanceEvaluation.Snapshots;

/// <summary>Immutable snapshot of a <see cref="IncidentTrackingPerformanceEvaluation"/> aggregate for fast rehydration.</summary>
public sealed record IncidentTrackingPerformanceEvaluationSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public IncidentTrackingPerformanceEvaluationId IncidentTrackingPerformanceEvaluationId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public IncidentTrackingPerformanceEvaluationStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}