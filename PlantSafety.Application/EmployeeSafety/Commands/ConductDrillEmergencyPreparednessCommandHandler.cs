// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handler for ConductDrillEmergencyPreparednessCommand.
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

/// <summary>Handles <see cref="ConductDrillEmergencyPreparednessCommand"/>.</summary>
public sealed class ConductDrillEmergencyPreparednessCommandHandler : IRequestHandler<ConductDrillEmergencyPreparednessCommand, Result<EmergencyPreparednessActionResult>>
{
    private readonly IEmergencyPreparednessRepository _repository;
    private readonly ILogger<ConductDrillEmergencyPreparednessCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ConductDrillEmergencyPreparednessCommandHandler(IEmergencyPreparednessRepository repository, ILogger<ConductDrillEmergencyPreparednessCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<EmergencyPreparednessActionResult>> Handle(ConductDrillEmergencyPreparednessCommand request, CancellationToken cancellationToken)
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

            aggregate.ConductDrill(
                request.ParticipantCount,
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
            _logger.LogError(ex, "Failed to execute ConductDrill for EmergencyPreparedness {EmergencyPreparednessId}", request.EmergencyPreparednessId);

            return Result<EmergencyPreparednessActionResult>.Failure(new ApplicationError(
                "EmployeeSafety.EmergencyPreparedness.ConductDrillFailed",
                "Unable to ConductDrill the EmergencyPreparedness record at this time.",
                Guid.NewGuid()));
        }
    }
}