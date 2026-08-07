// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Packaging
//  Layer:          Application
//  File:           AuthorizePackagingCommandHandler.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Handler for AuthorizePackagingCommand. Pure orchestration: constructs the
//                  PackagingSpecification aggregate via its domain factory method and persists it
//                  through the domain repository interface. No business rules live here. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.CreatePermitsCommandHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Compliance.PHMSA.Classification.ValueObjects.Ids;
using PlantSafety.Domain.Compliance.PHMSA.Packaging.Interfaces;
using PlantSafety.Domain.Compliance.PHMSA.Packaging.ValueObjects;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Compliance.PHMSA.Packaging.Commands;

/// <summary>Handles <see cref="AuthorizePackagingCommand"/>.</summary>
public sealed class AuthorizePackagingCommandHandler : IRequestHandler<AuthorizePackagingCommand, Result<AuthorizePackagingResult>>
{
    private readonly IPackagingSpecificationRepository _repository;
    private readonly ILogger<AuthorizePackagingCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public AuthorizePackagingCommandHandler(IPackagingSpecificationRepository repository, ILogger<AuthorizePackagingCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<AuthorizePackagingResult>> Handle(AuthorizePackagingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var authorizedBy = UserId.FromGuid(request.AuthorizedByUserId);

            var aggregate = PlantSafety.Domain.Compliance.PHMSA.Packaging.Aggregates.PackagingSpecification.Create(
                dotSpec: new DOTSpec(request.DOTSpec),
                performanceTests: request.PerformanceTests ?? [],
                maxCapacity: request.MaxCapacity,
                capacityUnit: request.CapacityUnit,
                closureInstructions: request.ClosureInstructions,
                reuseRestrictions: request.ReuseRestrictions,
                associatedMaterialId: request.AssociatedMaterialId.HasValue ? HazmatMaterialId.FromGuid(request.AssociatedMaterialId.Value) : null,
                createdBy: authorizedBy,
                createdAtUtc: UtcTimestamp.Now(),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(authorizedBy),
                tenantId: request.TenantId,
                siteId: request.SiteId);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<AuthorizePackagingResult>.Success(new AuthorizePackagingResult(
                aggregate.Id.Value,
                aggregate.DOTSpec.Value,
                aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to authorize packaging specification for Tenant {TenantId}", request.TenantId);

            return Result<AuthorizePackagingResult>.Failure(new ApplicationError(
                "Compliance.PHMSA.Packaging.AuthorizeFailed",
                "Unable to authorize the packaging specification at this time.",
                Guid.NewGuid()));
        }
    }
}
