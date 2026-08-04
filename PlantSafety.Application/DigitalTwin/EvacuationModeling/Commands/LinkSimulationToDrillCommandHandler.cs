// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for LinkSimulationToDrillCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EgressSimulations.Interfaces;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EgressSimulations.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.DigitalTwin.EvacuationModeling.Commands;

/// <summary>Handles <see cref="LinkSimulationToDrillCommand"/>.</summary>
public sealed class LinkSimulationToDrillCommandHandler : IRequestHandler<LinkSimulationToDrillCommand, Result<EgressSimulationActionResult>>
{
    private readonly IEgressSimulationRunRepository _repository;
    private readonly ILogger<LinkSimulationToDrillCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public LinkSimulationToDrillCommandHandler(IEgressSimulationRunRepository repository, ILogger<LinkSimulationToDrillCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<EgressSimulationActionResult>> Handle(LinkSimulationToDrillCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = EgressSimulationRunId.FromGuid(request.EgressSimulationRunId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
            {
                return Result<EgressSimulationActionResult>.Failure(new ApplicationError(
                    "DigitalTwin.EvacuationModeling.EgressSimulationRun.NotFound",
                    "The requested egress-simulation run could not be found.",
                    Guid.NewGuid()));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.LinkToDrill(
                request.DrillId, UtcTimestamp.From(request.RequestedAtUtc), PerformedByActor.User(performedBy),
                new CorrelationId(request.CorrelationId), new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<EgressSimulationActionResult>.Success(new EgressSimulationActionResult(
                aggregate.EgressSimulationRunId.Value, aggregate.Status.ToString(), aggregate.SimulatedEvacuationTimeSeconds));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to link EgressSimulationRun {EgressSimulationRunId} to drill", request.EgressSimulationRunId);

            return Result<EgressSimulationActionResult>.Failure(new ApplicationError(
                "DigitalTwin.EvacuationModeling.EgressSimulationRun.LinkToDrillFailed",
                "Unable to link the simulation run to the drill at this time.",
                Guid.NewGuid()));
        }
    }
}
