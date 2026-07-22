// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.EmergencyDrillPlanning
//  Layer:          Domain
//  File:           EmergencyDrillPlanningEventId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed identity value object for EmergencyDrillPlanning domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.EmergencyDrillPlanning.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for EmergencyDrillPlanning domain events.</summary>
public readonly record struct EmergencyDrillPlanningEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a EmergencyDrillPlanningEventId wrapping a non-empty GUID.</summary>
    public EmergencyDrillPlanningEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("EmergencyDrillPlanningEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new EmergencyDrillPlanningEventId using Uuid7.</summary>
    public static EmergencyDrillPlanningEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}