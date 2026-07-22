// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.HazardIsolationSequencing
//  Layer:          Domain
//  File:           HazardIsolationSequencingEventId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed identity value object for HazardIsolationSequencing domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.HazardIsolationSequencing.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for HazardIsolationSequencing domain events.</summary>
public readonly record struct HazardIsolationSequencingEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a HazardIsolationSequencingEventId wrapping a non-empty GUID.</summary>
    public HazardIsolationSequencingEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("HazardIsolationSequencingEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new HazardIsolationSequencingEventId using Uuid7.</summary>
    public static HazardIsolationSequencingEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}