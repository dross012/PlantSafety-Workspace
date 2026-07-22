// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handler for RecordMeasurementEnvironmentalHealthMonitoringCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.EmployeeSafety.EnvironmentalHealthMonitoring.Interfaces;
using PlantSafety.Domain.EmployeeSafety.EnvironmentalHealthMonitoring.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.EmployeeSafety.Commands;

/// <summary>Handles <see cref="RecordMeasurementEnvironmentalHealthMonitoringCommand"/>.</summary>
public sealed class RecordMeasurementEnvironmentalHealthMonitoringCommandHandler : IRequestHandler<RecordMeasurementEnvironmentalHealthMonitoringCommand, Result<EnvironmentalHealthMonitoringActionResult>>
{
    private readonly IEnvironmentalHealthMonitoringRepository _repository;
    private readonly ILogger<RecordMeasurementEnvironmentalHealthMonitoringCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RecordMeasurementEnvironmentalHealthMonitoringCommandHandler(IEnvironmentalHealthMonitoringRepository repository, ILogger<RecordMeasurementEnvironmentalHealthMonitoringCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<EnvironmentalHealthMonitoringActionResult>> Handle(RecordMeasurementEnvironmentalHealthMonitoringCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = EnvironmentalHealthMonitoringId.FromGuid(request.EnvironmentalHealthMonitoringId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
            {
                return Result<EnvironmentalHealthMonitoringActionResult>.Failure(new ApplicationError(
                    "EmployeeSafety.EnvironmentalHealthMonitoring.NotFound",
                    "The requested EnvironmentalHealthMonitoring record could not be found.",
                    Guid.NewGuid()));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.RecordMeasurement(
                request.ExposureLevelScore,
                UtcTimestamp.Now(),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<EnvironmentalHealthMonitoringActionResult>.Success(new EnvironmentalHealthMonitoringActionResult(
                aggregate.EnvironmentalHealthMonitoringId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute RecordMeasurement for EnvironmentalHealthMonitoring {EnvironmentalHealthMonitoringId}", request.EnvironmentalHealthMonitoringId);

            return Result<EnvironmentalHealthMonitoringActionResult>.Failure(new ApplicationError(
                "EmployeeSafety.EnvironmentalHealthMonitoring.RecordMeasurementFailed",
                "Unable to RecordMeasurement the EnvironmentalHealthMonitoring record at this time.",
                Guid.NewGuid()));
        }
    }
}