// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.Schedules
//  Layer:          Domain
//  File:           SchedulesId.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.Schedules.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the Schedules aggregate root.</summary>
public readonly record struct SchedulesId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a SchedulesId wrapping a non-empty GUID.</summary>
    public SchedulesId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("SchedulesId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new SchedulesId using Uuid7.</summary>
    public static SchedulesId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a SchedulesId.</summary>
    public static SchedulesId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a SchedulesId.</summary>
    public static SchedulesId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
