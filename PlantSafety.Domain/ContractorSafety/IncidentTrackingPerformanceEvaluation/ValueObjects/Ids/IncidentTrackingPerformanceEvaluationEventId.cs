// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorSafety.IncidentTrackingPerformanceEvaluation
//  Layer:          Domain
//  File:           IncidentTrackingPerformanceEvaluationEventId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for IncidentTrackingPerformanceEvaluation domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContractorSafety.IncidentTrackingPerformanceEvaluation.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for IncidentTrackingPerformanceEvaluation domain events.</summary>
public readonly record struct IncidentTrackingPerformanceEvaluationEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a IncidentTrackingPerformanceEvaluationEventId wrapping a non-empty GUID.</summary>
    public IncidentTrackingPerformanceEvaluationEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("IncidentTrackingPerformanceEvaluationEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new IncidentTrackingPerformanceEvaluationEventId using Uuid7.</summary>
    public static IncidentTrackingPerformanceEvaluationEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}