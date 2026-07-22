// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Shared result payload for MachineGuardingInspections lifecycle commands ("SafetyProcedures.MachineGuardingInspections").
// SchemaVersion: 1

namespace PlantSafety.Application.SafetyProcedures.Commands;

/// <summary>Result of a successful MachineGuardingInspections lifecycle command execution.</summary>
/// <param name="MachineGuardingInspectionsId">Identity of the affected MachineGuardingInspections aggregate.</param>
/// <param name="Status">Lifecycle status of the record after the command was applied.</param>
public sealed record MachineGuardingInspectionsActionResult(
    Guid MachineGuardingInspectionsId,
    string Status);