// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handler for RecordReadinessEmergencyPreparednessCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.EmployeeSafety.EmergencyPreparedness.Interfaces;
using PlantSafety.Domain.EmployeeSafety.EmergencyPreparedness.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.EmployeeSafety.Commands;

/// <summary>Handles <see cref="RecordReadinessEmergencyPreparednessCommand"/>.</summary>
public sealed class RecordReadinessEmergencyPreparednessCommandHandler : IRequestHandler<RecordReadinessEmergencyPreparednessCommand, Result<EmergencyPreparednessActionResult>>
{
    private readonly IEmergencyPreparednessRepository _repository;
    private readonly ILogger<RecordReadinessEmergencyPreparednessCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RecordReadinessEmergencyPreparednessCommandHandler(IEmergencyPreparednessRepository repository, ILogger<RecordReadinessEmergencyPreparednessCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<EmergencyPreparednessActionResult>> Handle(RecordReadinessEmergencyPreparednessCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = EmergencyPreparednessId.FromGuid(request.EmergencyPreparednessId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
            {
                return Result<EmergencyPreparednessActionResult>.Failure(new ApplicationError(
                    "EmployeeSafety.EmergencyPreparedness.NotFound",
                    "The requested EmergencyPreparedness record could not be found.",
                    Guid.NewGuid()));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.RecordReadiness(
                request.ReadinessScore,
                request.GapNotes,
                UtcTimestamp.Now(),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<EmergencyPreparednessActionResult>.Success(new EmergencyPreparednessActionResult(
                aggregate.EmergencyPreparednessId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute RecordReadiness for EmergencyPreparedness {EmergencyPreparednessId}", request.EmergencyPreparednessId);

            return Result<EmergencyPreparednessActionResult>.Failure(new ApplicationError(
                "EmployeeSafety.EmergencyPreparedness.RecordReadinessFailed",
                "Unable to RecordReadiness the EmergencyPreparedness record at this time.",
                Guid.NewGuid()));
        }
    }
}