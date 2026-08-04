// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Shared result payload for CraneHoistSystems lifecycle commands ("MachineSafety.CraneHoistSystems").
// SchemaVersion: 1

namespace PlantSafety.Application.MachineSafety.Commands;

/// <summary>Result of a successful CraneHoistSystems lifecycle command execution.</summary>
/// <param name="CraneHoistSystemsId">Identity of the affected CraneHoistSystems aggregate.</param>
/// <param name="Status">Lifecycle status of the record after the command was applied.</param>
public sealed record CraneHoistSystemsActionResult(
    Guid CraneHoistSystemsId,
    string Status);
