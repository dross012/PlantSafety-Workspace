// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Shared result payload for MaterialHandlingStorage lifecycle commands ("SafetyProcedures.MaterialHandlingStorage").
// SchemaVersion: 1

namespace PlantSafety.Application.SafetyProcedures.Commands;

/// <summary>Result of a successful MaterialHandlingStorage lifecycle command execution.</summary>
/// <param name="MaterialHandlingStorageId">Identity of the affected MaterialHandlingStorage aggregate.</param>
/// <param name="Status">Lifecycle status of the record after the command was applied.</param>
public sealed record MaterialHandlingStorageActionResult(
    Guid MaterialHandlingStorageId,
    string Status);