// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.ShippingPapers
//  Layer:          Application
//  File:           GetHazmatShipmentQueryHandler.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Handler for GetHazmatShipmentQuery. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Handlers.GetPermitsByIdQueryHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.PHMSA.ShippingPapers.Models;
using PlantSafety.Application.Compliance.PHMSA.ShippingPapers.Queries;
using PlantSafety.Application.Compliance.PHMSA.ShippingPapers.Repositories;

namespace PlantSafety.Application.Compliance.PHMSA.ShippingPapers.Handlers;

/// <summary>Handles <see cref="GetHazmatShipmentQuery"/>.</summary>
public sealed class GetHazmatShipmentQueryHandler : IRequestHandler<GetHazmatShipmentQuery, Result<HazmatShipmentDto>>
{
    private readonly IHazmatShipmentRepository _repository;
    private readonly ILogger<GetHazmatShipmentQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetHazmatShipmentQueryHandler(IHazmatShipmentRepository repository, ILogger<GetHazmatShipmentQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<HazmatShipmentDto>> Handle(GetHazmatShipmentQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _repository.GetByIdAsync(request.HazmatShipmentId, request.TenantId, cancellationToken);

            if (item is null || item.TenantId != request.TenantId)
            {
                _logger.LogDebug("HazmatShipment {HazmatShipmentId} not found for Tenant {TenantId}", request.HazmatShipmentId, request.TenantId);

                return Result<HazmatShipmentDto>.Failure(new ApplicationError(
                    "Compliance.PHMSA.ShippingPapers.NotFound",
                    "The requested hazmat shipment could not be found.",
                    Guid.NewGuid(),
                    ErrorSeverity.NotFound));
            }

            return Result<HazmatShipmentDto>.Success(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve HazmatShipment {HazmatShipmentId}", request.HazmatShipmentId);

            return Result<HazmatShipmentDto>.Failure(new ApplicationError(
                "Compliance.PHMSA.ShippingPapers.RetrievalFailed",
                "Unable to retrieve hazmat shipment data at this time.",
                Guid.NewGuid()));
        }
    }
}
