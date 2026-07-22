// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Command to trigger a Corrective and Preventive Action (CAPA) for an AuditFinding (Audit Workflow Step 4: Corrective Actions), assigning a responsible user and due date. Carries the full command envelope per Handbook Rule 2.1.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Auditing.Commands;

/// <summary>Triggers a CAPA for an AuditFinding under the Auditing bounded context.</summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier, constant across the whole request.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event. Null only for root operations.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued, generated once at the API boundary.</param>
/// <param name="ApiVersionToken">API version token the request was issued under.</param>
/// <param name="OwnerId">Identity of the accountable owner / CRUD-authorship user.</param>
/// <param name="AuditFindingId">Identity of the AuditFinding to trigger a CAPA for.</param>
/// <param name="AssignedToUserId">Identity of the user responsible for performing the corrective action.</param>
/// <param name="DueDateUtc">Due date by which the corrective action must be completed.</param>
public sealed record TriggerAuditFindingCapaCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid OwnerId,
    Guid AuditFindingId,
    Guid AssignedToUserId,
    DateTime DueDateUtc) : ICommand<Result<TriggerAuditFindingCapaResult>>
{
    /// <summary>Placeholder: no tenant field exists on this command's payload; defaults to <see cref="Guid.Empty"/> until tenant scoping is threaded through.</summary>
    public Guid TenantId { get; init; } = Guid.Empty;
    /// <inheritdoc/>
    public Guid ActorId => OwnerId;
    Guid? ICommand<Result<TriggerAuditFindingCapaResult>>.CausationId => CausationId;

    /// <summary>
    /// Builds a <see cref="TriggerAuditFindingCapaCommand"/> from raw, untrusted transport values, generating
    /// envelope identifiers where the caller did not supply them.
    /// </summary>
    public static TriggerAuditFindingCapaCommand FromDto(
        Guid ownerId,
        Guid auditFindingId,
        Guid assignedToUserId,
        DateTime dueDateUtc,
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
            AuditFindingId: auditFindingId,
            AssignedToUserId: assignedToUserId,
            DueDateUtc: dueDateUtc);
}
