// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.EmergencyResponsePlanning
//  Layer:          Domain
//  File:           EmergencyResponsePlanningId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the EmergencyResponsePlanning aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.EmergencyResponsePlanning.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the EmergencyResponsePlanning aggregate root.</summary>
public readonly record struct EmergencyResponsePlanningId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a EmergencyResponsePlanningId wrapping a non-empty GUID.</summary>
    public EmergencyResponsePlanningId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("EmergencyResponsePlanningId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new EmergencyResponsePlanningId using Uuid7.</summary>
    public static EmergencyResponsePlanningId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a EmergencyResponsePlanningId.</summary>
    public static EmergencyResponsePlanningId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a EmergencyResponsePlanningId.</summary>
    public static EmergencyResponsePlanningId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}