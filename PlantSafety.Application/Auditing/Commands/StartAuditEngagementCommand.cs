// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Command to begin field execution of an AuditEngagement (Audit Workflow Step 2: Execution). Carries the full command envelope per Handbook Rule 2.1.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Auditing.Commands;

/// <summary>Starts field execution of an AuditEngagement under the Auditing bounded context.</summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier, constant across the whole request.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event. Null only for root operations.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued, generated once at the API boundary.</param>
/// <param name="ApiVersionToken">API version token the request was issued under.</param>
/// <param name="OwnerId">Identity of the accountable owner / CRUD-authorship user.</param>
/// <param name="AuditEngagementId">Identity of the AuditEngagement to start.</param>
public sealed record StartAuditEngagementCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid OwnerId,
    Guid AuditEngagementId) : ICommand<Result<StartAuditEngagementResult>>
{
    /// <summary>Placeholder: no tenant field exists on this command's payload; defaults to <see cref="Guid.Empty"/> until tenant scoping is threaded through.</summary>
    public Guid TenantId { get; init; } = Guid.Empty;
    /// <inheritdoc/>
    public Guid ActorId => OwnerId;
    Guid? ICommand<Result<StartAuditEngagementResult>>.CausationId => CausationId;

    /// <summary>
    /// Builds a <see cref="StartAuditEngagementCommand"/> from raw, untrusted transport values, generating
    /// envelope identifiers where the caller did not supply them.
    /// </summary>
    public static StartAuditEngagementCommand FromDto(
        Guid ownerId,
        Guid auditEngagementId,
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
            AuditEngagementId: auditEngagementId);
}
