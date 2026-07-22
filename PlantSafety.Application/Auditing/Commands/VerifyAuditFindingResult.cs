// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Result payload for VerifyAuditFindingCommand.
// SchemaVersion: 1

namespace PlantSafety.Application.Auditing.Commands;

/// <summary>Result of a successful <see cref="VerifyAuditFindingCommand"/> execution.</summary>
/// <param name="AuditFindingId">Identity of the verified/closed AuditFinding.</param>
/// <param name="Status">Lifecycle status after verification.</param>
public sealed record VerifyAuditFindingResult(
    Guid AuditFindingId,
    string Status);
