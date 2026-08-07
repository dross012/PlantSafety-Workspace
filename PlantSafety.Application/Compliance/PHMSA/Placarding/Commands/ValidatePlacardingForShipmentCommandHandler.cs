// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Placarding
//  Layer:          Application
//  File:           ValidatePlacardingForShipmentCommandHandler.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Handler for ValidatePlacardingForShipmentCommand. Loads the shipment via the
//                  ShippingPapers Domain repository (cross-module read dependency, expected and
//                  supported), resolves each referenced material's hazard class via the
//                  Classification module's Application-layer read repository, re-runs
//                  PlacardingEngine, and compares the freshly computed placard set against the
//                  shipment's persisted PlacardingRequirements.
//
//                  KNOWN GAP: PlantSafety.Application.Compliance.PHMSA.Classification.Repositories.
//                  IHazmatMaterialRepository / .Models.HazmatMaterialDto are owned by a parallel
//                  build effort and were not present in this repository at the time this file was
//                  written. This handler references them by their documented expected
//                  namespace/shape (mirroring [Compliance].[PHMSA_HazmatMaterial]'s column set) --
//                  it will not compile until that parallel effort lands those two types.
//
//                  QUANTITY SIMPLIFICATION: HazmatShipment tracks only MaterialIds (no per-material
//                  quantity/unit), so this handler evaluates placarding using a nominal 1-unit
//                  quantity per material -- sufficient to determine Table 1 placard applicability,
//                  but Table 2 aggregate-weight-threshold results here may differ from a true
//                  quantity-aware evaluation. Callers that need weight-aware validation should use
//                  CalculatePlacardsCommand directly with real quantities.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Domain.Compliance.PHMSA.Classification.ValueObjects;
using PlantSafety.Domain.Compliance.PHMSA.Placarding.Services;
using PlantSafety.Domain.Compliance.PHMSA.ShippingPapers.Interfaces;
using PlantSafety.Domain.Compliance.PHMSA.ShippingPapers.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.PHMSA.Classification.Models;
using PlantSafety.Application.Compliance.PHMSA.Classification.Repositories;

namespace PlantSafety.Application.Compliance.PHMSA.Placarding.Commands;

/// <summary>Handles <see cref="ValidatePlacardingForShipmentCommand"/>.</summary>
public sealed class ValidatePlacardingForShipmentCommandHandler
    : IRequestHandler<ValidatePlacardingForShipmentCommand, Result<ValidatePlacardingForShipmentResult>>
{
    private readonly IHazmatShipmentRepository _shipmentRepository;
    private readonly IHazmatMaterialRepository _materialRepository;
    private readonly PlacardingEngine _placardingEngine;
    private readonly ILogger<ValidatePlacardingForShipmentCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ValidatePlacardingForShipmentCommandHandler(
        IHazmatShipmentRepository shipmentRepository,
        IHazmatMaterialRepository materialRepository,
        PlacardingEngine placardingEngine,
        ILogger<ValidatePlacardingForShipmentCommandHandler> logger)
    {
        _shipmentRepository = shipmentRepository;
        _materialRepository = materialRepository;
        _placardingEngine = placardingEngine;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ValidatePlacardingForShipmentResult>> Handle(ValidatePlacardingForShipmentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var shipmentId = HazmatShipmentId.FromGuid(request.ShipmentId);
            var shipment = await _shipmentRepository.GetByIdAsync(shipmentId, request.TenantId, cancellationToken);
            if (shipment is null)
            {
                return Result<ValidatePlacardingForShipmentResult>.Failure(new ApplicationError(
                    "Compliance.PHMSA.Placarding.ShipmentNotFound",
                    "The requested hazmat shipment could not be found.",
                    request.ShipmentId,
                    ErrorSeverity.NotFound));
            }

            var materials = new List<(HazardClass HazardClass, decimal Quantity, string QuantityUnit)>();
            foreach (var materialId in shipment.MaterialIds)
            {
                HazmatMaterialDto? material = await _materialRepository.GetByIdAsync(materialId.Value, request.TenantId, cancellationToken);
                if (material is null || !HazardClassParser.TryParse(material.HazardClass, out var hazardClass))
                    continue;

                materials.Add((hazardClass, 1m, "unit"));
            }

            var computed = _placardingEngine.GeneratePlacards(new PlacardingInput(materials, request.IsBulkPackaging));

            var persisted = new HashSet<string>(shipment.PlacardingRequirements, StringComparer.OrdinalIgnoreCase);
            var computedSet = new HashSet<string>(computed.RequiredPlacards, StringComparer.OrdinalIgnoreCase);

            var discrepancies = new List<string>();
            foreach (var missing in computedSet.Except(persisted, StringComparer.OrdinalIgnoreCase))
                discrepancies.Add($"Placard '{missing}' is required by the current load but is not recorded on the shipment.");
            foreach (var extra in persisted.Except(computedSet, StringComparer.OrdinalIgnoreCase))
                discrepancies.Add($"Placard '{extra}' is recorded on the shipment but is not required by the current load.");

            return Result<ValidatePlacardingForShipmentResult>.Success(new ValidatePlacardingForShipmentResult(
                request.ShipmentId,
                discrepancies.Count == 0,
                shipment.PlacardingRequirements,
                computed.RequiredPlacards,
                discrepancies));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate placarding for HazmatShipment {ShipmentId}", request.ShipmentId);

            return Result<ValidatePlacardingForShipmentResult>.Failure(new ApplicationError(
                "Compliance.PHMSA.Placarding.ValidationFailed",
                "Unable to validate placarding for the shipment at this time.",
                Guid.NewGuid()));
        }
    }
}
