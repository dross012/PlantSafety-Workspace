// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for DefineEvacuationRouteCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationRoutes.Aggregates;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationRoutes.Interfaces;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationRoutes.ValueObjects.Ids;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationZones.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.DigitalTwin.EvacuationModeling.Commands;

/// <summary>Handles <see cref="DefineEvacuationRouteCommand"/>.</summary>
public sealed class DefineEvacuationRouteCommandHandler : IRequestHandler<DefineEvacuationRouteCommand, Result<EvacuationRouteActionResult>>
{
    private readonly IEvacuationRouteRepository _repository;
    private readonly ILogger<DefineEvacuationRouteCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public DefineEvacuationRouteCommandHandler(IEvacuationRouteRepository repository, ILogger<DefineEvacuationRouteCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<EvacuationRouteActionResult>> Handle(DefineEvacuationRouteCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var performedBy = UserId.FromGuid(request.PerformedByUserId);
            var now = UtcTimestamp.From(request.RequestedAtUtc);

            var aggregate = EvacuationRoute.Create(
                performedBy, now,
                new CorrelationId(request.CorrelationId), new CausationId(request.CausationId), PerformedByActor.User(performedBy),
                request.TenantId, request.SiteId, request.RouteName,
                EvacuationZoneId.FromGuid(request.ZoneId), AssemblyPointId.FromGuid(request.AssemblyPointId),
                request.PathGeometryRef, request.TwinModelVersion, request.EstimatedEgressTimeSeconds);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<EvacuationRouteActionResult>.Success(new EvacuationRouteActionResult(
                aggregate.EvacuationRouteId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to define EvacuationRoute for Tenant {TenantId}", request.TenantId);

            return Result<EvacuationRouteActionResult>.Failure(new ApplicationError(
                "DigitalTwin.EvacuationModeling.EvacuationRoute.DefineFailed",
                "Unable to define the evacuation route at this time.",
                Guid.NewGuid()));
        }
    }
}
