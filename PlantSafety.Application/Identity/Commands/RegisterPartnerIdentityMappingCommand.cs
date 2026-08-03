// AUTHOR: Enterprise Integration & Ecosystem Expansion Agent
// CREATED: 2026-08-02
// FILE DESCRIPTION: Command to register a new partner identity mapping (Phase 6
//   §2.2 "Tenant Federation" — Partner identity mapping). Carries the full
//   command envelope per Handbook Rule 2.1.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Identity.DTOs;

namespace PlantSafety.Application.Identity.Commands;

/// <summary>Registers a new partner identity mapping (partner external identity → internal user).</summary>
public sealed record RegisterPartnerIdentityMappingCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    string PartnerIssuer,
    string PartnerExternalId,
    Guid InternalUserId,
    Guid RequesterUserId) : ICommand<Result<PartnerIdentityMappingDto>>
{
    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => RequesterUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<PartnerIdentityMappingDto>>.CausationId => CausationId;

    /// <summary>Builds a <see cref="RegisterPartnerIdentityMappingCommand"/> from raw, untrusted transport values.</summary>
    public static RegisterPartnerIdentityMappingCommand FromDto(
        Guid tenantId,
        string partnerIssuer,
        string partnerExternalId,
        Guid internalUserId,
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
            PartnerIssuer: (partnerIssuer ?? string.Empty).Trim(),
            PartnerExternalId: (partnerExternalId ?? string.Empty).Trim(),
            InternalUserId: internalUserId,
            RequesterUserId: requesterUserId);
}
