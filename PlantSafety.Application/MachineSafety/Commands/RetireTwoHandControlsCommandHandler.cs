// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handler for RetireTwoHandControlsCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.MachineSafety.TwoHandControls.Interfaces;
using PlantSafety.Domain.MachineSafety.TwoHandControls.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.MachineSafety.Commands;

/// <summary>Handles <see cref="RetireTwoHandControlsCommand"/>.</summary>
public sealed class RetireTwoHandControlsCommandHandler : IRequestHandler<RetireTwoHandControlsCommand, Result<TwoHandControlsActionResult>>
{
    private readonly ITwoHandControlsRepository _repository;
    private readonly ILogger<RetireTwoHandControlsCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RetireTwoHandControlsCommandHandler(ITwoHandControlsRepository repository, ILogger<RetireTwoHandControlsCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<TwoHandControlsActionResult>> Handle(RetireTwoHandControlsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = TwoHandControlsId.FromGuid(request.TwoHandControlsId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
            {
                return Result<TwoHandControlsActionResult>.Failure(new ApplicationError(
                    "MachineSafety.TwoHandControls.NotFound",
                    "The requested TwoHandControls record could not be found.",
                    Guid.NewGuid()));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.Retire(
                UtcTimestamp.Now(),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<TwoHandControlsActionResult>.Success(new TwoHandControlsActionResult(
                aggregate.TwoHandControlsId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute Retire for TwoHandControls {TwoHandControlsId}", request.TwoHandControlsId);

            return Result<TwoHandControlsActionResult>.Failure(new ApplicationError(
                "MachineSafety.TwoHandControls.RetireFailed",
                "Unable to Retire the TwoHandControls record at this time.",
                Guid.NewGuid()));
        }
    }
}