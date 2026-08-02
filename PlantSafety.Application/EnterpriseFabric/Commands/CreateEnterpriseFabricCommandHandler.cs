// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnterpriseFabric
//  Layer:          Application
//  File:           CreateEnterpriseFabricCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for CreateEnterpriseFabricCommand. Pure orchestration: constructs the
//                  EnterpriseFabric aggregate via its domain factory method and persists it
//                  through the domain repository interface. Mirrors
//                  PlantSafety.Application.Compliance.Auditing.Commands.CreateAuditingCommandHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.EnterpriseFabric.Repositories;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.EnterpriseFabric.Commands;

/// <summary>Handles <see cref="CreateEnterpriseFabricCommand"/>.</summary>
public sealed class CreateEnterpriseFabricCommandHandler : IRequestHandler<CreateEnterpriseFabricCommand, Result<CreateEnterpriseFabricResult>>
{
    private readonly IEnterpriseFabricRepository _repository;
    private readonly ILogger<CreateEnterpriseFabricCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CreateEnterpriseFabricCommandHandler(IEnterpriseFabricRepository repository, ILogger<CreateEnterpriseFabricCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CreateEnterpriseFabricResult>> Handle(CreateEnterpriseFabricCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var requesterId = UserId.FromGuid(request.RequesterUserId);
            var ownerId = UserId.FromGuid(request.OwnerUserId);

            var aggregate = PlantSafety.Domain.EnterpriseFabric.Aggregates.EnterpriseFabric.Create(
                createdBy: requesterId,
                createdAtUtc: UtcTimestamp.Now(),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(requesterId),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                fabricArea: request.FabricArea,
                systemReference: request.SystemReference,
                integrationType: request.IntegrationType,
                ownerUserId: ownerId);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<CreateEnterpriseFabricResult>.Success(new CreateEnterpriseFabricResult(
                aggregate.EnterpriseFabricId.Value,
                aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create EnterpriseFabric for Tenant {TenantId}", request.TenantId);

            return Result<CreateEnterpriseFabricResult>.Failure(new ApplicationError(
                "EnterpriseFabric.CreateFailed",
                "Unable to create the enterprise fabric record at this time.",
                Guid.NewGuid()));
        }
    }
}
