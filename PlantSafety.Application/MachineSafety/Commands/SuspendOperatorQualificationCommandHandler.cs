// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for SuspendOperatorQualificationCommand.
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

/// <summary>Handles <see cref="SuspendOperatorQualificationCommand"/>.</summary>
public sealed class SuspendOperatorQualificationCommandHandler : IRequestHandler<SuspendOperatorQualificationCommand, Result<CraneHoistSystemsActionResult>>
{
    private readonly ICraneHoistSystemsRepository _repository;
    private readonly ILogger<SuspendOperatorQualificationCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public SuspendOperatorQualificationCommandHandler(ICraneHoistSystemsRepository repository, ILogger<SuspendOperatorQualificationCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CraneHoistSystemsActionResult>> Handle(SuspendOperatorQualificationCommand request, CancellationToken cancellationToken)
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

            aggregate.SuspendOperatorQualification(
                request.Reason,
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
            _logger.LogError(ex, "Failed to execute SuspendOperatorQualification for CraneHoistSystems {CraneHoistSystemsId}", request.CraneHoistSystemsId);

            return Result<CraneHoistSystemsActionResult>.Failure(new ApplicationError(
                "MachineSafety.CraneHoistSystems.SuspendOperatorQualificationFailed",
                "Unable to SuspendOperatorQualification for the CraneHoistSystems record at this time.",
                Guid.NewGuid()));
        }
    }
}
