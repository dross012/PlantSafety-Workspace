// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for RecordAutonomyModeTransitionCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.AutonomousVehicleSafety.AutonomousVehicleAsset.Interfaces;
using PlantSafety.Domain.AutonomousVehicleSafety.AutonomousVehicleAsset.Enums;
using PlantSafety.Domain.AutonomousVehicleSafety.AutonomousVehicleAsset.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.AutonomousVehicleSafety.Commands;

/// <summary>Handles <see cref="RecordAutonomyModeTransitionCommand"/>.</summary>
public sealed class RecordAutonomyModeTransitionCommandHandler : IRequestHandler<RecordAutonomyModeTransitionCommand, Result<AutonomousVehicleAssetActionResult>>
{
    private readonly IAutonomousVehicleAssetRepository _repository;
    private readonly ILogger<RecordAutonomyModeTransitionCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RecordAutonomyModeTransitionCommandHandler(IAutonomousVehicleAssetRepository repository, ILogger<RecordAutonomyModeTransitionCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<AutonomousVehicleAssetActionResult>> Handle(RecordAutonomyModeTransitionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = AutonomousVehicleAssetId.FromGuid(request.AutonomousVehicleAssetId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
            {
                return Result<AutonomousVehicleAssetActionResult>.Failure(new ApplicationError(
                    "AutonomousVehicleSafety.AutonomousVehicleAsset.NotFound",
                    "The requested AutonomousVehicleAsset record could not be found.",
                    Guid.NewGuid()));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            if (!Enum.TryParse<AutonomyMode>(request.ToMode, ignoreCase: true, out var toMode))
                toMode = AutonomyMode.Autonomous;

            aggregate.RecordModeTransition(
                toMode,
                UtcTimestamp.From(request.RequestedAtUtc),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<AutonomousVehicleAssetActionResult>.Success(new AutonomousVehicleAssetActionResult(
                aggregate.AutonomousVehicleAssetId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record autonomy-mode transition for AutonomousVehicleAsset {AutonomousVehicleAssetId}", request.AutonomousVehicleAssetId);

            return Result<AutonomousVehicleAssetActionResult>.Failure(new ApplicationError(
                "AutonomousVehicleSafety.AutonomousVehicleAsset.RecordModeTransitionFailed",
                "Unable to record the autonomy-mode transition at this time.",
                Guid.NewGuid()));
        }
    }
}
