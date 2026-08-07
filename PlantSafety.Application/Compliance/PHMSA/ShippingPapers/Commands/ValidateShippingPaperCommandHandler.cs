// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.ShippingPapers
//  Layer:          Application
//  File:           ValidateShippingPaperCommandHandler.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Handler for ValidateShippingPaperCommand. Loads the shipment, resolves its
//                  materials via ShippingPaperMaterialResolver, re-runs
//                  ShippingPaperGenerator.BuildShippingPaper(...), and compares the freshly
//                  computed BasicDescriptionSequence against what is persisted. See
//                  ShippingPaperMaterialResolver's header for the parallel-effort dependency this
//                  handler carries (KNOWN GAP).
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Domain.Compliance.PHMSA.ShippingPapers.Interfaces;
using PlantSafety.Domain.Compliance.PHMSA.ShippingPapers.Services;
using PlantSafety.Domain.Compliance.PHMSA.ShippingPapers.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.PHMSA.Classification.Repositories;

namespace PlantSafety.Application.Compliance.PHMSA.ShippingPapers.Commands;

/// <summary>Handles <see cref="ValidateShippingPaperCommand"/>.</summary>
public sealed class ValidateShippingPaperCommandHandler : IRequestHandler<ValidateShippingPaperCommand, Result<ValidateShippingPaperResult>>
{
    private readonly IHazmatShipmentRepository _shipmentRepository;
    private readonly IHazmatMaterialRepository _materialRepository;
    private readonly ShippingPaperGenerator _shippingPaperGenerator;
    private readonly ILogger<ValidateShippingPaperCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ValidateShippingPaperCommandHandler(
        IHazmatShipmentRepository shipmentRepository,
        IHazmatMaterialRepository materialRepository,
        ShippingPaperGenerator shippingPaperGenerator,
        ILogger<ValidateShippingPaperCommandHandler> logger)
    {
        _shipmentRepository = shipmentRepository;
        _materialRepository = materialRepository;
        _shippingPaperGenerator = shippingPaperGenerator;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ValidateShippingPaperResult>> Handle(ValidateShippingPaperCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = HazmatShipmentId.FromGuid(request.HazmatShipmentId);
            var aggregate = await _shipmentRepository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<ValidateShippingPaperResult>.Failure(new ApplicationError(
                    "Compliance.PHMSA.ShippingPapers.NotFound",
                    "The requested hazmat shipment could not be found.",
                    request.HazmatShipmentId,
                    ErrorSeverity.NotFound));
            }

            var materials = await ShippingPaperMaterialResolver.ResolveAsync(_materialRepository, aggregate.MaterialIds, request.TenantId, cancellationToken);

            var persistedDescription = aggregate.ShippingPaper?.BasicDescriptionSequence;
            var discrepancies = new List<string>();

            if (materials.Count == 0)
            {
                discrepancies.Add("None of the shipment's referenced materials could be resolved for shipping paper validation.");
                return Result<ValidateShippingPaperResult>.Success(new ValidateShippingPaperResult(
                    request.HazmatShipmentId, persistedDescription is null, persistedDescription, null, discrepancies));
            }

            var generated = _shippingPaperGenerator.BuildShippingPaper(new ShippingPaperInput(
                materials,
                aggregate.Mode,
                aggregate.ShippingPaper?.WasteManifestReference,
                aggregate.ShippingPaper?.ShipperCertification ?? string.Empty));

            var computedDescription = generated.ShippingPaper?.BasicDescriptionSequence;

            if (!string.Equals(persistedDescription, computedDescription, StringComparison.Ordinal))
            {
                discrepancies.Add(persistedDescription is null
                    ? "No shipping paper has been generated for this shipment yet."
                    : "The persisted shipping paper's basic description sequence no longer matches the shipment's current materials.");
            }

            return Result<ValidateShippingPaperResult>.Success(new ValidateShippingPaperResult(
                request.HazmatShipmentId,
                discrepancies.Count == 0,
                persistedDescription,
                computedDescription,
                discrepancies));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate shipping paper for HazmatShipment {HazmatShipmentId}", request.HazmatShipmentId);

            return Result<ValidateShippingPaperResult>.Failure(new ApplicationError(
                "Compliance.PHMSA.ShippingPapers.ValidationFailed",
                "Unable to validate the shipping paper for the shipment at this time.",
                Guid.NewGuid()));
        }
    }
}
