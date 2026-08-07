// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Placarding
//  Layer:          Application
//  File:           CreateHazmatContainerCommandHandler.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Handler for CreateHazmatContainerCommand. Pure orchestration: constructs the
//                  HazmatContainer aggregate via its domain factory method and persists it through
//                  the domain repository interface. No business rules live here. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.CreatePermitsCommandHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Compliance.PHMSA.Classification.ValueObjects.Ids;
using PlantSafety.Domain.Compliance.PHMSA.Packaging.ValueObjects.Ids;
using PlantSafety.Domain.Compliance.PHMSA.Placarding.Enums;
using PlantSafety.Domain.Compliance.PHMSA.Placarding.Interfaces;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Compliance.PHMSA.Placarding.Commands;

/// <summary>Handles <see cref="CreateHazmatContainerCommand"/>.</summary>
public sealed class CreateHazmatContainerCommandHandler : IRequestHandler<CreateHazmatContainerCommand, Result<CreateHazmatContainerResult>>
{
    private readonly IHazmatContainerRepository _repository;
    private readonly ILogger<CreateHazmatContainerCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CreateHazmatContainerCommandHandler(IHazmatContainerRepository repository, ILogger<CreateHazmatContainerCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CreateHazmatContainerResult>> Handle(CreateHazmatContainerCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var requesterId = UserId.FromGuid(request.RequestedByUserId);

            if (!Enum.TryParse<ContainerType>(request.ContainerType, ignoreCase: true, out var containerType))
            {
                return Result<CreateHazmatContainerResult>.Failure(new ApplicationError(
                    "Compliance.PHMSA.Placarding.InvalidContainerType",
                    $"'{request.ContainerType}' is not a recognized container type.",
                    Guid.NewGuid(),
                    ErrorSeverity.Validation));
            }

            var aggregate = PlantSafety.Domain.Compliance.PHMSA.Placarding.Aggregates.HazmatContainer.Create(
                id: PlantSafety.Domain.Compliance.PHMSA.Placarding.ValueObjects.Ids.HazmatContainerId.New(),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                containerType: containerType,
                materialId: HazmatMaterialId.FromGuid(request.MaterialId),
                packagingSpecId: PackagingSpecificationId.FromGuid(request.PackagingSpecId),
                createdBy: requesterId,
                createdAtUtc: UtcTimestamp.Now(),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(requesterId));

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<CreateHazmatContainerResult>.Success(new CreateHazmatContainerResult(
                aggregate.Id.Value,
                aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create HazmatContainer for Tenant {TenantId}", request.TenantId);

            return Result<CreateHazmatContainerResult>.Failure(new ApplicationError(
                "Compliance.PHMSA.Placarding.CreateFailed",
                "Unable to create the hazmat container record at this time.",
                Guid.NewGuid()));
        }
    }
}
