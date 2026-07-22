// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.MachineGuardingInspections
//  Layer:          Domain
//  File:           MachineGuardingInspectionsEventId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for MachineGuardingInspections domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.MachineGuardingInspections.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for MachineGuardingInspections domain events.</summary>
public readonly record struct MachineGuardingInspectionsEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a MachineGuardingInspectionsEventId wrapping a non-empty GUID.</summary>
    public MachineGuardingInspectionsEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("MachineGuardingInspectionsEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new MachineGuardingInspectionsEventId using Uuid7.</summary>
    public static MachineGuardingInspectionsEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}