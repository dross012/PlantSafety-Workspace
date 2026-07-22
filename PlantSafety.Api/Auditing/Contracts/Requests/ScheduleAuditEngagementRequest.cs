// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Request body for scheduling an Audit Engagement over HTTP.

namespace PlantSafety.Api.Auditing.Contracts.Requests;

/// <summary>Request body for <c>POST /api/v1/auditing/engagements/{auditEngagementId}/schedule</c>.</summary>
/// <param name="LeadAuditorId">Identity of the lead auditor for this engagement.</param>
/// <param name="ScheduledAtUtc">Scheduled start instant (UTC).</param>
public sealed record ScheduleAuditEngagementRequest(
    Guid LeadAuditorId,
    DateTime ScheduledAtUtc);
