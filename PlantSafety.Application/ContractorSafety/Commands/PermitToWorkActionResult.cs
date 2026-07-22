// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Shared result payload for PermitToWork lifecycle commands ("ContractorSafety.PermitToWork").
// SchemaVersion: 1

namespace PlantSafety.Application.ContractorSafety.Commands;

/// <summary>Result of a successful PermitToWork lifecycle command execution.</summary>
/// <param name="PermitToWorkId">Identity of the affected PermitToWork aggregate.</param>
/// <param name="Status">Lifecycle status of the record after the command was applied.</param>
public sealed record PermitToWorkActionResult(
    Guid PermitToWorkId,
    string Status);