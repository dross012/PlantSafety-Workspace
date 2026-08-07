// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.ShippingPapers
//  Layer:          Application
//  File:           ListHazmatShipmentsQueryHandler.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Handler for ListHazmatShipmentsQuery. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Handlers.GetPermitsQueryHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.PHMSA.ShippingPapers.Queries;
using PlantSafety.Application.Compliance.PHMSA.ShippingPapers.Repositories;

namespace PlantSafety.Application.Compliance.PHMSA.ShippingPapers.Handlers;

/// <summary>Handles <see cref="ListHazmatShipmentsQuery"/>.</summary>
public sealed class ListHazmatShipmentsQueryHandler : IRequestHandler<ListHazmatShipmentsQuery, Result<ListHazmatShipmentsResult>>
{
    private readonly IHazmatShipmentRepository _repository;
    private readonly ILogger<ListHazmatShipmentsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ListHazmatShipmentsQueryHandler(IHazmatShipmentRepository repository, ILogger<ListHazmatShipmentsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ListHazmatShipmentsResult>> Handle(ListHazmatShipmentsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.ListAsync(request.TenantId, request.SiteId, request.Status, request.Mode, cancellationToken);

            return Result<ListHazmatShipmentsResult>.Success(new ListHazmatShipmentsResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list HazmatShipments for Tenant {TenantId}", request.TenantId);

            return Result<ListHazmatShipmentsResult>.Failure(new ApplicationError(
                "Compliance.PHMSA.ShippingPapers.RetrievalFailed",
                "Unable to retrieve hazmat shipment data at this time.",
                Guid.NewGuid()));
        }
    }
}
