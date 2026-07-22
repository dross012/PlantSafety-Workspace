// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Request body for triggering a CAPA workflow from an Audit Finding over HTTP.

namespace PlantSafety.Api.Auditing.Contracts.Requests;

/// <summary>Request body for <c>POST /api/v1/auditing/findings/{auditFindingId}/trigger-capa</c>.</summary>
/// <param name="AssignedToUserId">User assigned to own the CAPA.</param>
/// <param name="DueDateUtc">Due date for the CAPA (UTC).</param>
public sealed record TriggerAuditFindingCapaRequest(
    Guid AssignedToUserId,
    DateTime DueDateUtc);
