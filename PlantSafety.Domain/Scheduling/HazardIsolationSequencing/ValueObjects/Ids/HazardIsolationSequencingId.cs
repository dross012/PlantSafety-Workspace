// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.HazardIsolationSequencing
//  Layer:          Domain
//  File:           HazardIsolationSequencingId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed identity value object for the HazardIsolationSequencing aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.HazardIsolationSequencing.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the HazardIsolationSequencing aggregate root.</summary>
public readonly record struct HazardIsolationSequencingId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a HazardIsolationSequencingId wrapping a non-empty GUID.</summary>
    public HazardIsolationSequencingId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("HazardIsolationSequencingId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new HazardIsolationSequencingId using Uuid7.</summary>
    public static HazardIsolationSequencingId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a HazardIsolationSequencingId.</summary>
    public static HazardIsolationSequencingId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a HazardIsolationSequencingId.</summary>
    public static HazardIsolationSequencingId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}