// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.MachineGuardingInspections
//  Layer:          Domain
//  File:           MachineGuardingInspectionsId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the MachineGuardingInspections aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.MachineGuardingInspections.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the MachineGuardingInspections aggregate root.</summary>
public readonly record struct MachineGuardingInspectionsId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a MachineGuardingInspectionsId wrapping a non-empty GUID.</summary>
    public MachineGuardingInspectionsId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("MachineGuardingInspectionsId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new MachineGuardingInspectionsId using Uuid7.</summary>
    public static MachineGuardingInspectionsId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a MachineGuardingInspectionsId.</summary>
    public static MachineGuardingInspectionsId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a MachineGuardingInspectionsId.</summary>
    public static MachineGuardingInspectionsId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}