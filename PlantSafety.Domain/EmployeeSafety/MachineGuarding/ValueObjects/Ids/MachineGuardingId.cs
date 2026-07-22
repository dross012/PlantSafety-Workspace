// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.MachineGuarding
//  Layer:          Domain
//  File:           MachineGuardingId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the MachineGuarding aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.MachineGuarding.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the MachineGuarding aggregate root.</summary>
public readonly record struct MachineGuardingId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a MachineGuardingId wrapping a non-empty GUID.</summary>
    public MachineGuardingId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("MachineGuardingId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new MachineGuardingId using Uuid7.</summary>
    public static MachineGuardingId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a MachineGuardingId.</summary>
    public static MachineGuardingId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a MachineGuardingId.</summary>
    public static MachineGuardingId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}