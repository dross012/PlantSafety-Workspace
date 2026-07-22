// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Shared result payload for MachineGuardsBarriers lifecycle commands ("MachineSafety.MachineGuardsBarriers").
// SchemaVersion: 1

namespace PlantSafety.Application.MachineSafety.Commands;

/// <summary>Result of a successful MachineGuardsBarriers lifecycle command execution.</summary>
/// <param name="MachineGuardsBarriersId">Identity of the affected MachineGuardsBarriers aggregate.</param>
/// <param name="Status">Lifecycle status of the record after the command was applied.</param>
public sealed record MachineGuardsBarriersActionResult(
    Guid MachineGuardsBarriersId,
    string Status);