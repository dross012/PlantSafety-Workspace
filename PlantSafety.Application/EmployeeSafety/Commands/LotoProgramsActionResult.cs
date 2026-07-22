// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Shared result payload for LotoPrograms lifecycle commands ("EmployeeSafety.LotoPrograms").
// SchemaVersion: 1

namespace PlantSafety.Application.EmployeeSafety.Commands;

/// <summary>Result of a successful LotoPrograms lifecycle command execution.</summary>
/// <param name="LotoProgramsId">Identity of the affected LotoPrograms aggregate.</param>
/// <param name="Status">Lifecycle status of the record after the command was applied.</param>
public sealed record LotoProgramsActionResult(
    Guid LotoProgramsId,
    string Status);