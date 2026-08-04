// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for RecordRiggingInspectionCommand.
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

/// <summary>Handles <see cref="RecordRiggingInspectionCommand"/>.</summary>
public sealed class RecordRiggingInspectionCommandHandler : IRequestHandler<RecordRiggingInspectionCommand, Result<CraneHoistSystemsActionResult>>
{
    private readonly ICraneHoistSystemsRepository _repository;
    private readonly ILogger<RecordRiggingInspectionCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RecordRiggingInspectionCommandHandler(ICraneHoistSystemsRepository repository, ILogger<RecordRiggingInspectionCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CraneHoistSystemsActionResult>> Handle(RecordRiggingInspectionCommand request, CancellationToken cancellationToken)
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
                    Guid.NewGuid()));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.RecordRiggingInspection(
                request.DefectDetected,
                request.Notes ?? string.Empty,
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
            _logger.LogError(ex, "Failed to execute RecordRiggingInspection for CraneHoistSystems {CraneHoistSystemsId}", request.CraneHoistSystemsId);

            return Result<CraneHoistSystemsActionResult>.Failure(new ApplicationError(
                "MachineSafety.CraneHoistSystems.RecordRiggingInspectionFailed",
                "Unable to RecordRiggingInspection for the CraneHoistSystems record at this time.",
                Guid.NewGuid()));
        }
    }
}
