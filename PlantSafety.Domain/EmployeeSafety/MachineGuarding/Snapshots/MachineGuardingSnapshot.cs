// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.MachineGuarding
//  Layer:          Domain
//  File:           MachineGuardingSnapshot.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for MachineGuarding aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.MachineGuarding.Snapshots;

/// <summary>Immutable snapshot of a <see cref="MachineGuarding"/> aggregate for fast rehydration.</summary>
public sealed record MachineGuardingSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public MachineGuardingId MachineGuardingId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public MachineGuardingStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}