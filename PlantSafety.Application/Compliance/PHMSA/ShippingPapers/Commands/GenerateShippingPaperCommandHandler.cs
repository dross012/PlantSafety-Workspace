// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.ShippingPapers
//  Layer:          Application
//  File:           GenerateShippingPaperCommandHandler.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Handler for GenerateShippingPaperCommand. Loads the shipment, resolves its
//                  materials via ShippingPaperMaterialResolver, calls
//                  ShippingPaperGenerator.BuildShippingPaper(...), then calls
//                  aggregate.AttachShippingPaper(...). See ShippingPaperMaterialResolver's header
//                  for the parallel-effort dependency this handler carries (KNOWN GAP).
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Compliance.PHMSA.ShippingPapers.Interfaces;
using PlantSafety.Domain.Compliance.PHMSA.ShippingPapers.Services;
using PlantSafety.Domain.Compliance.PHMSA.ShippingPapers.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.PHMSA.Classification.Repositories;

namespace PlantSafety.Application.Compliance.PHMSA.ShippingPapers.Commands;

/// <summary>Handles <see cref="GenerateShippingPaperCommand"/>.</summary>
public sealed class GenerateShippingPaperCommandHandler : IRequestHandler<GenerateShippingPaperCommand, Result<HazmatShipmentStatusResult>>
{
    private readonly IHazmatShipmentRepository _shipmentRepository;
    private readonly IHazmatMaterialRepository _materialRepository;
    private readonly ShippingPaperGenerator _shippingPaperGenerator;
    private readonly ILogger<GenerateShippingPaperCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GenerateShippingPaperCommandHandler(
        IHazmatShipmentRepository shipmentRepository,
        IHazmatMaterialRepository materialRepository,
        ShippingPaperGenerator shippingPaperGenerator,
        ILogger<GenerateShippingPaperCommandHandler> logger)
    {
        _shipmentRepository = shipmentRepository;
        _materialRepository = materialRepository;
        _shippingPaperGenerator = shippingPaperGenerator;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<HazmatShipmentStatusResult>> Handle(GenerateShippingPaperCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = HazmatShipmentId.FromGuid(request.HazmatShipmentId);
            var aggregate = await _shipmentRepository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<HazmatShipmentStatusResult>.Failure(new ApplicationError(
                    "Compliance.PHMSA.ShippingPapers.NotFound",
                    "The requested hazmat shipment could not be found.",
                    request.HazmatShipmentId,
                    ErrorSeverity.NotFound));
            }

            var materials = await ShippingPaperMaterialResolver.ResolveAsync(_materialRepository, aggregate.MaterialIds, request.TenantId, cancellationToken);
            if (materials.Count == 0)
            {
                return Result<HazmatShipmentStatusResult>.Failure(new ApplicationError(
                    "Compliance.PHMSA.ShippingPapers.NoResolvableMaterials",
                    "None of the shipment's referenced materials could be resolved for shipping paper generation.",
                    Guid.NewGuid(),
                    ErrorSeverity.Validation));
            }

            var generated = _shippingPaperGenerator.BuildShippingPaper(new ShippingPaperInput(
                materials,
                aggregate.Mode,
                request.WasteManifestReference,
                request.ShipperCertificationText ?? string.Empty));

            if (!generated.Success || generated.ShippingPaper is null)
            {
                return Result<HazmatShipmentStatusResult>.Failure(new ApplicationError(
                    "Compliance.PHMSA.ShippingPapers.GenerationFailed",
                    string.Join(" ", generated.Messages),
                    Guid.NewGuid(),
                    ErrorSeverity.Validation));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.AttachShippingPaper(
                paper: generated.ShippingPaper,
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(performedBy));

            await _shipmentRepository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            return Result<HazmatShipmentStatusResult>.Success(new HazmatShipmentStatusResult(
                aggregate.Id.Value,
                aggregate.Status.ToString()));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate shipping paper for HazmatShipment {HazmatShipmentId}", request.HazmatShipmentId);

            return Result<HazmatShipmentStatusResult>.Failure(new ApplicationError(
                "Compliance.PHMSA.ShippingPapers.GenerationFailed",
                "Unable to generate the shipping paper at this time.",
                Guid.NewGuid()));
        }
    }
}
