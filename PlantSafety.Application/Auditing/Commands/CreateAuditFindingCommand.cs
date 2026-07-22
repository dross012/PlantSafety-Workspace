// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Command to create a new AuditFinding record within the Auditing module. Carries the full command envelope (CorrelationId, CausationId, CommandId, RequestedAtUtc, ApiVersionToken) per Handbook Rule 2.1.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Auditing.Commands;

/// <summary>
/// Creates a new AuditFinding record under the Auditing bounded context.
/// </summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier, constant across the whole request.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event. Null only for root operations.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued, generated once at the API boundary.</param>
/// <param name="ApiVersionToken">API version token the request was issued under.</param>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site scope. Null applies tenant-wide.</param>
/// <param name="OwnerId">Identity of the accountable owner / CRUD-authorship user.</param>
/// <param name="AuditEngagementId">Identifier of the parent AuditEngagement this finding is raised against.</param>
/// <param name="FindingTitle">Short finding title.</param>
/// <param name="Severity">Severity classification (Low/Medium/High/Critical).</param>
/// <param name="CorrectiveActionRequired">Whether a corrective action is required to close this finding.</param>
/// <param name="AssignedToUserId">Optional identity of the user assigned to remediate this finding.</param>
/// <param name="DueDateUtc">Optional corrective-action due date.</param>
public sealed record CreateAuditFindingCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid? SiteId,
    Guid OwnerId,
    Guid AuditEngagementId,
    string FindingTitle,
    string Severity,
    bool CorrectiveActionRequired,
    Guid? AssignedToUserId,
    DateTime? DueDateUtc) : ICommand<Result<CreateAuditFindingResult>>
{
    /// <inheritdoc/>
    public Guid ActorId => OwnerId;
    Guid? ICommand<Result<CreateAuditFindingResult>>.CausationId => CausationId;

    /// <summary>
    /// Builds a <see cref="CreateAuditFindingCommand"/> from raw, untrusted transport values, normalizing
    /// strings and generating envelope identifiers where the caller did not supply them.
    /// </summary>
    public static CreateAuditFindingCommand FromDto(
        Guid tenantId,
        Guid? siteId,
        Guid ownerId,
        Guid auditEngagementId,
        string findingTitle,
        string severity,
        bool correctiveActionRequired,
        Guid? assignedToUserId,
        DateTime? dueDateUtc,
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
            AuditEngagementId: auditEngagementId,
            FindingTitle: (findingTitle ?? string.Empty).Trim(),
            Severity: (severity ?? "Medium").Trim(),
            CorrectiveActionRequired: correctiveActionRequired,
            AssignedToUserId: assignedToUserId,
            DueDateUtc: dueDateUtc);
}