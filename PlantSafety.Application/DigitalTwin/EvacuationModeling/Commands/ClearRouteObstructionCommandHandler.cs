// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for ClearRouteObstructionCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationRoutes.Interfaces;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationRoutes.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.DigitalTwin.EvacuationModeling.Commands;

/// <summary>Handles <see cref="ClearRouteObstructionCommand"/>.</summary>
public sealed class ClearRouteObstructionCommandHandler : IRequestHandler<ClearRouteObstructionCommand, Result<EvacuationRouteActionResult>>
{
    private readonly IEvacuationRouteRepository _repository;
    private readonly ILogger<ClearRouteObstructionCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ClearRouteObstructionCommandHandler(IEvacuationRouteRepository repository, ILogger<ClearRouteObstructionCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<EvacuationRouteActionResult>> Handle(ClearRouteObstructionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = EvacuationRouteId.FromGuid(request.EvacuationRouteId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
            {
                return Result<EvacuationRouteActionResult>.Failure(new ApplicationError(
                    "DigitalTwin.EvacuationModeling.EvacuationRoute.NotFound",
                    "The requested evacuation route could not be found.",
                    Guid.NewGuid()));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.ClearObstruction(
                RouteObstructionId.FromGuid(request.RouteObstructionId),
                UtcTimestamp.From(request.RequestedAtUtc), PerformedByActor.User(performedBy),
                new CorrelationId(request.CorrelationId), new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<EvacuationRouteActionResult>.Success(new EvacuationRouteActionResult(
                aggregate.EvacuationRouteId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clear obstruction for EvacuationRoute {EvacuationRouteId}", request.EvacuationRouteId);

            return Result<EvacuationRouteActionResult>.Failure(new ApplicationError(
                "DigitalTwin.EvacuationModeling.EvacuationRoute.ClearObstructionFailed",
                "Unable to clear the route obstruction at this time.",
                Guid.NewGuid()));
        }
    }
}
