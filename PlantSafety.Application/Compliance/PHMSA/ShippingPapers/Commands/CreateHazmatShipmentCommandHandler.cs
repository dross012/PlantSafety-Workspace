// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.ShippingPapers
//  Layer:          Application
//  File:           CreateHazmatShipmentCommandHandler.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Handler for CreateHazmatShipmentCommand. Pure orchestration: constructs the
//                  HazmatShipment aggregate via its domain factory method and persists it through
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
using PlantSafety.Domain.Compliance.PHMSA.ShippingPapers.Enums;
using PlantSafety.Domain.Compliance.PHMSA.ShippingPapers.Interfaces;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Compliance.PHMSA.ShippingPapers.Commands;

/// <summary>Handles <see cref="CreateHazmatShipmentCommand"/>.</summary>
public sealed class CreateHazmatShipmentCommandHandler : IRequestHandler<CreateHazmatShipmentCommand, Result<CreateHazmatShipmentResult>>
{
    private readonly IHazmatShipmentRepository _repository;
    private readonly ILogger<CreateHazmatShipmentCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CreateHazmatShipmentCommandHandler(IHazmatShipmentRepository repository, ILogger<CreateHazmatShipmentCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CreateHazmatShipmentResult>> Handle(CreateHazmatShipmentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (!Enum.TryParse<TransportMode>(request.Mode, ignoreCase: true, out var mode))
            {
                return Result<CreateHazmatShipmentResult>.Failure(new ApplicationError(
                    "Compliance.PHMSA.ShippingPapers.InvalidMode",
                    $"'{request.Mode}' is not a recognized transport mode.",
                    Guid.NewGuid(),
                    ErrorSeverity.Validation));
            }

            var requesterId = UserId.FromGuid(request.RequestedByUserId);

            var materialIds = (request.MaterialIds ?? []).Select(HazmatMaterialId.FromGuid).ToList();
            var packagingIds = (request.PackagingIds ?? []).Select(PackagingSpecificationId.FromGuid).ToList();

            var aggregate = PlantSafety.Domain.Compliance.PHMSA.ShippingPapers.Aggregates.HazmatShipment.Create(
                id: PlantSafety.Domain.Compliance.PHMSA.ShippingPapers.ValueObjects.Ids.HazmatShipmentId.New(),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                materialIds: materialIds,
                packagingIds: packagingIds,
                mode: mode,
                createdBy: requesterId,
                createdAtUtc: UtcTimestamp.Now(),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(requesterId));

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<CreateHazmatShipmentResult>.Success(new CreateHazmatShipmentResult(
                aggregate.Id.Value,
                aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create HazmatShipment for Tenant {TenantId}", request.TenantId);

            return Result<CreateHazmatShipmentResult>.Failure(new ApplicationError(
                "Compliance.PHMSA.ShippingPapers.CreateFailed",
                "Unable to create the hazmat shipment record at this time.",
                Guid.NewGuid()));
        }
    }
}
