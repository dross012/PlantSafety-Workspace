// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handler for ScheduleNextCycleHazardCommunicationTrainingCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.EmployeeSafety.HazardCommunicationTraining.Interfaces;
using PlantSafety.Domain.EmployeeSafety.HazardCommunicationTraining.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.EmployeeSafety.Commands;

/// <summary>Handles <see cref="ScheduleNextCycleHazardCommunicationTrainingCommand"/>.</summary>
public sealed class ScheduleNextCycleHazardCommunicationTrainingCommandHandler : IRequestHandler<ScheduleNextCycleHazardCommunicationTrainingCommand, Result<HazardCommunicationTrainingActionResult>>
{
    private readonly IHazardCommunicationTrainingRepository _repository;
    private readonly ILogger<ScheduleNextCycleHazardCommunicationTrainingCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ScheduleNextCycleHazardCommunicationTrainingCommandHandler(IHazardCommunicationTrainingRepository repository, ILogger<ScheduleNextCycleHazardCommunicationTrainingCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<HazardCommunicationTrainingActionResult>> Handle(ScheduleNextCycleHazardCommunicationTrainingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = HazardCommunicationTrainingId.FromGuid(request.HazardCommunicationTrainingId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
            {
                return Result<HazardCommunicationTrainingActionResult>.Failure(new ApplicationError(
                    "EmployeeSafety.HazardCommunicationTraining.NotFound",
                    "The requested HazardCommunicationTraining record could not be found.",
                    Guid.NewGuid()));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.ScheduleNextCycle(
                UtcTimestamp.From(request.DueAtUtc),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<HazardCommunicationTrainingActionResult>.Success(new HazardCommunicationTrainingActionResult(
                aggregate.HazardCommunicationTrainingId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute ScheduleNextCycle for HazardCommunicationTraining {HazardCommunicationTrainingId}", request.HazardCommunicationTrainingId);

            return Result<HazardCommunicationTrainingActionResult>.Failure(new ApplicationError(
                "EmployeeSafety.HazardCommunicationTraining.ScheduleNextCycleFailed",
                "Unable to ScheduleNextCycle the HazardCommunicationTraining record at this time.",
                Guid.NewGuid()));
        }
    }
}