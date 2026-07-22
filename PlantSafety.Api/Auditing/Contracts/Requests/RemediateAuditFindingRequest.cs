// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Request body for remediating an Audit Finding over HTTP.

namespace PlantSafety.Api.Auditing.Contracts.Requests;

/// <summary>Request body for <c>POST /api/v1/auditing/findings/{auditFindingId}/remediate</c>.</summary>
/// <param name="RemediationNotes">Narrative description of the remediation performed.</param>
public sealed record RemediateAuditFindingRequest(string RemediationNotes);
