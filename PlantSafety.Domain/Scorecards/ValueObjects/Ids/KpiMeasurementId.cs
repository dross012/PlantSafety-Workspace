// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scorecards
//  Layer:          Domain
//  File:           KpiMeasurementId.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for a KpiMeasurement entity.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;

namespace PlantSafety.Domain.Scorecards.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for a KpiMeasurement entity.</summary>
public readonly record struct KpiMeasurementId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a KpiMeasurementId wrapping a non-empty GUID.</summary>
    public KpiMeasurementId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("KpiMeasurementId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new KpiMeasurementId using Uuid7.</summary>
    public static KpiMeasurementId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a KpiMeasurementId.</summary>
    public static KpiMeasurementId FromGuid(Guid value) => new(value);

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
