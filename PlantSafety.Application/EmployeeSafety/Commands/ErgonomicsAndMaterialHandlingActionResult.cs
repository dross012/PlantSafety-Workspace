// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Shared result payload for ErgonomicsAndMaterialHandling lifecycle commands ("EmployeeSafety.ErgonomicsAndMaterialHandling").
// SchemaVersion: 1

namespace PlantSafety.Application.EmployeeSafety.Commands;

/// <summary>Result of a successful ErgonomicsAndMaterialHandling lifecycle command execution.</summary>
/// <param name="ErgonomicsAndMaterialHandlingId">Identity of the affected ErgonomicsAndMaterialHandling aggregate.</param>
/// <param name="Status">Lifecycle status of the record after the command was applied.</param>
public sealed record ErgonomicsAndMaterialHandlingActionResult(
    Guid ErgonomicsAndMaterialHandlingId,
    string Status);