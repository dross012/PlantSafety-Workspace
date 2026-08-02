// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Permits
//  Layer:          Application
//  File:           CreatePermitsCommand.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Command to submit a new Permit-to-Work request under the Compliance.Permits
//                  bounded context. Carries the full command envelope (CorrelationId, CausationId,
//                  CommandId, RequestedAtUtc, ApiVersionToken) per Handbook Rule 2.1. Mirrors
//                  PlantSafety.Application.PermitToWork.Commands.RequestPermitWorkflowCommand.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Compliance.Permits.Commands;

/// <summary>Submits a new Permits request.</summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier, constant across the whole request.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event. Null only for root operations.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued, generated once at the API boundary.</param>
/// <param name="ApiVersionToken">API version token the request was issued under.</param>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site scope. Null applies tenant-wide.</param>
/// <param name="PermitType">Permit classification (e.g., HotWork, ConfinedSpace, Excavation, LOTO).</param>
/// <param name="PermitNumber">Human-readable permit number/reference.</param>
/// <param name="WorkDescription">Description of the work covered by this permit.</param>
/// <param name="Location">Physical location the permit applies to.</param>
/// <param name="HazardsIdentified">Hazards identified for this permitted work.</param>
/// <param name="RequesterUserId">Identity of the worker or contractor submitting the request.</param>
public sealed record CreatePermitsCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid? SiteId,
    string PermitType,
    string PermitNumber,
    string WorkDescription,
    string Location,
    string HazardsIdentified,
    Guid RequesterUserId) : ICommand<Result<CreatePermitsResult>>
{
    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => RequesterUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<CreatePermitsResult>>.CausationId => CausationId;

    /// <summary>Builds a <see cref="CreatePermitsCommand"/> from raw, untrusted transport values.</summary>
    public static CreatePermitsCommand FromDto(
        Guid tenantId,
        Guid? siteId,
        string permitType,
        string permitNumber,
        string workDescription,
        string location,
        string hazardsIdentified,
        Guid requesterUserId,
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
            PermitType: (permitType ?? string.Empty).Trim(),
            PermitNumber: (permitNumber ?? string.Empty).Trim(),
            WorkDescription: (workDescription ?? string.Empty).Trim(),
            Location: (location ?? string.Empty).Trim(),
            HazardsIdentified: (hazardsIdentified ?? string.Empty).Trim(),
            RequesterUserId: requesterUserId);
}
