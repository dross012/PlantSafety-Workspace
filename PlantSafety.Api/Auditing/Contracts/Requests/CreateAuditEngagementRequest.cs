// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Request body for creating a new Audit Engagement over HTTP.

namespace PlantSafety.Api.Auditing.Contracts.Requests;

/// <summary>Request body for <c>POST /api/v1/auditing/engagements</c>.</summary>
/// <param name="SiteId">Optional site scope. Null applies tenant-wide.</param>
/// <param name="EngagementTitle">Human-readable title of the engagement.</param>
/// <param name="DisciplineType">AuditDisciplineType code.</param>
/// <param name="Scope">Narrative scope description.</param>
/// <param name="LeadAuditorId">Identity of the lead auditor.</param>
/// <param name="ScheduledAtUtc">Optional scheduled start instant (UTC).</param>
public sealed record CreateAuditEngagementRequest(
    Guid? SiteId,
    string EngagementTitle,
    string DisciplineType,
    string Scope,
    Guid LeadAuditorId,
    DateTime? ScheduledAtUtc);
