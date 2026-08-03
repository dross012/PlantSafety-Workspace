// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.PredictiveScheduling
//  Layer:          Domain
//  File:           OptimizationRunId.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the OptimizationRun aggregate root.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;

namespace PlantSafety.Domain.Scheduling.PredictiveScheduling.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the OptimizationRun aggregate root.</summary>
public readonly record struct OptimizationRunId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates an OptimizationRunId wrapping a non-empty GUID.</summary>
    public OptimizationRunId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("OptimizationRunId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new OptimizationRunId using Uuid7.</summary>
    public static OptimizationRunId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as an OptimizationRunId.</summary>
    public static OptimizationRunId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into an OptimizationRunId.</summary>
    public static OptimizationRunId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
