// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.PMScheduling
//  Layer:          Domain
//  File:           PMSchedulingId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed identity value object for the PMScheduling aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.PMScheduling.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the PMScheduling aggregate root.</summary>
public readonly record struct PMSchedulingId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a PMSchedulingId wrapping a non-empty GUID.</summary>
    public PMSchedulingId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("PMSchedulingId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new PMSchedulingId using Uuid7.</summary>
    public static PMSchedulingId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a PMSchedulingId.</summary>
    public static PMSchedulingId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a PMSchedulingId.</summary>
    public static PMSchedulingId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}