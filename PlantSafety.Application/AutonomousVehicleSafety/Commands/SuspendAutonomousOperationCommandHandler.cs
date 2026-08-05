// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for SuspendAutonomousOperationCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.AutonomousVehicleSafety.AutonomousVehicleAsset.Interfaces;
using PlantSafety.Domain.AutonomousVehicleSafety.AutonomousVehicleAsset.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.AutonomousVehicleSafety.Commands;

/// <summary>Handles <see cref="SuspendAutonomousOperationCommand"/>.</summary>
public sealed class SuspendAutonomousOperationCommandHandler : IRequestHandler<SuspendAutonomousOperationCommand, Result<AutonomousVehicleAssetActionResult>>
{
    private readonly IAutonomousVehicleAssetRepository _repository;
    private readonly ILogger<SuspendAutonomousOperationCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public SuspendAutonomousOperationCommandHandler(IAutonomousVehicleAssetRepository repository, ILogger<SuspendAutonomousOperationCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<AutonomousVehicleAssetActionResult>> Handle(SuspendAutonomousOperationCommand request, CancellationToken cancellationToken)
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
                    Guid.NewGuid(),
                    ErrorSeverity.NotFound));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.Suspend(
                request.Reason,
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
            _logger.LogError(ex, "Failed to suspend autonomous operation for AutonomousVehicleAsset {AutonomousVehicleAssetId}", request.AutonomousVehicleAssetId);

            return Result<AutonomousVehicleAssetActionResult>.Failure(new ApplicationError(
                "AutonomousVehicleSafety.AutonomousVehicleAsset.SuspendFailed",
                "Unable to suspend autonomous operation at this time.",
                Guid.NewGuid()));
        }
    }
}
