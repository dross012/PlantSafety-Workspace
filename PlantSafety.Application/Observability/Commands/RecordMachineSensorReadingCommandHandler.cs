// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handler for RecordMachineSensorReadingCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Observability.MachineProcessMonitoring.Interfaces;
using PlantSafety.Domain.Observability.MachineProcessMonitoring.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Observability.Commands;

/// <summary>Handles <see cref="RecordMachineSensorReadingCommand"/>.</summary>
public sealed class RecordMachineSensorReadingCommandHandler : IRequestHandler<RecordMachineSensorReadingCommand, Result<MachineProcessMonitoringActionResult>>
{
    private readonly IMachineProcessMonitoringRepository _repository;
    private readonly ILogger<RecordMachineSensorReadingCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RecordMachineSensorReadingCommandHandler(IMachineProcessMonitoringRepository repository, ILogger<RecordMachineSensorReadingCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<MachineProcessMonitoringActionResult>> Handle(RecordMachineSensorReadingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = MachineProcessMonitoringId.FromGuid(request.MachineProcessMonitoringId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
            {
                return Result<MachineProcessMonitoringActionResult>.Failure(new ApplicationError(
                    "Observability.MachineProcessMonitoring.NotFound",
                    "The requested machine process monitoring channel could not be found.",
                    Guid.NewGuid()));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.RecordSensorReading(
                reading: request.Reading,
                thresholdMin: request.ThresholdMin,
                thresholdMax: request.ThresholdMax,
                recordedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<MachineProcessMonitoringActionResult>.Success(new MachineProcessMonitoringActionResult(
                aggregate.MachineProcessMonitoringId.Value,
                aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record sensor reading for MachineProcessMonitoring {MachineProcessMonitoringId}", request.MachineProcessMonitoringId);

            return Result<MachineProcessMonitoringActionResult>.Failure(new ApplicationError(
                "Observability.MachineProcessMonitoring.RecordSensorReadingFailed",
                "Unable to record the sensor reading at this time.",
                Guid.NewGuid()));
        }
    }
}
