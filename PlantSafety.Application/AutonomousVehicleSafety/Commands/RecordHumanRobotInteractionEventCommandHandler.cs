// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for RecordHumanRobotInteractionEventCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.AutonomousVehicleSafety.HumanRobotInteractionEvent.Interfaces;
using PlantSafety.Domain.AutonomousVehicleSafety.HumanRobotInteractionEvent.Enums;
using Agg = PlantSafety.Domain.AutonomousVehicleSafety.HumanRobotInteractionEvent.Aggregates.HumanRobotInteractionEvent;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.AutonomousVehicleSafety.Commands;

/// <summary>Handles <see cref="RecordHumanRobotInteractionEventCommand"/>.</summary>
public sealed class RecordHumanRobotInteractionEventCommandHandler : IRequestHandler<RecordHumanRobotInteractionEventCommand, Result<HumanRobotInteractionEventActionResult>>
{
    private readonly IHumanRobotInteractionEventRepository _repository;
    private readonly ILogger<RecordHumanRobotInteractionEventCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RecordHumanRobotInteractionEventCommandHandler(IHumanRobotInteractionEventRepository repository, ILogger<RecordHumanRobotInteractionEventCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<HumanRobotInteractionEventActionResult>> Handle(RecordHumanRobotInteractionEventCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            if (!Enum.TryParse<HumanRobotInteractionType>(request.InteractionType, ignoreCase: true, out var interactionType))
                interactionType = HumanRobotInteractionType.ProximityAlert;

            if (!Enum.TryParse<InteractionSeverityLevel>(request.SeverityLevel, ignoreCase: true, out var severityLevel))
                severityLevel = InteractionSeverityLevel.Low;

            var aggregate = Agg.Create(
                createdBy: performedBy,
                createdAtUtc: UtcTimestamp.From(request.RequestedAtUtc),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(performedBy),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                autonomousVehicleAssetId: request.AutonomousVehicleAssetId,
                interactionType: interactionType,
                severityLevel: severityLevel,
                distanceMeters: request.DistanceMeters,
                locationDescription: request.LocationDescription);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<HumanRobotInteractionEventActionResult>.Success(new HumanRobotInteractionEventActionResult(
                aggregate.HumanRobotInteractionEventId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record HumanRobotInteractionEvent for Tenant {TenantId}", request.TenantId);

            return Result<HumanRobotInteractionEventActionResult>.Failure(new ApplicationError(
                "AutonomousVehicleSafety.HumanRobotInteractionEvent.RecordFailed",
                "Unable to record the human-robot interaction event at this time.",
                Guid.NewGuid()));
        }
    }
}
