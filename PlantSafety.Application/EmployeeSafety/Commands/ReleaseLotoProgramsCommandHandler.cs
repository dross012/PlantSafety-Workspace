// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handler for ReleaseLotoProgramsCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.EmployeeSafety.LotoPrograms.Interfaces;
using PlantSafety.Domain.EmployeeSafety.LotoPrograms.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.EmployeeSafety.Commands;

/// <summary>Handles <see cref="ReleaseLotoProgramsCommand"/>.</summary>
public sealed class ReleaseLotoProgramsCommandHandler : IRequestHandler<ReleaseLotoProgramsCommand, Result<LotoProgramsActionResult>>
{
    private readonly ILotoProgramsRepository _repository;
    private readonly ILogger<ReleaseLotoProgramsCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ReleaseLotoProgramsCommandHandler(ILotoProgramsRepository repository, ILogger<ReleaseLotoProgramsCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<LotoProgramsActionResult>> Handle(ReleaseLotoProgramsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = LotoProgramsId.FromGuid(request.LotoProgramsId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
            {
                return Result<LotoProgramsActionResult>.Failure(new ApplicationError(
                    "EmployeeSafety.LotoPrograms.NotFound",
                    "The requested LotoPrograms record could not be found.",
                    Guid.NewGuid()));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.Release(
                UserId.FromGuid(request.ReleasedByUserId),
                UtcTimestamp.Now(),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<LotoProgramsActionResult>.Success(new LotoProgramsActionResult(
                aggregate.LotoProgramsId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute Release for LotoPrograms {LotoProgramsId}", request.LotoProgramsId);

            return Result<LotoProgramsActionResult>.Failure(new ApplicationError(
                "EmployeeSafety.LotoPrograms.ReleaseFailed",
                "Unable to Release the LotoPrograms record at this time.",
                Guid.NewGuid()));
        }
    }
}