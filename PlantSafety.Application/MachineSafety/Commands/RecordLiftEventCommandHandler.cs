// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for RecordLiftEventCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.MachineSafety.CraneHoistSystems.Interfaces;
using PlantSafety.Domain.MachineSafety.CraneHoistSystems.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.MachineSafety.Commands;

/// <summary>Handles <see cref="RecordLiftEventCommand"/>.</summary>
public sealed class RecordLiftEventCommandHandler : IRequestHandler<RecordLiftEventCommand, Result<CraneHoistSystemsActionResult>>
{
    private readonly ICraneHoistSystemsRepository _repository;
    private readonly ILogger<RecordLiftEventCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RecordLiftEventCommandHandler(ICraneHoistSystemsRepository repository, ILogger<RecordLiftEventCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CraneHoistSystemsActionResult>> Handle(RecordLiftEventCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = CraneHoistSystemsId.FromGuid(request.CraneHoistSystemsId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
            {
                return Result<CraneHoistSystemsActionResult>.Failure(new ApplicationError(
                    "MachineSafety.CraneHoistSystems.NotFound",
                    "The requested CraneHoistSystems record could not be found.",
                    Guid.NewGuid(),
                    ErrorSeverity.NotFound));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.RecordLiftEvent(
                request.ObservedLoadKg,
                UtcTimestamp.Now(),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<CraneHoistSystemsActionResult>.Success(new CraneHoistSystemsActionResult(
                aggregate.CraneHoistSystemsId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute RecordLiftEvent for CraneHoistSystems {CraneHoistSystemsId}", request.CraneHoistSystemsId);

            return Result<CraneHoistSystemsActionResult>.Failure(new ApplicationError(
                "MachineSafety.CraneHoistSystems.RecordLiftEventFailed",
                "Unable to RecordLiftEvent the CraneHoistSystems record at this time.",
                Guid.NewGuid()));
        }
    }
}
