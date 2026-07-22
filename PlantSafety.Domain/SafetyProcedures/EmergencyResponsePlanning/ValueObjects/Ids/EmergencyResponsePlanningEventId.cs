// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.EmergencyResponsePlanning
//  Layer:          Domain
//  File:           EmergencyResponsePlanningEventId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for EmergencyResponsePlanning domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.EmergencyResponsePlanning.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for EmergencyResponsePlanning domain events.</summary>
public readonly record struct EmergencyResponsePlanningEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a EmergencyResponsePlanningEventId wrapping a non-empty GUID.</summary>
    public EmergencyResponsePlanningEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("EmergencyResponsePlanningEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new EmergencyResponsePlanningEventId using Uuid7.</summary>
    public static EmergencyResponsePlanningEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}