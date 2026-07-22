// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handler for FlagPPEInventoryForReplacementCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.PPE.PPEInventory.Interfaces;
using PlantSafety.Domain.PPE.PPEInventory.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.PPE.Commands;

/// <summary>Handles <see cref="FlagPPEInventoryForReplacementCommand"/>.</summary>
public sealed class FlagPPEInventoryForReplacementCommandHandler : IRequestHandler<FlagPPEInventoryForReplacementCommand, Result<PPEInventoryActionResult>>
{
    private readonly IPPEInventoryRepository _repository;
    private readonly ILogger<FlagPPEInventoryForReplacementCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public FlagPPEInventoryForReplacementCommandHandler(IPPEInventoryRepository repository, ILogger<FlagPPEInventoryForReplacementCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PPEInventoryActionResult>> Handle(FlagPPEInventoryForReplacementCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = PPEInventoryId.FromGuid(request.PPEInventoryId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
            {
                return Result<PPEInventoryActionResult>.Failure(new ApplicationError(
                    "PPE.PPEInventory.NotFound",
                    "The requested PPE inventory item could not be found.",
                    Guid.NewGuid()));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.FlagForReplacement(
                reason: request.Reason,
                flaggedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<PPEInventoryActionResult>.Success(new PPEInventoryActionResult(
                aggregate.PPEInventoryId.Value,
                aggregate.Status.ToString(),
                aggregate.AvailableQuantity));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to flag PPEInventory {PPEInventoryId} for replacement", request.PPEInventoryId);

            return Result<PPEInventoryActionResult>.Failure(new ApplicationError(
                "PPE.PPEInventory.FlagForReplacementFailed",
                "Unable to flag the PPE item for replacement at this time.",
                Guid.NewGuid()));
        }
    }
}
