// AUTHOR: Enterprise Integration & Ecosystem Expansion Agent
// CREATED: 2026-08-02
// FILE DESCRIPTION: Handler for RegisterPartnerIdentityMappingCommand. Pure
//   orchestration: guards against a duplicate active mapping, constructs the
//   PartnerIdentityMapping aggregate via its domain factory method, and
//   persists it through the domain repository interface.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Identity.Commands;
using PlantSafety.Application.Identity.DTOs;
using PlantSafety.Application.Identity.Mappers;
using PlantSafety.Domain.Identity.Repositories;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;

namespace PlantSafety.Application.Identity.CommandHandlers;

/// <summary>Handles <see cref="RegisterPartnerIdentityMappingCommand"/>.</summary>
public sealed class RegisterPartnerIdentityMappingCommandHandler
    : IRequestHandler<RegisterPartnerIdentityMappingCommand, Result<PartnerIdentityMappingDto>>
{
    private readonly IPartnerIdentityMappingRepository _repository;
    private readonly ILogger<RegisterPartnerIdentityMappingCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    /// <param name="repository">The partner identity mapping repository.</param>
    /// <param name="logger">The structured logger.</param>
    public RegisterPartnerIdentityMappingCommandHandler(
        IPartnerIdentityMappingRepository repository,
        ILogger<RegisterPartnerIdentityMappingCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PartnerIdentityMappingDto>> Handle(
        RegisterPartnerIdentityMappingCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var existing = await _repository.FindActiveAsync(
                request.TenantId, request.PartnerIssuer, request.PartnerExternalId, cancellationToken);

            if (existing is not null)
            {
                return Result<PartnerIdentityMappingDto>.Failure(new ApplicationError(
                    "Identity.PartnerIdentityMapping.AlreadyExists",
                    "A partner identity mapping already exists for this issuer and external identity.",
                    request.CorrelationId,
                    ErrorSeverity.Domain));
            }

            var requesterId = UserId.FromGuid(request.RequesterUserId);
            var internalUserId = UserId.FromGuid(request.InternalUserId);

            var aggregate = Domain.Identity.Aggregates.PartnerIdentityMapping.Register(
                registeredBy: requesterId,
                registeredAtUtc: UtcTimestamp.Now(),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(requesterId),
                tenantId: request.TenantId,
                partnerIssuer: request.PartnerIssuer,
                partnerExternalId: request.PartnerExternalId,
                internalUserId: internalUserId);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<PartnerIdentityMappingDto>.Success(PartnerIdentityMappingMapper.ToDto(aggregate));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to register partner identity mapping for Tenant {TenantId}, Issuer {PartnerIssuer}",
                request.TenantId, request.PartnerIssuer);

            return Result<PartnerIdentityMappingDto>.Failure(new ApplicationError(
                "Identity.PartnerIdentityMapping.RegisterFailed",
                "Unable to register the partner identity mapping at this time.",
                request.CorrelationId));
        }
    }
}
