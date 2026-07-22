// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Shared result payload for PermitTemplate lifecycle commands (publish,
// deprecate, retire, archive).
// SchemaVersion: 1

namespace PlantSafety.Application.PermitToWork.Commands;

/// <summary>Result of a successful PermitTemplate lifecycle command execution.</summary>
/// <param name="PermitTemplateId">Identity of the affected PermitTemplate aggregate.</param>
/// <param name="Status">Lifecycle status of the record after the command was applied.</param>
public sealed record PermitTemplateActionResult(
    Guid PermitTemplateId,
    string Status);
