// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for RecordOccupancySnapshotCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationZones.Enums;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationZones.Interfaces;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationZones.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.DigitalTwin.EvacuationModeling.Commands;

/// <summary>Handles <see cref="RecordOccupancySnapshotCommand"/>.</summary>
public sealed class RecordOccupancySnapshotCommandHandler : IRequestHandler<RecordOccupancySnapshotCommand, Result<EvacuationZoneActionResult>>
{
    private readonly IEvacuationZoneRepository _repository;
    private readonly ILogger<RecordOccupancySnapshotCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RecordOccupancySnapshotCommandHandler(IEvacuationZoneRepository repository, ILogger<RecordOccupancySnapshotCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<EvacuationZoneActionResult>> Handle(RecordOccupancySnapshotCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = EvacuationZoneId.FromGuid(request.EvacuationZoneId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
            {
                return Result<EvacuationZoneActionResult>.Failure(new ApplicationError(
                    "DigitalTwin.EvacuationModeling.EvacuationZone.NotFound",
                    "The requested evacuation zone could not be found.",
                    Guid.NewGuid()));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);
            var source = Enum.TryParse<OccupancyReadingSource>(request.Source, ignoreCase: true, out var parsed)
                ? parsed
                : OccupancyReadingSource.Manual;

            aggregate.RecordOccupancySnapshot(
                OccupancyReadingId.New(), request.HeadcountEstimate, source,
                UtcTimestamp.From(request.RequestedAtUtc), PerformedByActor.User(performedBy),
                new CorrelationId(request.CorrelationId), new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<EvacuationZoneActionResult>.Success(new EvacuationZoneActionResult(
                aggregate.EvacuationZoneId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record occupancy snapshot for EvacuationZone {EvacuationZoneId}", request.EvacuationZoneId);

            return Result<EvacuationZoneActionResult>.Failure(new ApplicationError(
                "DigitalTwin.EvacuationModeling.EvacuationZone.RecordOccupancyFailed",
                "Unable to record the occupancy snapshot at this time.",
                Guid.NewGuid()));
        }
    }
}
