// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.PredictiveScheduling
//  Layer:          Domain
//  File:           OptimizationRunEventId.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for a single OptimizationRun domain
//                  event instance. Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;

namespace PlantSafety.Domain.Scheduling.PredictiveScheduling.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for a single OptimizationRun domain event instance.</summary>
public readonly record struct OptimizationRunEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates an OptimizationRunEventId wrapping a non-empty GUID.</summary>
    public OptimizationRunEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("OptimizationRunEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new OptimizationRunEventId using Uuid7.</summary>
    public static OptimizationRunEventId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as an OptimizationRunEventId.</summary>
    public static OptimizationRunEventId FromGuid(Guid value) => new(value);

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
