// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnterpriseFabric
//  Layer:          Application
//  File:           CreateEnterpriseFabricCommand.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Command to create a new EnterpriseFabric record under the EnterpriseFabric
//                  bounded context. Carries the full command envelope per Handbook Rule 2.1.
//                  Mirrors
//                  PlantSafety.Application.Compliance.Auditing.Commands.CreateAuditingCommand.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.EnterpriseFabric.Commands;

/// <summary>Creates a new EnterpriseFabric record.</summary>
public sealed record CreateEnterpriseFabricCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid? SiteId,
    string FabricArea,
    string SystemReference,
    string IntegrationType,
    Guid OwnerUserId,
    Guid RequesterUserId) : ICommand<Result<CreateEnterpriseFabricResult>>
{
    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => RequesterUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<CreateEnterpriseFabricResult>>.CausationId => CausationId;

    /// <summary>Builds a <see cref="CreateEnterpriseFabricCommand"/> from raw, untrusted transport values.</summary>
    public static CreateEnterpriseFabricCommand FromDto(
        Guid tenantId,
        Guid? siteId,
        string fabricArea,
        string systemReference,
        string integrationType,
        Guid ownerUserId,
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
            FabricArea: (fabricArea ?? string.Empty).Trim(),
            SystemReference: (systemReference ?? string.Empty).Trim(),
            IntegrationType: (integrationType ?? string.Empty).Trim(),
            OwnerUserId: ownerUserId,
            RequesterUserId: requesterUserId);
}
