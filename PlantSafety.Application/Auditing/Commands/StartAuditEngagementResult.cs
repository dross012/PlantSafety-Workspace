// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Result payload for StartAuditEngagementCommand.
// SchemaVersion: 1

namespace PlantSafety.Application.Auditing.Commands;

/// <summary>Result of a successful <see cref="StartAuditEngagementCommand"/> execution.</summary>
/// <param name="AuditEngagementId">Identity of the started AuditEngagement.</param>
/// <param name="Status">Lifecycle status after starting.</param>
public sealed record StartAuditEngagementResult(
    Guid AuditEngagementId,
    string Status);
