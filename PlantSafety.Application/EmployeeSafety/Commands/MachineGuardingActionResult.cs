// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Shared result payload for MachineGuarding lifecycle commands ("EmployeeSafety.MachineGuarding").
// SchemaVersion: 1

namespace PlantSafety.Application.EmployeeSafety.Commands;

/// <summary>Result of a successful MachineGuarding lifecycle command execution.</summary>
/// <param name="MachineGuardingId">Identity of the affected MachineGuarding aggregate.</param>
/// <param name="Status">Lifecycle status of the record after the command was applied.</param>
public sealed record MachineGuardingActionResult(
    Guid MachineGuardingId,
    string Status);