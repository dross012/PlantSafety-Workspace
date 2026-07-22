// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for CreateAuditFindingCommand.
// SchemaVersion: 1

namespace PlantSafety.Application.Auditing.Commands;

/// <summary>Result of a successful <see cref="CreateAuditFindingCommand"/> execution.</summary>
/// <param name="AuditFindingId">Identity of the newly created AuditFinding aggregate.</param>
/// <param name="Status">Lifecycle status of the newly created record.</param>
public sealed record CreateAuditFindingResult(
    Guid AuditFindingId,
    string Status);