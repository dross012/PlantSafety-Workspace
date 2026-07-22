// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Command to plan and schedule an AuditEngagement (Audit Workflow Step 1: Planning) -- assigns the lead auditor and sets the execution date. Carries the full command envelope per Handbook Rule 2.1.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Auditing.Commands;

/// <summary>Schedules an AuditEngagement under the Auditing bounded context.</summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier, constant across the whole request.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event. Null only for root operations.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued, generated once at the API boundary.</param>
/// <param name="ApiVersionToken">API version token the request was issued under.</param>
/// <param name="OwnerId">Identity of the accountable owner / CRUD-authorship user.</param>
/// <param name="AuditEngagementId">Identity of the AuditEngagement to schedule.</param>
/// <param name="LeadAuditorId">Identity of the lead auditor assigned to this engagement.</param>
/// <param name="ScheduledAtUtc">Planned execution date/time for the audit.</param>
public sealed record ScheduleAuditEngagementCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid OwnerId,
    Guid AuditEngagementId,
    Guid LeadAuditorId,
    DateTime ScheduledAtUtc) : ICommand<Result<ScheduleAuditEngagementResult>>
{
    /// <summary>Placeholder: no tenant field exists on this command's payload; defaults to <see cref="Guid.Empty"/> until tenant scoping is threaded through.</summary>
    public Guid TenantId { get; init; } = Guid.Empty;
    /// <inheritdoc/>
    public Guid ActorId => OwnerId;
    Guid? ICommand<Result<ScheduleAuditEngagementResult>>.CausationId => CausationId;

    /// <summary>
    /// Builds a <see cref="ScheduleAuditEngagementCommand"/> from raw, untrusted transport values, generating
    /// envelope identifiers where the caller did not supply them.
    /// </summary>
    public static ScheduleAuditEngagementCommand FromDto(
        Guid ownerId,
        Guid auditEngagementId,
        Guid leadAuditorId,
        DateTime scheduledAtUtc,
        Guid? correlationId = null,
        Guid? causationId = null,
        string apiVersionToken = "v1")
        => new(
            CommandId: Guid.NewGuid(),
            CorrelationId: correlationId ?? Guid.NewGuid(),
            CausationId: causationId ?? Guid.NewGuid(),
            RequestedAtUtc: DateTime.UtcNow,
            ApiVersionToken: apiVersionToken,
            OwnerId: ownerId,
            AuditEngagementId: auditEngagementId,
            LeadAuditorId: leadAuditorId,
            ScheduledAtUtc: scheduledAtUtc);
}
