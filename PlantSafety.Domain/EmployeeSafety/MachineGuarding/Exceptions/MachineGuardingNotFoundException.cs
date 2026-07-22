// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.MachineGuarding
//  Layer:          Domain
//  File:           MachineGuardingNotFoundException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a MachineGuarding aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.MachineGuarding.Exceptions;

/// <summary>Thrown when a <see cref="MachineGuarding"/> aggregate lookup finds no match.</summary>
public sealed class MachineGuardingNotFoundException : MachineGuardingDomainException
{
    /// <summary>Creates a not-found exception for the given MachineGuarding identity.</summary>
    public MachineGuardingNotFoundException(MachineGuardingId id, CorrelationId correlationId, CausationId causationId)
        : base(
            "MachineGuarding with id '{id}' was not found.",
            "MachineGuarding.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}