// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for RunEgressSimulationCommand. Starts and completes a simulation run
//   in a single call (the caller supplies the already-computed simulation result), then invokes
//   EgressSimulationBottleneckAnalysisService against the optionally-supplied route to detect or
//   record recurrence of a Bottleneck Analysis subdomain finding.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EgressSimulations.Enums;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EgressSimulations.Interfaces;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EgressSimulations.Services;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationRoutes.Interfaces;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationRoutes.ValueObjects.Ids;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationBottlenecks.Aggregates;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationBottlenecks.Interfaces;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationZones.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.DigitalTwin.EvacuationModeling.Commands;

/// <summary>Handles <see cref="RunEgressSimulationCommand"/>.</summary>
public sealed class RunEgressSimulationCommandHandler : IRequestHandler<RunEgressSimulationCommand, Result<EgressSimulationActionResult>>
{
    private readonly IEgressSimulationRunRepository _simulationRepository;
    private readonly IEvacuationRouteRepository _routeRepository;
    private readonly IEvacuationBottleneckRepository _bottleneckRepository;
    private readonly EgressSimulationBottleneckAnalysisService _bottleneckAnalysisService;
    private readonly ILogger<RunEgressSimulationCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RunEgressSimulationCommandHandler(
        IEgressSimulationRunRepository simulationRepository,
        IEvacuationRouteRepository routeRepository,
        IEvacuationBottleneckRepository bottleneckRepository,
        EgressSimulationBottleneckAnalysisService bottleneckAnalysisService,
        ILogger<RunEgressSimulationCommandHandler> logger)
    {
        _simulationRepository = simulationRepository;
        _routeRepository = routeRepository;
        _bottleneckRepository = bottleneckRepository;
        _bottleneckAnalysisService = bottleneckAnalysisService;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<EgressSimulationActionResult>> Handle(RunEgressSimulationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var performedBy = UserId.FromGuid(request.PerformedByUserId);
            var now = UtcTimestamp.From(request.RequestedAtUtc);
            var correlationId = new CorrelationId(request.CorrelationId);
            var causationId = new CausationId(request.CausationId);
            var actor = PerformedByActor.User(performedBy);

            var trigger = Enum.TryParse<EgressSimulationTrigger>(request.TriggerReason, ignoreCase: true, out var parsedTrigger)
                ? parsedTrigger
                : EgressSimulationTrigger.Manual;

            var run = Domain.DigitalTwin.EvacuationModeling.EgressSimulations.Aggregates.EgressSimulationRun.Create(
                performedBy, now, correlationId, causationId, actor,
                request.TenantId, request.SiteId, EvacuationZoneId.FromGuid(request.ZoneId), request.TwinModelVersion, trigger);

            run.Complete(request.SimulatedEvacuationTimeSeconds, request.PredictedPeakOccupancy, now, actor, correlationId, causationId);

            await _simulationRepository.AddAsync(run, cancellationToken);

            if (request.RouteId is { } routeIdValue)
            {
                var route = await _routeRepository.GetByIdAsync(EvacuationRouteId.FromGuid(routeIdValue), cancellationToken);
                if (route is not null && _bottleneckAnalysisService.TryDetectBottleneck(run, route.EstimatedEgressTimeSeconds, out var delaySeconds))
                {
                    var existing = await _bottleneckRepository.GetOpenForRouteAsync(route.EvacuationRouteId, cancellationToken);
                    if (existing is null)
                    {
                        var bottleneck = EvacuationBottleneck.Create(
                            performedBy, now, correlationId, causationId, actor,
                            request.TenantId, request.SiteId, route.EvacuationRouteId, run.EgressSimulationRunId,
                            $"Route {route.EvacuationRouteId} - simulated delay {delaySeconds}s over estimate", delaySeconds);

                        await _bottleneckRepository.AddAsync(bottleneck, cancellationToken);
                    }
                    else
                    {
                        existing.RecordRecurrence(run.EgressSimulationRunId, delaySeconds, now, actor, correlationId, causationId);
                        await _bottleneckRepository.UpdateAsync(existing, cancellationToken);
                    }
                }
            }

            return Result<EgressSimulationActionResult>.Success(new EgressSimulationActionResult(
                run.EgressSimulationRunId.Value, run.Status.ToString(), run.SimulatedEvacuationTimeSeconds));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to run egress simulation for Zone {ZoneId}", request.ZoneId);

            return Result<EgressSimulationActionResult>.Failure(new ApplicationError(
                "DigitalTwin.EvacuationModeling.EgressSimulationRun.RunFailed",
                "Unable to run the egress simulation at this time.",
                Guid.NewGuid()));
        }
    }
}
