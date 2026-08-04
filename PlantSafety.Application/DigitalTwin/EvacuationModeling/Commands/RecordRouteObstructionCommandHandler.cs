// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for RecordRouteObstructionCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationRoutes.Enums;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationRoutes.Interfaces;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationRoutes.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.DigitalTwin.EvacuationModeling.Commands;

/// <summary>Handles <see cref="RecordRouteObstructionCommand"/>.</summary>
public sealed class RecordRouteObstructionCommandHandler : IRequestHandler<RecordRouteObstructionCommand, Result<EvacuationRouteActionResult>>
{
    private readonly IEvacuationRouteRepository _repository;
    private readonly ILogger<RecordRouteObstructionCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RecordRouteObstructionCommandHandler(IEvacuationRouteRepository repository, ILogger<RecordRouteObstructionCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<EvacuationRouteActionResult>> Handle(RecordRouteObstructionCommand request, CancellationToken cancellationToken)
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
            var severity = Enum.TryParse<ObstructionSeverity>(request.Severity, ignoreCase: true, out var parsed)
                ? parsed
                : ObstructionSeverity.Moderate;

            aggregate.RecordObstruction(
                RouteObstructionId.New(), request.Description, severity,
                UtcTimestamp.From(request.RequestedAtUtc), PerformedByActor.User(performedBy),
                new CorrelationId(request.CorrelationId), new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<EvacuationRouteActionResult>.Success(new EvacuationRouteActionResult(
                aggregate.EvacuationRouteId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record obstruction for EvacuationRoute {EvacuationRouteId}", request.EvacuationRouteId);

            return Result<EvacuationRouteActionResult>.Failure(new ApplicationError(
                "DigitalTwin.EvacuationModeling.EvacuationRoute.RecordObstructionFailed",
                "Unable to record the route obstruction at this time.",
                Guid.NewGuid()));
        }
    }
}
