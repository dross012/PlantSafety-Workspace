// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Result payload for RemediateAuditFindingCommand.
// SchemaVersion: 1

namespace PlantSafety.Application.Auditing.Commands;

/// <summary>Result of a successful <see cref="RemediateAuditFindingCommand"/> execution.</summary>
/// <param name="AuditFindingId">Identity of the remediated AuditFinding.</param>
/// <param name="Status">Lifecycle status after remediation.</param>
public sealed record RemediateAuditFindingResult(
    Guid AuditFindingId,
    string Status);
