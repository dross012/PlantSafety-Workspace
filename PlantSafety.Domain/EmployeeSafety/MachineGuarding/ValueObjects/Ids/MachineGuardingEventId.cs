// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.MachineGuarding
//  Layer:          Domain
//  File:           MachineGuardingEventId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for MachineGuarding domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.MachineGuarding.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for MachineGuarding domain events.</summary>
public readonly record struct MachineGuardingEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a MachineGuardingEventId wrapping a non-empty GUID.</summary>
    public MachineGuardingEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("MachineGuardingEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new MachineGuardingEventId using Uuid7.</summary>
    public static MachineGuardingEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}