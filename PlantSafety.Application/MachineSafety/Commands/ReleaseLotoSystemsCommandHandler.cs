// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handler for ReleaseLotoSystemsCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.MachineSafety.LotoSystems.Interfaces;
using PlantSafety.Domain.MachineSafety.LotoSystems.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.MachineSafety.Commands;

/// <summary>Handles <see cref="ReleaseLotoSystemsCommand"/>.</summary>
public sealed class ReleaseLotoSystemsCommandHandler : IRequestHandler<ReleaseLotoSystemsCommand, Result<LotoSystemsActionResult>>
{
    private readonly ILotoSystemsRepository _repository;
    private readonly ILogger<ReleaseLotoSystemsCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ReleaseLotoSystemsCommandHandler(ILotoSystemsRepository repository, ILogger<ReleaseLotoSystemsCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<LotoSystemsActionResult>> Handle(ReleaseLotoSystemsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = LotoSystemsId.FromGuid(request.LotoSystemsId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
            {
                return Result<LotoSystemsActionResult>.Failure(new ApplicationError(
                    "MachineSafety.LotoSystems.NotFound",
                    "The requested LotoSystems record could not be found.",
                    Guid.NewGuid()));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.Release(
                UtcTimestamp.Now(),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<LotoSystemsActionResult>.Success(new LotoSystemsActionResult(
                aggregate.LotoSystemsId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute Release for LotoSystems {LotoSystemsId}", request.LotoSystemsId);

            return Result<LotoSystemsActionResult>.Failure(new ApplicationError(
                "MachineSafety.LotoSystems.ReleaseFailed",
                "Unable to Release the LotoSystems record at this time.",
                Guid.NewGuid()));
        }
    }
}