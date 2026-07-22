// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Command to create a new AuditEngagement record within the Auditing module. Carries the full command envelope (CorrelationId, CausationId, CommandId, RequestedAtUtc, ApiVersionToken) per Handbook Rule 2.1.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Auditing.Commands;

/// <summary>
/// Creates a new AuditEngagement record under the Auditing bounded context.
/// </summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier, constant across the whole request.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event. Null only for root operations.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued, generated once at the API boundary.</param>
/// <param name="ApiVersionToken">API version token the request was issued under.</param>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site scope. Null applies tenant-wide.</param>
/// <param name="OwnerId">Identity of the accountable owner / CRUD-authorship user.</param>
/// <param name="EngagementTitle">Human-readable audit engagement title.</param>
/// <param name="DisciplineType">AuditDisciplineType code (see Auditing.SmartEnums.AuditDisciplineType).</param>
/// <param name="Scope">Narrative scope of the audit.</param>
/// <param name="LeadAuditorId">Identity of the lead auditor assigned to this engagement.</param>
/// <param name="ScheduledAtUtc">Optional scheduled execution timestamp.</param>
public sealed record CreateAuditEngagementCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid? SiteId,
    Guid OwnerId,
    string EngagementTitle,
    string DisciplineType,
    string Scope,
    Guid LeadAuditorId,
    DateTime? ScheduledAtUtc) : ICommand<Result<CreateAuditEngagementResult>>
{
    /// <inheritdoc/>
    public Guid ActorId => OwnerId;
    Guid? ICommand<Result<CreateAuditEngagementResult>>.CausationId => CausationId;

    /// <summary>
    /// Builds a <see cref="CreateAuditEngagementCommand"/> from raw, untrusted transport values, normalizing
    /// strings and generating envelope identifiers where the caller did not supply them.
    /// </summary>
    public static CreateAuditEngagementCommand FromDto(
        Guid tenantId,
        Guid? siteId,
        Guid ownerId,
        string engagementTitle,
        string disciplineType,
        string scope,
        Guid leadAuditorId,
        DateTime? scheduledAtUtc,
        Guid? correlationId = null,
        Guid? causationId = null,
        string apiVersionToken = "v1")
        => new(
            CommandId: Guid.NewGuid(),
            CorrelationId: correlationId ?? Guid.NewGuid(),
            CausationId: causationId ?? Guid.NewGuid(),
            RequestedAtUtc: DateTime.UtcNow,
            ApiVersionToken: apiVersionToken,
            TenantId: tenantId,
            SiteId: siteId,
            OwnerId: ownerId,
            EngagementTitle: (engagementTitle ?? string.Empty).Trim(),
            DisciplineType: (disciplineType ?? string.Empty).Trim(),
            Scope: (scope ?? string.Empty).Trim(),
            LeadAuditorId: leadAuditorId,
            ScheduledAtUtc: scheduledAtUtc);
}