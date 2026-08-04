// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for GetEvacuationRouteByIdQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.DigitalTwin.EvacuationModeling.EvacuationRoutes.Models;
using PlantSafety.Application.DigitalTwin.EvacuationModeling.EvacuationRoutes.Queries;
using PlantSafety.Application.DigitalTwin.EvacuationModeling.EvacuationRoutes.Repositories;

namespace PlantSafety.Application.DigitalTwin.EvacuationModeling.EvacuationRoutes.Handlers;

/// <summary>Handles <see cref="GetEvacuationRouteByIdQuery"/>.</summary>
public sealed class GetEvacuationRouteByIdQueryHandler : IRequestHandler<GetEvacuationRouteByIdQuery, Result<EvacuationRouteItemDto?>>
{
    private readonly IEvacuationRouteRepository _repository;
    private readonly ILogger<GetEvacuationRouteByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetEvacuationRouteByIdQueryHandler(IEvacuationRouteRepository repository, ILogger<GetEvacuationRouteByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<EvacuationRouteItemDto?>> Handle(GetEvacuationRouteByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _repository.GetByIdAsync(request.TenantId, request.EvacuationRouteId, cancellationToken);
            return Result<EvacuationRouteItemDto?>.Success(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve EvacuationRoute {EvacuationRouteId}", request.EvacuationRouteId);

            return Result<EvacuationRouteItemDto?>.Failure(new ApplicationError(
                "DigitalTwin.EvacuationModeling.EvacuationRoute.RetrievalFailed",
                "Unable to retrieve the evacuation route at this time.",
                Guid.NewGuid()));
        }
    }
}
