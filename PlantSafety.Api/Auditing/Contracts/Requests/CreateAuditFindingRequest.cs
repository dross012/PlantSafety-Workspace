// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Request body for creating a new Audit Finding over HTTP.

namespace PlantSafety.Api.Auditing.Contracts.Requests;

/// <summary>Request body for <c>POST /api/v1/auditing/findings</c>.</summary>
/// <param name="SiteId">Optional site scope. Null applies tenant-wide.</param>
/// <param name="AuditEngagementId">Parent engagement this finding belongs to.</param>
/// <param name="FindingTitle">Human-readable title of the finding.</param>
/// <param name="Severity">FindingSeverity code.</param>
/// <param name="CorrectiveActionRequired">Whether a corrective action is required.</param>
/// <param name="AssignedToUserId">Optional user assigned to remediate.</param>
/// <param name="DueDateUtc">Optional remediation due date (UTC).</param>
public sealed record CreateAuditFindingRequest(
    Guid? SiteId,
    Guid AuditEngagementId,
    string FindingTitle,
    string Severity,
    bool CorrectiveActionRequired,
    Guid? AssignedToUserId,
    DateTime? DueDateUtc);
