// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for CompleteCraneInspectionCommand.
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

/// <summary>Handles <see cref="CompleteCraneInspectionCommand"/>.</summary>
public sealed class CompleteCraneInspectionCommandHandler : IRequestHandler<CompleteCraneInspectionCommand, Result<CraneHoistSystemsActionResult>>
{
    private readonly ICraneHoistSystemsRepository _repository;
    private readonly ILogger<CompleteCraneInspectionCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CompleteCraneInspectionCommandHandler(ICraneHoistSystemsRepository repository, ILogger<CompleteCraneInspectionCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CraneHoistSystemsActionResult>> Handle(CompleteCraneInspectionCommand request, CancellationToken cancellationToken)
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

            aggregate.CompleteCraneInspection(
                request.Passed,
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
            _logger.LogError(ex, "Failed to execute CompleteCraneInspection for CraneHoistSystems {CraneHoistSystemsId}", request.CraneHoistSystemsId);

            return Result<CraneHoistSystemsActionResult>.Failure(new ApplicationError(
                "MachineSafety.CraneHoistSystems.CompleteCraneInspectionFailed",
                "Unable to CompleteCraneInspection the CraneHoistSystems record at this time.",
                Guid.NewGuid()));
        }
    }
}
