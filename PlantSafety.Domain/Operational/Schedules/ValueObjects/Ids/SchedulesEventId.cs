// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.Schedules
//  Layer:          Domain
//  File:           SchedulesEventId.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.Schedules.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for Schedules domain events.</summary>
public readonly record struct SchedulesEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a SchedulesEventId wrapping a non-empty GUID.</summary>
    public SchedulesEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("SchedulesEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new SchedulesEventId using Uuid7.</summary>
    public static SchedulesEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
