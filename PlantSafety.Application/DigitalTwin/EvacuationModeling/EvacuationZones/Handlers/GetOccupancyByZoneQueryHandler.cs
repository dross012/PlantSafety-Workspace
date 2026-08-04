// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for GetOccupancyByZoneQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.DigitalTwin.EvacuationModeling.EvacuationZones.Models;
using PlantSafety.Application.DigitalTwin.EvacuationModeling.EvacuationZones.Queries;
using PlantSafety.Application.DigitalTwin.EvacuationModeling.EvacuationZones.Repositories;

namespace PlantSafety.Application.DigitalTwin.EvacuationModeling.EvacuationZones.Handlers;

/// <summary>Handles <see cref="GetOccupancyByZoneQuery"/>.</summary>
public sealed class GetOccupancyByZoneQueryHandler : IRequestHandler<GetOccupancyByZoneQuery, Result<IReadOnlyList<OccupancyReadingItemDto>>>
{
    private readonly IEvacuationZoneRepository _repository;
    private readonly ILogger<GetOccupancyByZoneQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetOccupancyByZoneQueryHandler(IEvacuationZoneRepository repository, ILogger<GetOccupancyByZoneQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<OccupancyReadingItemDto>>> Handle(GetOccupancyByZoneQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetOccupancyByZoneAsync(request.TenantId, request.SiteId, request.ZoneId, request.From, request.To, cancellationToken);
            return Result<IReadOnlyList<OccupancyReadingItemDto>>.Success(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve occupancy readings for Zone {ZoneId}", request.ZoneId);

            return Result<IReadOnlyList<OccupancyReadingItemDto>>.Failure(new ApplicationError(
                "DigitalTwin.EvacuationModeling.EvacuationZone.OccupancyRetrievalFailed",
                "Unable to retrieve occupancy readings at this time.",
                Guid.NewGuid()));
        }
    }
}
