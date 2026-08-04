// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for ListActiveObstructionsQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.DigitalTwin.EvacuationModeling.EvacuationRoutes.Models;
using PlantSafety.Application.DigitalTwin.EvacuationModeling.EvacuationRoutes.Queries;
using PlantSafety.Application.DigitalTwin.EvacuationModeling.EvacuationRoutes.Repositories;

namespace PlantSafety.Application.DigitalTwin.EvacuationModeling.EvacuationRoutes.Handlers;

/// <summary>Handles <see cref="ListActiveObstructionsQuery"/>.</summary>
public sealed class ListActiveObstructionsQueryHandler : IRequestHandler<ListActiveObstructionsQuery, Result<IReadOnlyList<RouteObstructionItemDto>>>
{
    private readonly IEvacuationRouteRepository _repository;
    private readonly ILogger<ListActiveObstructionsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ListActiveObstructionsQueryHandler(IEvacuationRouteRepository repository, ILogger<ListActiveObstructionsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<RouteObstructionItemDto>>> Handle(ListActiveObstructionsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.ListActiveObstructionsAsync(request.TenantId, request.SiteId, cancellationToken);
            return Result<IReadOnlyList<RouteObstructionItemDto>>.Success(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list active route obstructions for Tenant {TenantId}", request.TenantId);

            return Result<IReadOnlyList<RouteObstructionItemDto>>.Failure(new ApplicationError(
                "DigitalTwin.EvacuationModeling.EvacuationRoute.ListActiveObstructionsFailed",
                "Unable to list active route obstructions at this time.",
                Guid.NewGuid()));
        }
    }
}
