// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.EmergencyDrillPlanning
//  Layer:          Domain
//  File:           EmergencyDrillPlanningId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed identity value object for the EmergencyDrillPlanning aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.EmergencyDrillPlanning.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the EmergencyDrillPlanning aggregate root.</summary>
public readonly record struct EmergencyDrillPlanningId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a EmergencyDrillPlanningId wrapping a non-empty GUID.</summary>
    public EmergencyDrillPlanningId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("EmergencyDrillPlanningId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new EmergencyDrillPlanningId using Uuid7.</summary>
    public static EmergencyDrillPlanningId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a EmergencyDrillPlanningId.</summary>
    public static EmergencyDrillPlanningId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a EmergencyDrillPlanningId.</summary>
    public static EmergencyDrillPlanningId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}