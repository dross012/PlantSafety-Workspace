// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Command to submit a new Permit-to-Work request (PTW Workflow Step 1 -
// Permit Request). Carries the full command envelope (CorrelationId, CausationId, CommandId,
// RequestedAtUtc, ApiVersionToken) per Handbook Rule 2.1.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.PermitToWork.Commands;

/// <summary>
/// Submits a new permit request under the PermitToWork bounded context (PTW Workflow Step 1).
/// </summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier, constant across the whole request.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event. Null only for root operations.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued, generated once at the API boundary.</param>
/// <param name="ApiVersionToken">API version token the request was issued under.</param>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site scope. Null applies tenant-wide.</param>
/// <param name="PermitNumber">Human-readable permit number/identifier.</param>
/// <param name="PermitTypeCode">Permit category code (e.g. HOT-WORK, CONFINED-SPACE, ELECTRICAL-ISOLATION, WORKING-AT-HEIGHT, EXCAVATION).</param>
/// <param name="RequesterUserId">Identity of the worker or contractor submitting the request.</param>
/// <param name="WorkDescription">Scope-of-work description: task, exact location, equipment involved, expected duration.</param>
public sealed record RequestPermitWorkflowCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid? SiteId,
    string PermitNumber,
    string PermitTypeCode,
    Guid RequesterUserId,
    string WorkDescription) : ICommand<Result<PermitWorkflowActionResult>>
{
    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => RequesterUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<PermitWorkflowActionResult>>.CausationId => CausationId;

    /// <summary>Builds a <see cref="RequestPermitWorkflowCommand"/> from raw, untrusted transport values.</summary>
    public static RequestPermitWorkflowCommand FromDto(
        Guid tenantId,
        Guid? siteId,
        string permitNumber,
        string permitTypeCode,
        Guid requesterUserId,
        string workDescription,
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
            PermitNumber: (permitNumber ?? string.Empty).Trim(),
            PermitTypeCode: (permitTypeCode ?? string.Empty).Trim(),
            RequesterUserId: requesterUserId,
            WorkDescription: (workDescription ?? string.Empty).Trim());
}
