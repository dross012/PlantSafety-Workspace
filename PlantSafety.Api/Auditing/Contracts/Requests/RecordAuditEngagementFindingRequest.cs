// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Request body for recording a finding reference against an Audit Engagement.

namespace PlantSafety.Api.Auditing.Contracts.Requests;

/// <summary>Request body for <c>POST /api/v1/auditing/engagements/{auditEngagementId}/findings/{auditFindingId}/record</c>.</summary>
/// <param name="Severity">FindingSeverity code being recorded against the engagement.</param>
public sealed record RecordAuditEngagementFindingRequest(string Severity);
