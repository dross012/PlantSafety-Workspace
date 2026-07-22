// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Shared result payload for PermitWorkflow lifecycle commands (request, hazard
// assessment, control implementation, approval, issuance, closure, archive, rejection, expiration).
// SchemaVersion: 1

namespace PlantSafety.Application.PermitToWork.Commands;

/// <summary>Result of a successful PermitWorkflow lifecycle command execution.</summary>
/// <param name="PermitWorkflowId">Identity of the affected PermitWorkflow aggregate.</param>
/// <param name="Status">Lifecycle status of the record after the command was applied.</param>
public sealed record PermitWorkflowActionResult(
    Guid PermitWorkflowId,
    string Status);
