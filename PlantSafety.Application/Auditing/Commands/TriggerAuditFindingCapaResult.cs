// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Result payload for TriggerAuditFindingCapaCommand.
// SchemaVersion: 1

namespace PlantSafety.Application.Auditing.Commands;

/// <summary>Result of a successful <see cref="TriggerAuditFindingCapaCommand"/> execution.</summary>
/// <param name="AuditFindingId">Identity of the AuditFinding the CAPA was triggered for.</param>
/// <param name="Status">Lifecycle status after triggering the CAPA.</param>
/// <param name="DueDateUtc">Confirmed corrective action due date.</param>
public sealed record TriggerAuditFindingCapaResult(
    Guid AuditFindingId,
    string Status,
    DateTime DueDateUtc);
