// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Command to record that a new AuditFinding has been raised against an in-progress AuditEngagement (Audit Workflow Step 3: Findings), incrementing the running findings count. Carries the full command envelope per Handbook Rule 2.1.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Auditing.Commands;

/// <summary>Records a finding against an in-progress AuditEngagement.</summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier, constant across the whole request.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event. Null only for root operations.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued, generated once at the API boundary.</param>
/// <param name="ApiVersionToken">API version token the request was issued under.</param>
/// <param name="OwnerId">Identity of the accountable owner / CRUD-authorship user.</param>
/// <param name="AuditEngagementId">Identity of the in-progress AuditEngagement.</param>
/// <param name="AuditFindingId">Identity of the AuditFinding aggregate already created for this observation.</param>
/// <param name="Severity">Finding severity: Low, Medium, High, or Critical.</param>
public sealed record RecordAuditEngagementFindingCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid OwnerId,
    Guid AuditEngagementId,
    Guid AuditFindingId,
    string Severity) : ICommand<Result<RecordAuditEngagementFindingResult>>
{
    /// <summary>Placeholder: no tenant field exists on this command's payload; defaults to <see cref="Guid.Empty"/> until tenant scoping is threaded through.</summary>
    public Guid TenantId { get; init; } = Guid.Empty;
    /// <inheritdoc/>
    public Guid ActorId => OwnerId;
    Guid? ICommand<Result<RecordAuditEngagementFindingResult>>.CausationId => CausationId;

    /// <summary>
    /// Builds a <see cref="RecordAuditEngagementFindingCommand"/> from raw, untrusted transport values, normalizing
    /// strings and generating envelope identifiers where the caller did not supply them.
    /// </summary>
    public static RecordAuditEngagementFindingCommand FromDto(
        Guid ownerId,
        Guid auditEngagementId,
        Guid auditFindingId,
        string severity,
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
            AuditFindingId: auditFindingId,
            Severity: (severity ?? "Medium").Trim());
}
