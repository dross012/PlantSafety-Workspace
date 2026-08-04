// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for GetZoneOccupancyFromAccessEventsQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.PhysicalAccessControl.SafetyZones.Queries;
using PlantSafety.Application.PhysicalAccessControl.SafetyZones.Repositories;

namespace PlantSafety.Application.PhysicalAccessControl.SafetyZones.Handlers;

/// <summary>Handles <see cref="GetZoneOccupancyFromAccessEventsQuery"/>.</summary>
public sealed class GetZoneOccupancyFromAccessEventsQueryHandler : IRequestHandler<GetZoneOccupancyFromAccessEventsQuery, Result<GetZoneOccupancyFromAccessEventsResult>>
{
    private readonly ISafetyZoneReadRepository _repository;
    private readonly ILogger<GetZoneOccupancyFromAccessEventsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetZoneOccupancyFromAccessEventsQueryHandler(ISafetyZoneReadRepository repository, ILogger<GetZoneOccupancyFromAccessEventsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<GetZoneOccupancyFromAccessEventsResult>> Handle(GetZoneOccupancyFromAccessEventsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var occupancy = await _repository.GetOccupancyAsync(request.TenantId, request.SafetyZoneId, cancellationToken);

            return Result<GetZoneOccupancyFromAccessEventsResult>.Success(new GetZoneOccupancyFromAccessEventsResult(occupancy));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve occupancy for SafetyZone {SafetyZoneId}", request.SafetyZoneId);

            return Result<GetZoneOccupancyFromAccessEventsResult>.Failure(new ApplicationError(
                "PhysicalAccessControl.SafetyZone.RetrievalFailed",
                "Unable to retrieve SafetyZone occupancy at this time.",
                Guid.NewGuid()));
        }
    }
}
