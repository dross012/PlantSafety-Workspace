// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for GetLatestEgressSimulationQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.DigitalTwin.EvacuationModeling.EgressSimulations.Models;
using PlantSafety.Application.DigitalTwin.EvacuationModeling.EgressSimulations.Queries;
using PlantSafety.Application.DigitalTwin.EvacuationModeling.EgressSimulations.Repositories;

namespace PlantSafety.Application.DigitalTwin.EvacuationModeling.EgressSimulations.Handlers;

/// <summary>Handles <see cref="GetLatestEgressSimulationQuery"/>.</summary>
public sealed class GetLatestEgressSimulationQueryHandler : IRequestHandler<GetLatestEgressSimulationQuery, Result<EgressSimulationItemDto?>>
{
    private readonly IEgressSimulationRepository _repository;
    private readonly ILogger<GetLatestEgressSimulationQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetLatestEgressSimulationQueryHandler(IEgressSimulationRepository repository, ILogger<GetLatestEgressSimulationQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<EgressSimulationItemDto?>> Handle(GetLatestEgressSimulationQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _repository.GetLatestByZoneAsync(request.TenantId, request.SiteId, request.ZoneId, cancellationToken);
            return Result<EgressSimulationItemDto?>.Success(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve latest EgressSimulationRun for Zone {ZoneId}", request.ZoneId);

            return Result<EgressSimulationItemDto?>.Failure(new ApplicationError(
                "DigitalTwin.EvacuationModeling.EgressSimulationRun.RetrievalFailed",
                "Unable to retrieve the latest egress simulation at this time.",
                Guid.NewGuid()));
        }
    }
}
