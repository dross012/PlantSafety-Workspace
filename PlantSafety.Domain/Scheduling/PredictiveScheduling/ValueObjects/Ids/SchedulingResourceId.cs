// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.PredictiveScheduling
//  Layer:          Domain
//  File:           SchedulingResourceId.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Phase 5 Predictive Scheduling Engine)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed identity for a schedulable resource (crew, machine, isolation
//                  team) tracked by the resource availability model. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.PredictiveScheduling.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for a schedulable resource (crew or machine).</summary>
public readonly record struct SchedulingResourceId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a SchedulingResourceId wrapping a non-empty GUID.</summary>
    public SchedulingResourceId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("SchedulingResourceId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new SchedulingResourceId using Uuid7.</summary>
    public static SchedulingResourceId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a SchedulingResourceId.</summary>
    public static SchedulingResourceId FromGuid(Guid value) => new(value);

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
