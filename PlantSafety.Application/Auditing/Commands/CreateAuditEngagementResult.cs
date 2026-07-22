// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for CreateAuditEngagementCommand.
// SchemaVersion: 1

namespace PlantSafety.Application.Auditing.Commands;

/// <summary>Result of a successful <see cref="CreateAuditEngagementCommand"/> execution.</summary>
/// <param name="AuditEngagementId">Identity of the newly created AuditEngagement aggregate.</param>
/// <param name="Status">Lifecycle status of the newly created record.</param>
public sealed record CreateAuditEngagementResult(
    Guid AuditEngagementId,
    string Status);