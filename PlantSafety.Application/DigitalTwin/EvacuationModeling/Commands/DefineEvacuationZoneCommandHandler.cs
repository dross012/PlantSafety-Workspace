// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for DefineEvacuationZoneCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationZones.Aggregates;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationZones.Interfaces;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.DigitalTwin.EvacuationModeling.Commands;

/// <summary>Handles <see cref="DefineEvacuationZoneCommand"/>.</summary>
public sealed class DefineEvacuationZoneCommandHandler : IRequestHandler<DefineEvacuationZoneCommand, Result<EvacuationZoneActionResult>>
{
    private readonly IEvacuationZoneRepository _repository;
    private readonly ILogger<DefineEvacuationZoneCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public DefineEvacuationZoneCommandHandler(IEvacuationZoneRepository repository, ILogger<DefineEvacuationZoneCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<EvacuationZoneActionResult>> Handle(DefineEvacuationZoneCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var performedBy = UserId.FromGuid(request.PerformedByUserId);
            var now = UtcTimestamp.From(request.RequestedAtUtc);

            var aggregate = EvacuationZone.Create(
                performedBy, now,
                new CorrelationId(request.CorrelationId), new CausationId(request.CausationId), PerformedByActor.User(performedBy),
                request.TenantId, request.SiteId, request.ZoneName, request.TwinModelVersion,
                request.ZoneCapacity, request.OccupancyThreshold);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<EvacuationZoneActionResult>.Success(new EvacuationZoneActionResult(
                aggregate.EvacuationZoneId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to define EvacuationZone for Tenant {TenantId}", request.TenantId);

            return Result<EvacuationZoneActionResult>.Failure(new ApplicationError(
                "DigitalTwin.EvacuationModeling.EvacuationZone.DefineFailed",
                "Unable to define the evacuation zone at this time.",
                Guid.NewGuid()));
        }
    }
}
