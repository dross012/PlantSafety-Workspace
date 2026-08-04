// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for ListEvacuationRoutesByZoneQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.DigitalTwin.EvacuationModeling.EvacuationRoutes.Models;
using PlantSafety.Application.DigitalTwin.EvacuationModeling.EvacuationRoutes.Queries;
using PlantSafety.Application.DigitalTwin.EvacuationModeling.EvacuationRoutes.Repositories;

namespace PlantSafety.Application.DigitalTwin.EvacuationModeling.EvacuationRoutes.Handlers;

/// <summary>Handles <see cref="ListEvacuationRoutesByZoneQuery"/>.</summary>
public sealed class ListEvacuationRoutesByZoneQueryHandler : IRequestHandler<ListEvacuationRoutesByZoneQuery, Result<IReadOnlyList<EvacuationRouteItemDto>>>
{
    private readonly IEvacuationRouteRepository _repository;
    private readonly ILogger<ListEvacuationRoutesByZoneQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ListEvacuationRoutesByZoneQueryHandler(IEvacuationRouteRepository repository, ILogger<ListEvacuationRoutesByZoneQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<EvacuationRouteItemDto>>> Handle(ListEvacuationRoutesByZoneQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.ListByZoneAsync(request.TenantId, request.ZoneId, cancellationToken);
            return Result<IReadOnlyList<EvacuationRouteItemDto>>.Success(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list EvacuationRoutes for Zone {ZoneId}", request.ZoneId);

            return Result<IReadOnlyList<EvacuationRouteItemDto>>.Failure(new ApplicationError(
                "DigitalTwin.EvacuationModeling.EvacuationRoute.ListByZoneFailed",
                "Unable to list evacuation routes for the requested zone at this time.",
                Guid.NewGuid()));
        }
    }
}
