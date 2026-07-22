// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handler for RecordCalibrationResultPresenceSensingDevicesCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.MachineSafety.PresenceSensingDevices.Interfaces;
using PlantSafety.Domain.MachineSafety.PresenceSensingDevices.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.MachineSafety.Commands;

/// <summary>Handles <see cref="RecordCalibrationResultPresenceSensingDevicesCommand"/>.</summary>
public sealed class RecordCalibrationResultPresenceSensingDevicesCommandHandler : IRequestHandler<RecordCalibrationResultPresenceSensingDevicesCommand, Result<PresenceSensingDevicesActionResult>>
{
    private readonly IPresenceSensingDevicesRepository _repository;
    private readonly ILogger<RecordCalibrationResultPresenceSensingDevicesCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RecordCalibrationResultPresenceSensingDevicesCommandHandler(IPresenceSensingDevicesRepository repository, ILogger<RecordCalibrationResultPresenceSensingDevicesCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PresenceSensingDevicesActionResult>> Handle(RecordCalibrationResultPresenceSensingDevicesCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = PresenceSensingDevicesId.FromGuid(request.PresenceSensingDevicesId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
            {
                return Result<PresenceSensingDevicesActionResult>.Failure(new ApplicationError(
                    "MachineSafety.PresenceSensingDevices.NotFound",
                    "The requested PresenceSensingDevices record could not be found.",
                    Guid.NewGuid()));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.RecordCalibrationResult(
                request.DetectionAccuracyScore,
                UserId.FromGuid(request.CalibratedByUserId),
                UtcTimestamp.Now(),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<PresenceSensingDevicesActionResult>.Success(new PresenceSensingDevicesActionResult(
                aggregate.PresenceSensingDevicesId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute RecordCalibrationResult for PresenceSensingDevices {PresenceSensingDevicesId}", request.PresenceSensingDevicesId);

            return Result<PresenceSensingDevicesActionResult>.Failure(new ApplicationError(
                "MachineSafety.PresenceSensingDevices.RecordCalibrationResultFailed",
                "Unable to RecordCalibrationResult the PresenceSensingDevices record at this time.",
                Guid.NewGuid()));
        }
    }
}