// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Shared result payload for LotoSystems lifecycle commands ("MachineSafety.LotoSystems").
// SchemaVersion: 1

namespace PlantSafety.Application.MachineSafety.Commands;

/// <summary>Result of a successful LotoSystems lifecycle command execution.</summary>
/// <param name="LotoSystemsId">Identity of the affected LotoSystems aggregate.</param>
/// <param name="Status">Lifecycle status of the record after the command was applied.</param>
public sealed record LotoSystemsActionResult(
    Guid LotoSystemsId,
    string Status);