// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Shared result payload for LotoProcedures lifecycle commands ("SafetyProcedures.LotoProcedures").
// SchemaVersion: 1

namespace PlantSafety.Application.SafetyProcedures.Commands;

/// <summary>Result of a successful LotoProcedures lifecycle command execution.</summary>
/// <param name="LotoProceduresId">Identity of the affected LotoProcedures aggregate.</param>
/// <param name="Status">Lifecycle status of the record after the command was applied.</param>
public sealed record LotoProceduresActionResult(
    Guid LotoProceduresId,
    string Status);