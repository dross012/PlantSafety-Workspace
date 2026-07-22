// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.PMScheduling
//  Layer:          Domain
//  File:           PMSchedulingEventId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed identity value object for PMScheduling domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.PMScheduling.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for PMScheduling domain events.</summary>
public readonly record struct PMSchedulingEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a PMSchedulingEventId wrapping a non-empty GUID.</summary>
    public PMSchedulingEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("PMSchedulingEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new PMSchedulingEventId using Uuid7.</summary>
    public static PMSchedulingEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}