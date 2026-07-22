// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Request body for closing an Audit Engagement over HTTP.

namespace PlantSafety.Api.Auditing.Contracts.Requests;

/// <summary>Request body for <c>POST /api/v1/auditing/engagements/{auditEngagementId}/close</c>.</summary>
/// <param name="ComplianceScore">Optional computed compliance score.</param>
/// <param name="ComplianceRating">ComplianceRating code assigned at close-out.</param>
public sealed record CloseAuditEngagementRequest(
    decimal? ComplianceScore,
    string ComplianceRating);
