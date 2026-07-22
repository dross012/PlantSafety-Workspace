// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Result payload for RecordAuditEngagementFindingCommand.
// SchemaVersion: 1

namespace PlantSafety.Application.Auditing.Commands;

/// <summary>Result of a successful <see cref="RecordAuditEngagementFindingCommand"/> execution.</summary>
/// <param name="AuditEngagementId">Identity of the AuditEngagement the finding was recorded against.</param>
/// <param name="FindingsCount">Running total of findings recorded on this engagement.</param>
public sealed record RecordAuditEngagementFindingResult(
    Guid AuditEngagementId,
    int FindingsCount);
