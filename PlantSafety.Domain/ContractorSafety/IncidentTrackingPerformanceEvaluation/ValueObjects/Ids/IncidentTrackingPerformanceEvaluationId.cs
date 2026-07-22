// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorSafety.IncidentTrackingPerformanceEvaluation
//  Layer:          Domain
//  File:           IncidentTrackingPerformanceEvaluationId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the IncidentTrackingPerformanceEvaluation aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContractorSafety.IncidentTrackingPerformanceEvaluation.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the IncidentTrackingPerformanceEvaluation aggregate root.</summary>
public readonly record struct IncidentTrackingPerformanceEvaluationId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a IncidentTrackingPerformanceEvaluationId wrapping a non-empty GUID.</summary>
    public IncidentTrackingPerformanceEvaluationId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("IncidentTrackingPerformanceEvaluationId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new IncidentTrackingPerformanceEvaluationId using Uuid7.</summary>
    public static IncidentTrackingPerformanceEvaluationId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a IncidentTrackingPerformanceEvaluationId.</summary>
    public static IncidentTrackingPerformanceEvaluationId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a IncidentTrackingPerformanceEvaluationId.</summary>
    public static IncidentTrackingPerformanceEvaluationId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}