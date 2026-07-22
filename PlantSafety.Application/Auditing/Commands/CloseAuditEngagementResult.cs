// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Result payload for CloseAuditEngagementCommand.
// SchemaVersion: 1

namespace PlantSafety.Application.Auditing.Commands;

/// <summary>Result of a successful <see cref="CloseAuditEngagementCommand"/> execution.</summary>
/// <param name="AuditEngagementId">Identity of the closed AuditEngagement.</param>
/// <param name="Status">Lifecycle status after closing.</param>
/// <param name="ComplianceScore">Final compliance score.</param>
/// <param name="ComplianceRating">Final compliance rating.</param>
/// <param name="FindingsCount">Total findings recorded during this engagement.</param>
public sealed record CloseAuditEngagementResult(
    Guid AuditEngagementId,
    string Status,
    decimal? ComplianceScore,
    string ComplianceRating,
    int FindingsCount);
